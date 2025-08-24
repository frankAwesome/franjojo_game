using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SpeechRecognitionSystem;

using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class SpeechRecognizer : MonoBehaviour
{
    public string LanguageModelDirPath = "SpeechRecognitionSystem/model/english_small";

    public void OnDataProviderReady(IAudioProvider audioProvider)
    {
        _audioProvider = audioProvider;
    }

    [Serializable]
    public class MessageEvent : UnityEvent<string> { }

    public MessageEvent LogMessageReceived = new MessageEvent();
    public MessageEvent PartialResultReceived = new MessageEvent();
    public MessageEvent ResultReceived = new MessageEvent();

    private void onLanguageModelCopyComplete(string modelDirPath)
    {
        if (!string.IsNullOrEmpty(modelDirPath))
        {
            _languageModelWasCopied = Directory.Exists(modelDirPath);
            _absoluteLanguageModelDirPath = modelDirPath;
        }
        else
        {
            LogMessageReceived?.Invoke("Error on copying streaming assets");
        }
    }

    #region initialization management
    private void tryDeinitSpeechRecognizer()
    {
        var languageModelNeed2Update = !_absoluteLanguageModelDirPath.Contains(LanguageModelDirPath);
        var frequencyNeed2Update = _audioProvider != null &&
                                   _sr != null &&
                                   _sr.Frequency != _audioProvider.Frequency;

        if (languageModelNeed2Update)
        {
            _languageModelWasCopied = false;
            _copyRequested = false;
        }
        if (languageModelNeed2Update || frequencyNeed2Update)
        {
            _init = false;
            _running = false;

            if (_sr != null)
            {
                _sr.Dispose();
                _sr = null;
            }
        }
    }

    private void tryToInitLanguageModel()
    {
        if (!_languageModelWasCopied)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (!_copyRequested)
                {
                    copyAssets2ExternalStorage(LanguageModelDirPath);
                    _copyRequested = true;
                }
            }
            else
            {
                onLanguageModelCopyComplete(Application.streamingAssetsPath + "/" + LanguageModelDirPath);
            }
        }
    }

    private void tryToInitSpeechRecognizer()
    {
        if (!_init &&
            _languageModelWasCopied &&
            _audioProvider != null)
        {
            if (_sr == null)
            {
                _sr = new SpeechRecognitionSystem.SpeechRecognizer();
            }
            _sr.Frequency = _audioProvider.Frequency;

            _init = _sr.Init(_absoluteLanguageModelDirPath);

            if (_init)
            {
                LogMessageReceived?.Invoke("The SRS plugin is ready to work...");

                _running = true;
                _cts = new CancellationTokenSource();
                // background loop feeds audio & signals result polls (no Unity API calls here)
                Task.Run(processing, _cts.Token).ConfigureAwait(false);
            }
            else
            {
                LogMessageReceived?.Invoke("Error on init SRS plugin. Check 'Language model dir path'\n" + _absoluteLanguageModelDirPath);
            }
        }
    }
    #endregion

    private void onReceiveLogMess(string message)
    {
        LogMessageReceived?.Invoke(message);
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
    }

    private void Update()
    {
        tryDeinitSpeechRecognizer();
        tryToInitLanguageModel();
        tryToInitSpeechRecognizer();

        if (_audioProvider is AudioRecorder mic)
        {
            micIsRecording = mic.IsRecording();
        }
        else if (_audioProvider is AudioPlayer)
        {
            micIsRecording = true;
        }

        if (_init && _audioProvider != null)
        {
            // Gather audio on main thread; processed by worker
            var audioData = _audioProvider.GetData();
            if (audioData != null)
            {
                _threadedBufferQueue.Enqueue(audioData);
            }

            // MAIN THREAD: perform plugin result polling & enqueue strings
            while (_resultSignalQueue.TryDequeue(out int sig))
            {
                try
                {
                    if (sig == 0) // partial
                    {
                        string part;
                        lock (_srLock)
                        {
                            part = _sr?.GetPartialResult()?.partial;
                        }
                        if (!string.IsNullOrEmpty(part))
                            _recognitionPartialResultsQueue.Enqueue(part);
                    }
                    else // final
                    {
                        string final;
                        lock (_srLock)
                        {
                            final = _sr?.GetResult()?.text;
                        }
                        if (!string.IsNullOrEmpty(final))
                            _recognitionFinalResultsQueue.Enqueue(final);
                    }
                }
                catch (ThreadAbortException) { /* domain reload */ }
                catch (Exception ex)
                {
                    Debug.LogError($"[SpeechRecognizer] Result poll failed: {ex.Message}");
                }
            }

            // Drain text queues → fire UnityEvents (main thread)
            if (_recognitionPartialResultsQueue.TryDequeue(out string partOut))
            {
                if (partOut != string.Empty)
                    PartialResultReceived?.Invoke(partOut);
            }
            if (_recognitionFinalResultsQueue.TryDequeue(out string resultOut))
            {
                if (resultOut != string.Empty)
                    ResultReceived?.Invoke(resultOut);
            }
        }
    }

    bool micIsRecording = false;

    private async Task processing()
    {
        var token = _cts?.Token ?? CancellationToken.None;

        try
        {
            while (_running && !token.IsCancellationRequested)
            {
                if (micIsRecording)
                {
                    if (_threadedBufferQueue.TryDequeue(out float[] audioData))
                    {
                        int resultReady;
                        lock (_srLock)
                        {
                            // Background thread feeds audio only
                            resultReady = _sr.AppendAudioData(audioData);
                        }

                        // Signal the main thread to poll plugin for text
                        if (resultReady == 0)
                            _resultSignalQueue.Enqueue(0); // partial
                        else
                            _resultSignalQueue.Enqueue(1); // final (or anything non-zero)
                    }
                    else
                    {
                        await Task.Delay(10, token);
                    }
                }
                else
                {
                    // Not recording: occasionally poll both on main thread
                    _resultSignalQueue.Enqueue(0);
                    _resultSignalQueue.Enqueue(1);
                    await Task.Delay(50, token);
                }
            }
        }
        catch (OperationCanceledException) { /* expected on shutdown / reload */ }
        catch (ThreadAbortException) { /* expected on domain reload */ }
        catch (Exception ex)
        {
            Debug.LogError($"[SpeechRecognizer] processing error: {ex}");
        }
    }

    private void OnDisable()
    {
        CancelAndStop();
    }

    private void OnDestroy()
    {
        CancelAndStop();
        tryDeinitSpeechRecognizer();
    }

    // Called from editor helper before domain reload
    public void CancelFromEditorReload() => CancelAndStop();

    private void CancelAndStop()
    {
        _running = false;
        try { _cts?.Cancel(); } catch { }
        _cts = null;
    }

    private void copyAssets2ExternalStorage(string modelDirPath)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var recognizerActivity = new AndroidJavaObject("com.sss.unity_asset_manager.MainActivity", currentActivity);
            recognizerActivity.CallStatic("setReceiverObjectName", this.gameObject.name);
            recognizerActivity.CallStatic("setLogReceiverMethodName", "onReceiveLogMess");
            recognizerActivity.CallStatic("setOnCopyingCompleteMethod", "onLanguageModelCopyComplete");

            LogMessageReceived?.Invoke("Please wait until the files of language model are copied...");
            recognizerActivity.Call("tryCopyStreamingAssets2ExternalStorage", modelDirPath);
        }
    }

    private SpeechRecognitionSystem.SpeechRecognizer _sr = null;
    private IAudioProvider _audioProvider = null;
    private bool _init = false;
    private bool _copyRequested = false;

    private readonly ConcurrentQueue<float[]> _threadedBufferQueue = new ConcurrentQueue<float[]>();
    private readonly ConcurrentQueue<string> _recognitionPartialResultsQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<string> _recognitionFinalResultsQueue = new ConcurrentQueue<string>();

    // Signals from worker → main thread (0 = partial, 1 = final)
    private readonly ConcurrentQueue<int> _resultSignalQueue = new ConcurrentQueue<int>();

    private readonly object _srLock = new object();

    private bool _languageModelWasCopied = false;
    private string _absoluteLanguageModelDirPath = string.Empty;

    private bool _running = false;
    private CancellationTokenSource _cts;
}





//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//using SpeechRecognitionSystem;

//using UnityEngine;
//using UnityEngine.Android;
//using UnityEngine.Events;

//internal class SpeechRecognizer : MonoBehaviour {
//    public string LanguageModelDirPath = "SpeechRecognitionSystem/model/english_small";

//    public void OnDataProviderReady( IAudioProvider audioProvider ) {
//        _audioProvider = audioProvider;
//    }

//    [System.Serializable]
//    public class MessageEvent : UnityEvent<string> { }

//    public MessageEvent LogMessageReceived = new MessageEvent( );
//    public MessageEvent PartialResultReceived = new MessageEvent( );
//    public MessageEvent ResultReceived = new MessageEvent( );

//    private void onLanguageModelCopyComplete( string modelDirPath ) {
//        if ( !String.IsNullOrEmpty( modelDirPath ) ) {
//            _languageModelWasCopied = Directory.Exists( modelDirPath );
//            _absoluteLanguageModelDirPath = modelDirPath;
//        }
//        else {
//            LogMessageReceived?.Invoke( "Error on copying streaming assets" );
//        }
//    }

//    #region initialization management
//    private void tryDeinitSpeechRecognizer( ) {
//        var languageModelNeed2Update = !_absoluteLanguageModelDirPath.Contains( LanguageModelDirPath );
//        var frequencyNeed2Update = _audioProvider != null &&
//            _sr != null &&
//            _sr.Frequency != _audioProvider.Frequency;

//        if ( languageModelNeed2Update ) {
//            _languageModelWasCopied = false;
//            _copyRequested = false;
//        }
//        if ( languageModelNeed2Update || frequencyNeed2Update ) {
//            _init = false;
//            _running = false;

//            if ( _sr != null ) {
//                _sr.Dispose( );
//                _sr = null;
//            }
//        }
//    }

//    private void tryToInitLanguageModel( ) {
//        if ( !_languageModelWasCopied ) {
//            if ( Application.platform == RuntimePlatform.Android ) {
//                if ( !_copyRequested ) {
//                    copyAssets2ExternalStorage( LanguageModelDirPath );
//                    _copyRequested = true;
//                }
//            }
//            else {
//                onLanguageModelCopyComplete( Application.streamingAssetsPath + "/" + LanguageModelDirPath );
//            }
//        }
//    }

//    private void tryToInitSpeechRecognizer( ) {
//        if ( !_init && 
//            _languageModelWasCopied &&
//            _audioProvider != null ) {

//            if ( _sr == null ) {
//                _sr = new SpeechRecognitionSystem.SpeechRecognizer( );
//            }
//            _sr.Frequency = _audioProvider.Frequency;

//            _init = _sr.Init( _absoluteLanguageModelDirPath );

//            if ( _init ) {
//                LogMessageReceived?.Invoke( "The SRS plugin is ready to work..." );

//                _running = true;
//                Task.Run( processing ).ConfigureAwait( false );
//            }
//            else {
//                LogMessageReceived?.Invoke( "Error on init SRS plugin. Check 'Language model dir path'\n" + _absoluteLanguageModelDirPath );
//            }
//        }
//    }
//    #endregion

//    private void onReceiveLogMess( string message ) {
//        LogMessageReceived?.Invoke( message );
//    }

//    private void Update( ) {
//        tryDeinitSpeechRecognizer( );

//        tryToInitLanguageModel( );

//        tryToInitSpeechRecognizer( );

//        if ( _audioProvider is AudioRecorder mic ) {
//            micIsRecording = mic.IsRecording( );
//        }
//        else if ( _audioProvider is AudioPlayer player ) {
//            micIsRecording = true;
//        }

//        if ( _init && _audioProvider != null ) {
//            var audioData = _audioProvider.GetData( );
//            if ( audioData != null ) {
//                _threadedBufferQueue.Enqueue( audioData );   
//            }

//            if ( _recognitionPartialResultsQueue.TryDequeue( out string part ) ) {
//                if ( part != string.Empty )
//                    PartialResultReceived?.Invoke( part );
//            }
//            if ( _recognitionFinalResultsQueue.TryDequeue( out string result ) ) {
//                if ( result != string.Empty )
//                    ResultReceived?.Invoke( result );
//            }
//        }
//    }

//    bool micIsRecording = false;

//    private async Task processing( ) {
//        while ( _running ) {
//            if ( micIsRecording ) {
//                float[ ] audioData;
//                var isOk = _threadedBufferQueue.TryDequeue( out audioData );
//                if ( isOk ) {
//                    int resultReady = _sr.AppendAudioData( audioData );
//                    if ( resultReady == 0 ) {
//                        _recognitionPartialResultsQueue.Enqueue( _sr.GetPartialResult( )?.partial );
//                    }
//                    else {
//                        _recognitionFinalResultsQueue.Enqueue( _sr.GetResult( )?.text );
//                    }
//                }
//                else {
//                    await Task.Delay( 10 );
//                }
//            }
//            else {
//                _sr.GetPartialResult( );
//                _sr.GetResult( );
//            }
//        }
//    }

//    private void OnDestroy( ) {
//        tryDeinitSpeechRecognizer( );
//    }

//    private void copyAssets2ExternalStorage( string modelDirPath ) {
//        if ( Application.platform == RuntimePlatform.Android ) {
//            var javaUnityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
//            var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" );
//            var recognizerActivity = new AndroidJavaObject( "com.sss.unity_asset_manager.MainActivity", currentActivity );
//            recognizerActivity.CallStatic( "setReceiverObjectName", this.gameObject.name );
//            recognizerActivity.CallStatic( "setLogReceiverMethodName", "onReceiveLogMess" );
//            recognizerActivity.CallStatic( "setOnCopyingCompleteMethod", "onLanguageModelCopyComplete" );

//            LogMessageReceived?.Invoke( "Please wait until the files of language model are copied..." );
//            recognizerActivity.Call( "tryCopyStreamingAssets2ExternalStorage", modelDirPath );
//        }
//    }

//    private SpeechRecognitionSystem.SpeechRecognizer _sr = null;
//    private IAudioProvider _audioProvider = null;
//    private bool _init = false;
//    private bool _copyRequested = false;

//    private readonly ConcurrentQueue<float[ ]> _threadedBufferQueue = new ConcurrentQueue<float[ ]>( );
//    private readonly ConcurrentQueue<string> _recognitionPartialResultsQueue = new ConcurrentQueue<string>( );
//    private readonly ConcurrentQueue<string> _recognitionFinalResultsQueue = new ConcurrentQueue<string>( );

//    private bool _languageModelWasCopied = false;
//    private string _absoluteLanguageModelDirPath = string.Empty;

//    private bool _running = false;
//}
