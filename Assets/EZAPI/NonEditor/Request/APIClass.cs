using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EzAPI
{
    namespace RunTime
    {
        namespace UserAccessible
        {
            /// <summary>
            /// Handles the configuration and execution of API requests in runtime.
            /// </summary>
            public class APIClass
            {
                #region Static

                private static MethodInfo apiHitMethod = null;

                /// <summary>
                /// Retrieves the MethodInfo for the <see cref="APIManager.Instance.HitAPI"/> method.
                /// </summary>
                /// <returns>MethodInfo representing the HitAPI method.</returns>
                private static MethodInfo GetAPIHitMethod()
                {
                    if (apiHitMethod == null)
                    {
                        apiHitMethod = typeof(APIManager).GetMethod(nameof(APIManager.Instance.HitAPI));
                    }
                    return apiHitMethod;
                }

                private static List<MethodStorage> methods = new List<MethodStorage>();

                /// <summary>
                /// Retrieves or generates a generic method for the API request, based on the provided payload and response class types.
                /// </summary>
                /// <param name="payloadClassType">The type of the request payload.</param>
                /// <param name="responseClassType">The type of the response.</param>
                /// <returns>The MethodInfo for the appropriate generic method.</returns>
                private static MethodInfo GetGenericMethod(Type payloadClassType, Type responseClassType)
                {
                    foreach (var item in methods)
                    {
                        if (item.type1 == payloadClassType && item.type2 == responseClassType)
                        {
                            return item.method;
                        }
                    }
                    MethodInfo method = GetAPIHitMethod().MakeGenericMethod(payloadClassType, responseClassType);
                    methods.Add(new MethodStorage(payloadClassType, responseClassType, method));
                    return method;
                }

                #endregion Static

                private EndPoints endPoints;
                private Dictionary<string, string> queryParams;
                private List<HeaderKeysAndValue> headerKeysAndValues;
                private RequestPayloadBase payload;
                private Action<RequestResponseBase> gotResponse;
                private Action<float> progress;

                /// <summary>
                /// Initializes a new instance of the <see cref="APIClass"/> class with the given parameters.
                /// </summary>
                /// <param name="endPoints">The endpoint to hit.</param>
                /// <param name="payload">The payload to send with the request (optional).</param>
                /// <param name="headerKeysAndValues">List of headers to include (optional).</param>
                /// <param name="gotResponse">Callback invoked upon receiving the response (optional).</param>
                /// <param name="progress">Callback invoked to report progress (optional).</param>
                public APIClass(EndPoints endPoints, RequestPayloadBase payload = null, List<HeaderKeysAndValue> headerKeysAndValues = null, Action<RequestResponseBase> gotResponse = null, Action<float> progress = null)
                {
                    ResetData(endPoints, payload, headerKeysAndValues, gotResponse, progress);
                }

                /// <summary>
                /// Resets the internal request data of this APIClass instance.
                /// </summary>
                /// <param name="endPoints">The endpoint to hit.</param>
                /// <param name="payload">The payload to send with the request.</param>
                /// <param name="headerKeysAndValues">List of headers to include.</param>
                /// <param name="response">Callback invoked upon receiving the response.</param>
                /// <param name="progress">Callback invoked to report progress.</param>
                public void ResetData(EndPoints endPoints, RequestPayloadBase payload, List<HeaderKeysAndValue> headerKeysAndValues, Action<RequestResponseBase> response, Action<float> progress)
                {
                    this.endPoints = endPoints;
                    this.payload = payload;
                    this.headerKeysAndValues = headerKeysAndValues;
                    this.gotResponse = response;
                    this.progress = progress;
                }

                /// <summary>
                /// Changes the current request payload.
                /// </summary>
                /// <param name="requestPayloadBase">The new payload to set.</param>
                public void ChangeRequestPayload(RequestPayloadBase requestPayloadBase)
                {
                    this.payload = requestPayloadBase;
                }

                #region Params

                /// <summary>
                /// Adds or updates a query parameter for the request.
                /// </summary>
                /// <param name="key">Query parameter key.</param>
                /// <param name="value">Query parameter value.</param>
                public void AddQueryParam(string key, string value)
                {
                    if (queryParams == null)
                        queryParams = new Dictionary<string, string>();

                    if (queryParams.ContainsKey(key))
                    {
                        queryParams[key] = value;
                    }
                    else
                    {
                        queryParams.Add(key, value);
                    }
                }

                /// <summary>
                /// Clears all query parameters.
                /// </summary>
                public void ClearQueryParams()
                {
                    queryParams = new Dictionary<string, string>();
                }

                /// <summary>
                /// Removes a specific query parameter by key.
                /// </summary>
                /// <param name="key">The key of the query parameter to remove.</param>
                public void RemoveQueryParam(string key)
                {
                    if (queryParams == null) return;
                    queryParams.Remove(key);
                }

                #endregion Params

                #region Header

                /// <summary>
                /// Removes a header with the specified key.
                /// </summary>
                /// <param name="key">The key of the header to remove.</param>
                public void RemoveHeader(string key)
                {
                    if (headerKeysAndValues == null)
                    {
                        return;
                    }
                    var item = headerKeysAndValues.FirstOrDefault(h => h.key == key);
                    if (item != null) headerKeysAndValues.Remove(item);
                }

                /// <summary>
                /// Removes all headers from the request.
                /// </summary>
                public void RemoveAllHeader()
                {
                    headerKeysAndValues = null;
                }

                /// <summary>
                /// Adds or updates a header for the request.
                /// </summary>
                /// <param name="key">Header key.</param>
                /// <param name="value">Header value.</param>
                public void AddHeader(string key, string value)
                {
                    if (headerKeysAndValues == null)
                        headerKeysAndValues = new List<HeaderKeysAndValue>();

                    var existing = headerKeysAndValues.FirstOrDefault(h => h.key == key);
                    if (existing != null)
                    {
                        existing.value = value;
                    }
                    else
                    {
                        headerKeysAndValues.Add(new HeaderKeysAndValue() { key = key, value = value });
                    }
                }

                #endregion Header

                #region Progress Listener

                /// <summary>
                /// Adds a listener to receive progress updates during the request.
                /// </summary>
                /// <param name="progress">The callback to invoke with progress updates.</param>
                public void AddProgressListener(Action<float> progress)
                {
                    this.progress += progress;
                }

                /// <summary>
                /// Removes all registered progress listeners.
                /// </summary>
                public void RemoveAllProgressListener()
                {
                    this.progress = null;
                }

                /// <summary>
                /// Removes a specific progress listener.
                /// </summary>
                /// <param name="progress">The progress callback to remove.</param>
                public void RemoveProgressListener(Action<float> progress)
                {
                    this.progress -= progress;
                }

                #endregion Progress Listener

                #region Response Listener

                /// <summary>
                /// Adds a listener to receive the API response.
                /// </summary>
                /// <param name="gotResponse">The callback to invoke with the response.</param>
                public void AddResponseListener(Action<RequestResponseBase> gotResponse)
                {
                    this.gotResponse += gotResponse;
                }

                /// <summary>
                /// Removes all registered response listeners.
                /// </summary>
                public void RemoveAllResponseListener()
                {
                    this.gotResponse = null;
                }

                /// <summary>
                /// Removes a specific response listener.
                /// </summary>
                /// <param name="gotResponse">The response callback to remove.</param>
                public void RemoveResponseListener(Action<RequestResponseBase> gotResponse)
                {
                    this.gotResponse -= gotResponse;
                }

                #endregion Response Listener

                /// <summary>
                /// Sends the API request to the specified endpoint using configured payload, headers, and listeners.
                /// </summary>
                public void HitAPI()
                {
                    var apiManager = APIManager.Instance;
                    ResponseEnum responseType;
                    PayLoadEnum payloadType;
                    if (!apiManager.GetResponseTypeAndPayloadType(endPoints, out payloadType, out responseType))
                    {
                        Debug.LogError("Error Occurred see previous Log");
                    }
                    Type responseClassType = TypeFinder.FindTypeByName(responseType.GetDisplayName());
                    if (responseClassType == null)
                    {
                        Debug.LogError($"Response type '{responseType.GetDisplayName()}' could not be resolved.");
                        return;
                    }
                    Type payloadClassType = TypeFinder.FindTypeByName(payloadType.GetDisplayName());

                    System.Reflection.MethodInfo genericMethod = null;

                    if (genericMethod == null)
                    {
                        if (payloadType != PayLoadEnum.None)
                        {
                            if (payloadClassType == null)
                            {
                                Debug.LogError($"Payload type '{payloadType.GetDisplayName()}' could not be resolved.");
                                return;
                            }
                            genericMethod = GetGenericMethod(payloadClassType, responseClassType);
                        }
                        else
                        {
                            if (payload != null)
                            {
                                Debug.LogWarning($"Payload should be null, processing with null");
                            }
                            genericMethod = GetGenericMethod(typeof(RequestPayloadBase), responseClassType);
                        }
                    }

                    Action<RequestResponseBase> callback = (RequestResponseBase response) =>
                    {
                        gotResponse?.Invoke(response);
                    };
                    Action<float> _progress = (float value) =>
                    {
                        progress?.Invoke(value);
                    };
                    genericMethod.Invoke(apiManager, new object[] { endPoints, payloadType == PayLoadEnum.None ? null : ConvertPayloadToType(payload, payloadClassType), headerKeysAndValues, callback, _progress, queryParams });
                }

                /// <summary>
                /// Converts the provided payload to the specified target type.
                /// </summary>
                /// <param name="basePayload">The base payload to convert.</param>
                /// <param name="targetType">The target type to convert the payload to.</param>
                /// <returns>The converted payload of the specified type.</returns>
                private object ConvertPayloadToType(RequestPayloadBase basePayload, Type targetType)
                {
                    if (basePayload == null) return null;

                    if (targetType.IsInstanceOfType(basePayload))
                    {
                        return basePayload;
                    }

                    string json = JsonUtility.ToJson(basePayload);
                    return JsonUtility.FromJson(json, targetType);
                }
            }
        }
    }
}