using System;

namespace EzAPI
{
    namespace RunTime
    {
        namespace UserAccessible
        {
            /// <summary>
            /// Base class for request payloads.
            /// </summary>
            [Serializable]
            public class RequestPayloadBase
            {
                // Empty base class for request payloads. Derived classes can extend this to add specific request data.
            }

            /// <summary>
            /// Base class for request responses.
            /// </summary>
            [Serializable]
            public class RequestResponseBase
            {
                /// <summary>
                /// Indicates whether the request was successful.
                /// </summary>
                public bool success;

                /// <summary>
                /// The response code from the request.
                /// </summary>
                public int responseCode;

                /// <summary>
                /// The failure message, if any, when the request was not successful.
                /// </summary>
                public string failureMessage;
            }
        }
    }
}