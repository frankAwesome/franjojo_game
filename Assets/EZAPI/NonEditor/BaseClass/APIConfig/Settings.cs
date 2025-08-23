using EzAPI.RunTime.UserAccessible;
using System.Collections.Generic;
using UnityEngine;

namespace EzAPI
{
    namespace RunTime
    {
        /// <summary>
        /// Represents the settings for the API manager.
        /// This ScriptableObject holds the API configuration and a list of API endpoints.
        /// </summary>
        [CreateAssetMenu(fileName = "APIData", menuName = "EzAPI/API Manager", order = 2)]
        public class Settings : ScriptableObject
        {
            [SerializeField]
            private APIConfig apiConfig;

            [SerializeField]
            private List<RequestClass> endPoints;

            /// <summary>
            /// Gets the API configuration.
            /// </summary>
            /// <returns>The API configuration.</returns>
            public APIConfig GetAPIConfig()
            {
                return apiConfig;
            }

            /// <summary>
            /// Gets the list of all available API endpoints.
            /// </summary>
            /// <returns>A list of <see cref="RequestClass"/> representing the available API endpoints.</returns>
            public List<RequestClass> GetEndPoints()
            {
                return endPoints;
            }

            /// <summary>
            /// Retrieves the request class associated with a specific API endpoint.
            /// </summary>
            /// <param name="endPoint">The endpoint to search for.</param>
            /// <returns>The <see cref="RequestClass"/> associated with the specified endpoint, or null if not found.</returns>
            /// <remarks>
            /// If no matching endpoint is found, a message will be logged.
            /// </remarks>
            public RequestClass GetRequestClass(EndPoints endPoint)
            {
                foreach (var item in endPoints)
                {
                    if (item.endPoint == endPoint.GetDisplayName())
                    {
                        return item;
                    }
                }
                Debug.Log($"Could Not Find Request Class For End Point {endPoint}");
                return null;
            }
        }
    }
}
