using EzAPI.RunTime.UserAccessible;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace EzAPI
{
    namespace RunTime
    {
        /// <summary>
        /// Stores method information for generic API calls.
        /// </summary>
        public class MethodStorage
        {
            /// <summary>
            /// First type associated with the method.
            /// </summary>
            public Type type1;

            /// <summary>
            /// Second type associated with the method.
            /// </summary>
            public Type type2;

            /// <summary>
            /// The method information for the API method.
            /// </summary>
            public MethodInfo method;

            /// <summary>
            /// Constructor for creating an instance of MethodStorage.
            /// </summary>
            /// <param name="type1">The first type for the method.</param>
            /// <param name="type2">The second type for the method.</param>
            /// <param name="method">The method information.</param>
            public MethodStorage(Type type1, Type type2, MethodInfo method)
            {
                this.type1 = type1;
                this.type2 = type2;
                this.method = method;
            }
        }

        /// <summary>
        /// Contains the data type and content type for requests.
        /// </summary>
        [Serializable]
        public class DataTypeAndContentType
        {
            /// <summary>
            /// The data type for the request.
            /// </summary>
            public DataType dataType = DataType.Json;

            /// <summary>
            /// The content type for the request.
            /// </summary>
            public string contentType = "application/json";
        }

        /// <summary>
        /// Represents the request details for API calls.
        /// </summary>
        [Serializable]
        public class RequestClass
        {
            /// <summary>
            /// The endpoint for the API request.
            /// </summary>
            public string endPoint;

            /// <summary>
            /// The type of request (GET, POST, PUT, DELETE).
            /// </summary>
            public RequestTypes requestTypes;

            /// <summary>
            /// The payload class used for the request.
            /// </summary>
            public PayLoadEnum payLoadClass;

            /// <summary>
            /// The response class used for the request.
            /// </summary>
            public ResponseEnum responseClass;

            /// <summary>
            /// Timeout information for the request.
            /// </summary>
            public IntAndBool requestTimeout;

            /// <summary>
            /// Retry information for the request.
            /// </summary>
            public IntAndBool retryInfo;

            /// <summary>
            /// Data type structure for the request.
            /// </summary>
            public DataTypeStruct dataTypeStruct;

            /// <summary>
            /// Content type structure for the request.
            /// </summary>
            public ContentTypeStruct contentTypeStruct;
        }

        /// <summary>
        /// Contains content type information for the request.
        /// </summary>
        [Serializable]
        public class ContentTypeStruct
        {
            /// <summary>
            /// Whether the content type is overridden.
            /// </summary>
            public bool contentTypeOverride;

            /// <summary>
            /// The content type to be used for the request.
            /// </summary>
            public string contentType;
        }

        /// <summary>
        /// Contains data type information for the request.
        /// </summary>
        [Serializable]
        public class DataTypeStruct
        {
            /// <summary>
            /// Whether the data type is overridden.
            /// </summary>
            public bool dataTypeOverride;

            /// <summary>
            /// The data type for the request.
            /// </summary>
            public DataType dataType;
        }

        /// <summary>
        /// Represents the type of HTTP request (GET, POST, PUT, DELETE).
        /// </summary>
        [Serializable]
        public enum RequestTypes
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        /// <summary>
        /// Represents the data type for the request (JSON or Form).
        /// </summary>
        [Serializable]
        public enum DataType
        {
            Json,
            Form
        }

        /// <summary>
        /// A class that stores an integer value along with a boolean flag for overriding the value.
        /// </summary>
        [System.Serializable]
        public class IntAndBool
        {
            /// <summary>
            /// Whether the value is overridden.
            /// </summary>
            public bool overrideValue;

            /// <summary>
            /// The overridden integer value.
            /// </summary>
            [Range(1, 1000)]
            public int overridenValue = 1;
        }

        /// <summary>
        /// Represents a key-value pair for HTTP headers.
        /// </summary>
        [Serializable]
        public class HeaderKeysAndValue
        {
            /// <summary>
            /// The key for the header.
            /// </summary>
            public string key;

            /// <summary>
            /// The value for the header.
            /// </summary>
            public string value;
        }
    }
}
