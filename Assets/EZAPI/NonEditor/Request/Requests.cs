#define Debug
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EzAPI
{
    namespace RunTime
    {
        /// <summary>
        /// Contains methods for sending HTTP requests using Unity's UnityWebRequest.
        /// </summary>
        public static class Requests
        {
            /// <summary>
            /// Sends a POST request to the specified route.
            /// </summary>
            /// <param name="route">The route to send the request to.</param>
            /// <param name="dataType">The data type (Json or Form) to be sent.</param>
            /// <param name="jsonData">The data to send in the request body.</param>
            /// <param name="requestTimeout">The timeout duration for the request.</param>
            /// <param name="headerKeysAndValues">The headers to include in the request.</param>
            /// <param name="onSuccess">Callback function to be invoked on a successful response.</param>
            /// <param name="onFailure">Callback function to be invoked on failure response.</param>
            /// <param name="onConnectionError">Callback function to be invoked on connection error.</param>
            /// <returns>The UnityWebRequestAsyncOperation representing the request.</returns>
            public static UnityWebRequestAsyncOperation Post(string route, DataType dataType, string jsonData, int requestTimeout, List<HeaderKeysAndValue> headerKeysAndValues, Action<string> onSuccess = null, Action<int, string> onFailure = null, Action<int> onConnectionError = null)
            {
                #region Debug

                string headersAre = "";
                foreach (var item in headerKeysAndValues)
                {
                    headersAre += $"{item.key} : {item.value} ,";
                }
                Debug.Log($"Hitting [POST] ::API:: {route} ::SendingData:: {jsonData} :: Headers Are {headersAre}");

                #endregion Debug

                byte[] bite = GetBites(jsonData, dataType);
                UnityWebRequest request = new UnityWebRequest(route, UnityWebRequest.kHttpVerbPOST)
                {
                    uploadHandler = new UploadHandlerRaw(bite),
                    downloadHandler = new DownloadHandlerBuffer(),
                };
                return CommonCallBack(request, route, headerKeysAndValues, onSuccess, onFailure, onConnectionError, requestTimeout);
            }

            /// <summary>
            /// Sends a PUT request to the specified route.
            /// </summary>
            /// <param name="route">The route to send the request to.</param>
            /// <param name="dataType">The data type (Json or Form) to be sent.</param>
            /// <param name="jsonData">The data to send in the request body.</param>
            /// <param name="requestTimeout">The timeout duration for the request.</param>
            /// <param name="headerKeysAndValues">The headers to include in the request.</param>
            /// <param name="onSuccess">Callback function to be invoked on a successful response.</param>
            /// <param name="onFailure">Callback function to be invoked on failure response.</param>
            /// <param name="onConnectionError">Callback function to be invoked on connection error.</param>
            /// <returns>The UnityWebRequestAsyncOperation representing the request.</returns>
            public static UnityWebRequestAsyncOperation PUT(string route, DataType dataType, string jsonData, int requestTimeout, List<HeaderKeysAndValue> headerKeysAndValues, Action<string> onSuccess = null, Action<int, string> onFailure = null, Action<int> onConnectionError = null)
            {
                #region Debug

                string headersAre = "";
                foreach (var item in headerKeysAndValues)
                {
                    headersAre += $"{item.key} : {item.value} ,";
                }
                Debug.Log($"Hitting [PUT] ::API:: {route} ::SendingData:: {jsonData} :: Headers Are {headersAre}");

                #endregion Debug

                byte[] bite = GetBites(jsonData, dataType);
                UnityWebRequest request = new UnityWebRequest(route, UnityWebRequest.kHttpVerbPUT)
                {
                    uploadHandler = new UploadHandlerRaw(bite),
                    downloadHandler = new DownloadHandlerBuffer(),
                };
                return CommonCallBack(request, route, headerKeysAndValues, onSuccess, onFailure, onConnectionError, requestTimeout);
            }

            /// <summary>
            /// Sends a DELETE request to the specified route.
            /// </summary>
            /// <param name="route">The route to send the request to.</param>
            /// <param name="dataType">The data type (Json or Form) to be sent.</param>
            /// <param name="jsonData">The data to send in the request body.</param>
            /// <param name="requestTimeout">The timeout duration for the request.</param>
            /// <param name="headerKeysAndValues">The headers to include in the request.</param>
            /// <param name="onSuccess">Callback function to be invoked on a successful response.</param>
            /// <param name="onFailure">Callback function to be invoked on failure response.</param>
            /// <param name="onConnectionError">Callback function to be invoked on connection error.</param>
            /// <returns>The UnityWebRequestAsyncOperation representing the request.</returns>
            public static UnityWebRequestAsyncOperation Delete(string route, DataType dataType, string jsonData, int requestTimeout, List<HeaderKeysAndValue> headerKeysAndValues, Action<string> onSuccess = null, Action<int, string> onFailure = null, Action<int> onConnectionError = null)
            {
                #region Debug

                string headersAre = "";
                foreach (var item in headerKeysAndValues)
                {
                    headersAre += $"{item.key} : {item.value} ,";
                }
                Debug.Log($"Hitting [DELETE] ::API:: {route} ::SendingData:: {jsonData} :: Headers Are {headersAre}");

                #endregion Debug

                byte[] bite = GetBites(jsonData, dataType);
                UnityWebRequest request = new UnityWebRequest(route, UnityWebRequest.kHttpVerbDELETE)
                {
                    uploadHandler = new UploadHandlerRaw(bite),
                    downloadHandler = new DownloadHandlerBuffer(),
                };
                return CommonCallBack(request, route, headerKeysAndValues, onSuccess, onFailure, onConnectionError, requestTimeout);
            }

            /// <summary>
            /// Sends a GET request to the specified route.
            /// </summary>
            /// <param name="route">The route to send the request to.</param>
            /// <param name="dataType">The data type (Json or Form) to be sent.</param>
            /// <param name="jsonData">The data to send in the request body.</param>
            /// <param name="requestTimeout">The timeout duration for the request.</param>
            /// <param name="headerKeysAndValues">The headers to include in the request.</param>
            /// <param name="onSuccess">Callback function to be invoked on a successful response.</param>
            /// <param name="onFailure">Callback function to be invoked on failure response.</param>
            /// <param name="onConnectionError">Callback function to be invoked on connection error.</param>
            /// <returns>The UnityWebRequestAsyncOperation representing the request.</returns>
            public static UnityWebRequestAsyncOperation Get(string route, DataType dataType, string jsonData, int requestTimeout, List<HeaderKeysAndValue> headerKeysAndValues, Action<string> onSuccess = null, Action<int, string> onFailure = null, Action<int> onConnectionError = null)
            {
                #region Debug

                string headersAre = "";
                foreach (var item in headerKeysAndValues)
                {
                    headersAre += $"{item.key} : {item.value} ,";
                }
                Debug.Log($"Hitting [GET] ::API:: {route} ::SendingData:: {jsonData} :: Headers Are {headersAre}");

                #endregion Debug

                byte[] bite = GetBites(jsonData, dataType);
                UnityWebRequest request = new UnityWebRequest(route, UnityWebRequest.kHttpVerbGET)
                {
                    uploadHandler = new UploadHandlerRaw(bite),
                    downloadHandler = new DownloadHandlerBuffer(),
                };
                return CommonCallBack(request, route, headerKeysAndValues, onSuccess, onFailure, onConnectionError, requestTimeout);
            }

            #region CommonCallBack

            /// <summary>
            /// Converts the given JSON data into a byte array based on the specified data type.
            /// </summary>
            /// <param name="jsonData">The JSON data to be converted.</param>
            /// <param name="dataType">The data type (Json or Form).</param>
            /// <returns>The byte array representation of the data.</returns>
            private static byte[] GetBites(string jsonData, DataType dataType)
            {
                byte[] bite = null;
                if (DataType.Json == dataType)
                {
                    bite = System.Text.Encoding.UTF8.GetBytes(jsonData);
                }
                else if (dataType == DataType.Form)
                {
                    WWWForm form = ConvertJsonToWWWForm(jsonData);
                    bite = form.data;
                }
                else
                {
                    Debug.LogError("Wrong Data Type");
                    return null;
                }
                return bite;
            }

            /// <summary>
            /// Sends the request and handles the response.
            /// </summary>
            /// <param name="request">The UnityWebRequest to send.</param>
            /// <param name="route">The route of the API.</param>
            /// <param name="headerKeysAndValues">The headers for the request.</param>
            /// <param name="onSuccess">Callback function to be invoked on success.</param>
            /// <param name="onFailure">Callback function to be invoked on failure.</param>
            /// <param name="onConnectionError">Callback function to be invoked on connection error.</param>
            /// <param name="requestTimeout">The timeout duration for the request.</param>
            /// <returns>The UnityWebRequestAsyncOperation representing the request.</returns>
            private static UnityWebRequestAsyncOperation CommonCallBack(UnityWebRequest request, string route, List<HeaderKeysAndValue> headerKeysAndValues, Action<string> onSuccess, Action<int, string> onFailure, Action<int> onConnectionError, int requestTimeout)
            {
                if (headerKeysAndValues != null)
                {
                    foreach (var item in headerKeysAndValues)
                    {
                        request.SetRequestHeader(item.key, item.value);
                    }
                }
                request.timeout = requestTimeout;

                UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

                asyncOperation.completed += operation =>
                {
                    Debug.Log($"Hit Response [{request.method}] ::API:: {route} ::Result:: {request.result} ::ResponseCode:: {request.responseCode} ::ReceivedData:: {request.downloadHandler.text}");
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        onConnectionError?.Invoke(-1);
                    }
                    else if (request.result == UnityWebRequest.Result.Success)
                    {
                        onSuccess?.Invoke(request.downloadHandler.text);
                    }
                    else
                    {
                        onFailure?.Invoke((int)request.responseCode, request.downloadHandler.text);
                    }

                    request.Dispose();
                };
                return asyncOperation;
            }

            /// <summary>
            /// Converts a JSON string into a WWWForm.
            /// </summary>
            /// <param name="jsonString">The JSON string to convert.</param>
            /// <returns>The WWWForm created from the JSON string.</returns>
            private static WWWForm ConvertJsonToWWWForm(string jsonString)
            {
                var wwwForm = new WWWForm();
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return wwwForm;
                }
                JSONNode json = JSON.Parse(jsonString);

                foreach (KeyValuePair<string, JSONNode> pair in json.AsObject)
                {
                    wwwForm.AddField(pair.Key, pair.Value.Value);
                }

                return wwwForm;
            }
            #endregion
        }
    }
}
