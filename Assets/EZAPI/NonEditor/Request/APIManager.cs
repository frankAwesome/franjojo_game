using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EzAPI
{
    namespace RunTime
    {
        namespace UserAccessible
        {
            /// <summary>
            /// Handles API communication, manages requests, retries, response parsing and validation.
            /// </summary>
            public class APIManager : MonoBehaviourSingletonPersistent<APIManager>
            {
                [SerializeField] private Settings settings;

                /// <summary>
                /// Gets the payload and response types for the specified endpoint.
                /// </summary>
                /// <param name="endPoint">The endpoint to look up.</param>
                /// <param name="payLoadEnum">The payload type associated with the endpoint.</param>
                /// <param name="responseEnum">The response type associated with the endpoint.</param>
                /// <returns>True if types were found successfully; otherwise false.</returns>
                public bool GetResponseTypeAndPayloadType(EndPoints endPoint, out PayLoadEnum payLoadEnum, out ResponseEnum responseEnum)
                {
                    RequestClass requestClass = settings.GetRequestClass(endPoint);
                    if (requestClass == null)
                    {
                        payLoadEnum = PayLoadEnum.None;
                        responseEnum = ResponseEnum.Requestresponsebase;
                        return false;
                    }
                    payLoadEnum = requestClass.payLoadClass;
                    responseEnum = requestClass.responseClass;
                    return true;
                }

                /// <summary>
                /// Sends an API request with the given endpoint, payload, and headers, and processes the response.
                /// </summary>
                /// <typeparam name="T1">Type of payload.</typeparam>
                /// <typeparam name="T2">Type of response.</typeparam>
                /// <param name="endPoint">API endpoint to hit.</param>
                /// <param name="payload">Request payload (can be null if not required).</param>
                /// <param name="headerKeysAndValues">List of headers to include.</param>
                /// <param name="response">Callback for the API response.</param>
                /// <param name="progress">Callback for tracking download progress.</param>
                /// <param name="queryParams">Optional query parameters.</param>
                public void HitAPI<T1, T2>(EndPoints endPoint, T1 payload, List<HeaderKeysAndValue> headerKeysAndValues, Action<T2> response = null, Action<float> progress = null, Dictionary<string, string> queryParams = null)
                    where T1 : RequestPayloadBase
                    where T2 : RequestResponseBase, new()
                {
                    try
                    {
                        RequestClass requestClass = settings.GetRequestClass(endPoint);
                        if (requestClass == null)
                        {
                            HandleOtherResponse(-2, response, $"Request Class Not Found For End Point {endPoint}");
                            return;
                        }

                        if (typeof(T2).Name != requestClass.responseClass.GetDisplayName())
                        {
                            HandleOtherResponse(-2, response, $"Wrong Response Type :: Expected {requestClass.responseClass.GetDisplayName()} Has :: {(new T2()).GetType().Name}");
                            return;
                        }

                        if (requestClass.payLoadClass == PayLoadEnum.None)
                        {
                            if (payload != null)
                            {
                                Debug.LogWarning($"Wrong PayloadType :: Expected None \"Null\" Has :: {payload.GetType().Name} Processing with null");
                            }
                        }
                        else
                        {
                            if (payload == null)
                            {
                                HandleOtherResponse(-2, response, $"Wrong PayloadType :: Expected {requestClass.payLoadClass.GetDisplayName()} Has NULL");
                                return;
                            }
                            if (payload.GetType().Name != requestClass.payLoadClass.GetDisplayName())
                            {
                                HandleOtherResponse(-2, response, $"Wrong PayloadType :: Expected {requestClass.payLoadClass.GetDisplayName()} Has {payload.GetType().Name}");
                                return;
                            }
                        }

                        string jsonData = payload == null ? " " : JsonUtility.ToJson(payload);
                        int retryRemaining = requestClass.retryInfo.overrideValue ? requestClass.retryInfo.overridenValue : settings.GetAPIConfig().defaultRetryCount;
                        int requestTimeout = requestClass.requestTimeout.overrideValue ? requestClass.requestTimeout.overridenValue : settings.GetAPIConfig().defaultRequestTimeout;
                        DataType dataType = requestClass.dataTypeStruct.dataTypeOverride ? requestClass.dataTypeStruct.dataType : settings.GetAPIConfig().dataType;
                        string contentType = requestClass.contentTypeStruct.contentTypeOverride ? requestClass.contentTypeStruct.contentType : settings.GetAPIConfig().ContentType(dataType);

                        if (headerKeysAndValues == null)
                        {
                            headerKeysAndValues = new List<HeaderKeysAndValue>();
                        }

                        headerKeysAndValues.Add(new HeaderKeysAndValue() { key = "Content-Type", value = contentType });

                        KeepSendingRequest(requestClass.requestTypes, dataType, requestTimeout, retryRemaining, AppendQueryParams(requestClass.endPoint, queryParams), jsonData, headerKeysAndValues, response, progress);
                    }
                    catch (Exception exception)
                    {
                        HandleOtherResponse(-2, response, "API HIT FAILED " + exception);
                        return;
                    }
                }

                /// <summary>
                /// Retries the API request if necessary, until max retries are exhausted or a successful response is received.
                /// </summary>
                private void KeepSendingRequest<T>(RequestTypes requestTypes, DataType dataType, int requestTimeout, int retryRemaining, string endPoint, string jsonData, List<HeaderKeysAndValue> headerKeysAndValues, Action<T> response = null, Action<float> progress = null) where T : RequestResponseBase, new()
                {
                    retryRemaining--;
                    UnityWebRequestAsyncOperation unityWebRequest = SendRequest<T>(requestTypes, dataType, requestTimeout, endPoint, jsonData, headerKeysAndValues, (currResponse) =>
                    {
                        if (!currResponse.success)
                        {
                            if (retryRemaining > 0)
                            {
                                KeepSendingRequest(requestTypes, dataType, requestTimeout, retryRemaining, endPoint, jsonData, headerKeysAndValues, response);
                            }
                            else
                            {
                                response?.Invoke(currResponse);
                            }
                        }
                        else
                        {
                            response?.Invoke(currResponse);
                        }
                    });
                    if (unityWebRequest != null && progress != null)
                    {
                        StartCoroutine(DownloadProgress(unityWebRequest, progress));
                    }
                }

                /// <summary>
                /// Dispatches a UnityWebRequest based on request type (GET/POST/etc).
                /// </summary>
                private UnityWebRequestAsyncOperation SendRequest<T>(RequestTypes requestTypes, DataType dataType, int requestTimeout, string endPoint, string jsonData, List<HeaderKeysAndValue> headerKeysAndValues, Action<T> response) where T : RequestResponseBase, new()
                {
                    UnityWebRequestAsyncOperation unityWebRequest = null;
                    switch (requestTypes)
                    {
                        case RequestTypes.GET:
                            unityWebRequest = Requests.Get(settings.GetAPIConfig().baseUrl + endPoint.ToString(), dataType, jsonData, requestTimeout, headerKeysAndValues,
                            (responseString) => HandleSuccessResponse(responseString, response),
                            (code, responseString) => HandleFailureResponse(code, responseString, response),
                            (code) => HandleOtherResponse(code, response));
                            break;

                        case RequestTypes.POST:
                            unityWebRequest = Requests.Post(settings.GetAPIConfig().baseUrl + endPoint.ToString(), dataType, jsonData, requestTimeout, headerKeysAndValues,
                                (responseString) => HandleSuccessResponse(responseString, response),
                                (code, responseString) => HandleFailureResponse(code, responseString, response),
                                (code) => HandleOtherResponse(code, response));
                            break;

                        case RequestTypes.PUT:
                            unityWebRequest = Requests.PUT(settings.GetAPIConfig().baseUrl + endPoint.ToString(), dataType, jsonData, requestTimeout, headerKeysAndValues,
                                (responseString) => HandleSuccessResponse(responseString, response),
                                (code, responseString) => HandleFailureResponse(code, responseString, response),
                                (code) => HandleOtherResponse(code, response));
                            break;

                        case RequestTypes.DELETE:
                            unityWebRequest = Requests.Delete(settings.GetAPIConfig().baseUrl + endPoint.ToString(), dataType, jsonData, requestTimeout, headerKeysAndValues,
                                (responseString) => HandleSuccessResponse(responseString, response),
                                (code, responseString) => HandleFailureResponse(code, responseString, response),
                                (code) => HandleOtherResponse(code, response));
                            break;
                    }
                    return unityWebRequest;
                }

                /// <summary>
                /// Handles failed backend responses by attempting to parse error data and passing it to callback.
                /// </summary>
                private void HandleFailureResponse<T>(int responseCode, string responseString, Action<T> response) where T : RequestResponseBase, new()
                {
                    T responseData = new T();
                    try
                    {
                        responseData = JsonUtility.FromJson<T>(responseString);
                        responseData.success = false;
                        responseData.responseCode = responseCode;
                        responseData.failureMessage = "Failed From Backend";
                        response?.Invoke(responseData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Json Not Parseable {ex}");
                        responseData.success = false;
                        response?.Invoke(responseData);
                    }
                }

                /// <summary>
                /// Handles successful responses and deserializes them into expected response type.
                /// </summary>
                private void HandleSuccessResponse<T>(string responseString, Action<T> response) where T : RequestResponseBase, new()
                {
                    T responseData = new T();
                    try
                    {
                        responseData = JsonUtility.FromJson<T>(responseString);
                        responseData.success = true;
                        response?.Invoke(responseData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Json Not Parseable {ex}");
                        responseData.success = true;
                        response?.Invoke(responseData);
                    }
                }

                /// <summary>
                /// Handles unknown or non-JSON parse able responses by invoking callback with error info.
                /// </summary>
                private void HandleOtherResponse<T>(int code, Action<T> response, string message = null) where T : RequestResponseBase, new()
                {
                    T responseData = new T();
                    responseData.success = false;
                    responseData.failureMessage = message == null ? "Unknown Error" : message;
                    responseData.responseCode = code;
                    response?.Invoke(responseData);
                }

                /// <summary>
                /// Coroutine that continuously updates download progress via callback until complete.
                /// </summary>
                private IEnumerator DownloadProgress(UnityWebRequestAsyncOperation operation, Action<float> progress)
                {
                    while (!operation.isDone)
                    {
                        progress?.Invoke(operation.progress);
                        yield return null;
                    }
                }

                /// <summary>
                /// Appends query parameters to a URL.
                /// </summary>
                /// <param name="url">Base URL to append to.</param>
                /// <param name="queryParams">Key-value query parameters.</param>
                /// <returns>URL with query string appended.</returns>
                private string AppendQueryParams(string url, Dictionary<string, string> queryParams)
                {
                    if (queryParams == null || queryParams.Count == 0) return url;

                    var query = string.Join("&", queryParams
                        .Select(kvp => UnityWebRequest.EscapeURL(kvp.Key) + "=" + UnityWebRequest.EscapeURL(kvp.Value)));

                    if (url.Contains("?"))
                        return url + "&" + query;
                    else
                        return url + "?" + query;
                }
            }
        }
    }
}