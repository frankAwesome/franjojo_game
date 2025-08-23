using UnityEngine;
using UnityEngine.Events;

public class SpeechManager : MonoBehaviour
{
    public static SpeechManager Instance;

    public UnityEvent<string> OnSpeechComplete;

    public string RecordedSpeech = "";
    public AudioRecorder AudioRecorder;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleReadyToListen(string tip)
    {
        IsReadyForRecording = true;
        
        RecordedSpeech = "";
        UIManager.Instance.ToggleVoiceButton(true, tip); // Show the voice button when ready
        AudioRecorder.OnStart();
    }


    public bool IsReadyForRecording = false;

    public void LogMessageReceived(string message)
    {
        
    }

    public void OnPartialResultReceived(string message)
    {
        if (!IsReadyForRecording) return; // Prevent processing if not ready
        UIManager.Instance.SetChatText(message, false); // Update chat text with partial result
    }

    public void OnResultRecieved(string message)
    {
        if (!IsReadyForRecording) return; // Prevent processing if not ready
        AudioRecorder.OnStop();

        IsReadyForRecording = false;
        UIManager.Instance.SetChatText(message, true);
        RecordedSpeech = message; // Store the final recorded speech

        //UIManager.Instance.ToggleVoiceButton(false); // Hide the voice button after receiving a result
        //OnSpeechComplete?.Invoke(message);
    }

    public void AcceptSpeech()
    {
        UIManager.Instance.ToggleVoiceButton(false, ""); // Hide the voice button after accepting speech
        OnSpeechComplete?.Invoke(RecordedSpeech);       
    }

    public void RetrySpeech()
    {
        
        IsReadyForRecording = true; // Reset the state to allow for new recording
        RecordedSpeech = ""; // Clear the recorded speech
        UIManager.Instance.ToggleVoiceButton(true, "Say something."); // Show the voice button again
        AudioRecorder.OnStart();
    }
}
