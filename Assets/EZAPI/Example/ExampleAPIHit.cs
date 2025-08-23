using EzAPI.RunTime.UserAccessible;
using TMPro;
using UnityEngine;
namespace EzAPI
{
    namespace Example
    {
        public class ExampleAPIHit : MonoBehaviour
        {
            [SerializeField] private TextMeshProUGUI textProgress;
            [SerializeField] private TextMeshProUGUI responseText;

            public void APIHitPublic()
            {
                APIManager.Instance.HitAPI<RequestPayloadBase, RequestResponseBase>(EndPoints.Com, null, null, ResponseListener, Progress);
            }
            public void APIHitPublic2()
            {
                APIClass aPIClass = new APIClass(EndPoints.Com, null);

                aPIClass.AddResponseListener(ResponseListener);
                aPIClass.AddProgressListener(Progress);
                aPIClass.HitAPI();
            }

            private void ResponseListener<T>(T response) where T : RequestResponseBase
            {
                if (response.success)
                {
                    Debug.Log($"API Hit is successful, got json Data as  {JsonUtility.ToJson(response)}");
                    responseText.text = $"API Hit is successful, got json Data as  {JsonUtility.ToJson(response)}";
                }
                else
                {
                    if (response.responseCode == -1)
                    {
                        // This Means Error is due to Network
                        Debug.Log($"API Hit has Failed,\n Response Code is {response.responseCode} \n Failure Message Is {response.failureMessage} \n Json Is {JsonUtility.ToJson(response)}");
                        responseText.text = $"API Hit has Failed,\n Response Code is {response.responseCode} \n Failure Message Is {response.failureMessage} \n Json Is {JsonUtility.ToJson(response)}";
                    }
                    else if (response.responseCode == -2)
                    {
                        // This Means Error is due to API HIT METHOD CALL
                        Debug.Log($"API Hit has Failed,\n Response Code is  {response.responseCode} \n Failure Message Is {response.failureMessage} \n Json Is {JsonUtility.ToJson(response)}");
                        responseText.text = $"API Hit has Failed,\n Response Code is  {response.responseCode} \n Failure Message Is {response.failureMessage} \n Json Is {JsonUtility.ToJson(response)}";
                    }
                    else
                    {
                        // This Means Error has come from backend
                        Debug.Log($"API Hit has Failed,\n Response Code is {response.responseCode} \n Failure Message Is {response.failureMessage} \n Json Is {JsonUtility.ToJson(response)}");
                        responseText.text = $"API Hit has Failed,\n Response Code is {response.responseCode} \n Failure Message Is {response.failureMessage} \n Json Is {JsonUtility.ToJson(response)}";
                    }
                }
            }

            private void Progress(float value)
            {
                textProgress.text = $"API Hit is in Progress {value}";
            }
        }
    }
}