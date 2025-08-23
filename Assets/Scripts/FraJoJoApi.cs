using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Events;

public class FraJoJoApi : MonoBehaviour
{
    public string BearerToken;

    public static FraJoJoApi Instance;
    public string BaseUrl = "";

    public UnityAction<GetDialogResponseModel> OnDialogResponseReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {        
    }

    public void GetGameStorieParams(int activeChapterId, string msg)
    {
        var body = new GetDialogRequestModel
        {
            activeChapterId = activeChapterId,
            completedChapterIds = new List<int>(),
            milestones = new List<int>(),
            playerQuestion = msg
        };

        var headers = new (string, string)[]
        {
            ("Authorization", "Bearer " + BearerToken),            
        };

        Debug.Log("Starting API call with URL: " + BaseUrl + "v1/getDialog/1/1");

        StartCoroutine(PostJson<GetDialogRequestModel, GetDialogResponseModel>(BaseUrl + "v1/getDialog/1/1", body, headers, onSuccess: suc =>
        {
            Debug.Log("Response received: " + suc.response?.dialogResponse);
            OnDialogResponseReceived?.Invoke(suc);
        }, onFail: (code, error, txt) =>
        {
            Debug.LogError($"Error: {code}, {error}, Response: {txt}");
        }, 5));
        
    }

    public static IEnumerator PostJson<TReq, TRes>(string url, TReq payload, (string, string)[] headers = null, Action<TRes> onSuccess = null,
        Action<long, string, string> onFail = null,
        int timeoutSeconds = 15)
    {
        var json = JsonUtility.ToJson(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (headers != null)
            {
                foreach (var (k, v) in headers)
                {
                    req.SetRequestHeader(k, v);
                }
            }

            req.timeout = timeoutSeconds;

            Debug.Log($"Sending POST request to {url} with payload: {json}");
            yield return req.SendWebRequest();

            bool ok = req.result == UnityWebRequest.Result.Success;
            Debug.Log($"Request completed with status: {req.result}, response code: {req.responseCode}");

            if (!ok)
            {
                Debug.LogError($"Request failed: {req.error}, response code: {req.responseCode}, response text: {req.downloadHandler?.text}");
                onFail?.Invoke(req.responseCode, req.error, req.downloadHandler?.text);
                yield break;
            }

            var text = req.downloadHandler.text;

            try
            {
                Debug.Log($"Response text: {text}");
                var data = JsonUtility.FromJson<TRes>(text);                                
                onSuccess?.Invoke(data);
            }
            catch (Exception ex)
            {                
                Debug.LogError($"Failed to parse response: {ex.Message}, response text: {text}");
                onFail?.Invoke(req.responseCode, ex.Message, text);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
