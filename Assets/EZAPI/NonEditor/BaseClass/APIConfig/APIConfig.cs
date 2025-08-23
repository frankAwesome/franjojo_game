using System;
using System.Collections.Generic;
using UnityEngine;

namespace EzAPI
{
    namespace RunTime
    {
        /// <summary>
        /// ScriptableObject used to configure API settings such as base URL, timeout, retry count, and content types.
        /// </summary>
        [CreateAssetMenu(fileName = "APIConfig", menuName = "EzAPI/API Config", order = 3)]
        public class APIConfig : ScriptableObject
        {
            /// <summary>
            /// Base URL for all API requests.
            /// </summary>
            public string baseUrl;

            /// <summary>
            /// Default timeout (in seconds) for API requests. Valid range: 1 to 100.
            /// </summary>
            [Range(1, 1000)]
            public int defaultRequestTimeout;

            /// <summary>
            /// Number of retry attempts for failed API requests. Valid range: 1 to 100.
            /// </summary>
            [Range(1, 1000)]
            public int defaultRetryCount;

            /// <summary>
            /// Default data type used for requests (e.g., JSON, Form).
            /// </summary>
            public DataType dataType = DataType.Json;

            /// <summary>
            /// List mapping DataType values to corresponding HTTP Content-Type strings.
            /// Used to determine the correct Content-Type header for API requests.
            /// </summary>
            public List<DataTypeAndContentType> dataTypeAndContentTypes = new List<DataTypeAndContentType>()
            {
                new DataTypeAndContentType()
                {
                    dataType = DataType.Json,
                    contentType = "application/json",
                },
                new DataTypeAndContentType()
                {
                    dataType = DataType.Form,
                    contentType = "application/x-www-form-urlencoded"
                }
            };

            /// <summary>
            /// Returns the Content-Type string associated with the specified DataType.
            /// If the data type is not found, returns "application/json" and logs a warning.
            /// </summary>
            /// <param name="dataType">The data type for which to get the Content-Type header.</param>
            /// <returns>A string representing the HTTP Content-Type.</returns>
            public string ContentType(DataType dataType)
            {
                foreach (var item in dataTypeAndContentTypes)
                {
                    if (item.dataType == dataType)
                    {
                        return item.contentType;
                    }
                }
                Debug.LogWarning("Data Type not found in settings. Returning default content type [application/json].");
                return "application/json";
            }
        }
    }
}
