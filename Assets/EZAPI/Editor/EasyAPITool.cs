using EzAPI.RunTime;
using EzAPI.RunTime.UserAccessible;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EzAPI
{
    namespace Editor
    {
        /// <summary>
        /// Provides utility methods for managing EasyAPI-related assets and actions within the Unity editor.
        /// </summary>
        public static class EzAPIMenus
        {
            /// <summary>
            /// Gets the EnumRegistry instance in the project.
            /// </summary>
            /// <remarks>
            /// This property checks for the existence of an <see cref="EnumRegistry"/> asset in the project,
            /// ensuring there is exactly one asset available. If multiple assets are found, an error is logged.
            /// </remarks>
            private static EnumRegistry enumRegistry
            {
                get
                {
                    string[] guids = AssetDatabase.FindAssets($"t:{nameof(EnumRegistry)}");

                    if (guids.Length > 0)
                    {
                        if (guids.Length > 1)
                        {
                            Debug.LogError("Found More than one EnumRegistry asset in project. Please make sure there is only one.");
                            return null;
                        }
                        else
                        {
                            Debug.Log($"Found EnumRegistry asset in project. Loading...");
                            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                            return AssetDatabase.LoadAssetAtPath<EnumRegistry>(path);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No MyDataObject asset found in project.");
                        return null;
                    }
                }
            }

            /// <summary>
            /// Finds and returns the Settings asset in the project.
            /// </summary>
            /// <returns>The <see cref="Settings"/> asset, or null if not found.</returns>
            private static Settings FindAPIDataAsset()
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(Settings)}");

                if (guids.Length > 1)
                {
                    Debug.LogError("Found more than one APIData asset. Please ensure there is only one.");
                    return null;
                }

                if (guids.Length == 1)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<Settings>(path);
                }

                Debug.LogWarning("No APIData asset found.");
                return null;
            }

            /// <summary>
            /// Opens the APIData asset in the editor.
            /// </summary>
            [MenuItem("Tools/EzAPI/Settings")]
            public static void SelectAPIData()
            {
                Settings asset = FindAPIDataAsset();
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                    Debug.Log("Selected APIData asset.");
                }
            }

            /// <summary>
            /// Regenerates the APIData and updates associated enums, ensuring no duplicate endpoints exist.
            /// </summary>
            [MenuItem("Tools/EzAPI/Save")]
            public static void RegenerateAPIData()
            {
                // Check for duplicate endpoints in APIData
                foreach (var item in FindAPIDataAsset().GetEndPoints())
                {
                    foreach (var itemToTestWith in FindAPIDataAsset().GetEndPoints())
                    {
                        if (item.endPoint == itemToTestWith.endPoint && item != itemToTestWith)
                        {
                            Debug.LogError($"Failed to update enum. Can not have Two types of same endPoint. Duplicate EndPoint {item.endPoint}");
                            return;
                        }
                    }
                }

                // Update enums for Payload, Response, and Endpoints
                string fullPath;
                if (enumRegistry.GetEnumData(nameof(PayLoadEnum), out fullPath))
                {
                    bool result = EnumInheritanceUpdater.UpdateEnumFromInheritance(nameof(PayLoadEnum), fullPath, nameof(RequestPayloadBase), true, true);
                    if (result)
                    {
                        Debug.Log($"Successfully updated enum '{nameof(PayLoadEnum)}' via static call.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to update enum '{nameof(PayLoadEnum)}' via static call. Check previous logs for errors.");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to update enum '{nameof(PayLoadEnum)}' via static call. Check previous logs for errors.");
                }

                if (enumRegistry.GetEnumData(nameof(ResponseEnum), out fullPath))
                {
                    bool result = EnumInheritanceUpdater.UpdateEnumFromInheritance(nameof(ResponseEnum), fullPath, nameof(RequestResponseBase), false, true);
                    if (result)
                    {
                        Debug.Log($"Successfully updated enum '{nameof(ResponseEnum)}' via static call.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to update enum '{nameof(ResponseEnum)}' via static call. Check previous logs for errors.");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to update enum '{nameof(PayLoadEnum)}' via static call. Check previous logs for errors.");
                }

                if (enumRegistry.GetEnumData(nameof(EndPoints), out fullPath))
                {
                    List<string> namesOfEnums = new List<string>();
                    foreach (var requestClass in FindAPIDataAsset().GetEndPoints())
                    {
                        namesOfEnums.Add(requestClass.endPoint);
                    }
                    EnumAdder.UpdateEnumFile(fullPath, nameof(EndPoints), namesOfEnums, false);
                }
                else
                {
                    Debug.LogError($"Failed to update enum '{nameof(PayLoadEnum)}' via static call. Check previous logs for errors.");
                }

                AssetDatabase.Refresh();
            }

            /// <summary>
            /// Instantiates the APIManager prefab in the scene.
            /// </summary>
            [MenuItem("Tools/EzAPI/API Hander")]
            public static void InstantiateMyPrefab()
            {
                const string APIManager = "APIManager";

                string[] guids = AssetDatabase.FindAssets($"{APIManager} t:Prefab");

                if (guids.Length == 0)
                {
                    Debug.LogWarning($"No prefab named '{APIManager}' found in the project.");
                    return;
                }

                if (guids.Length > 1)
                {
                    Debug.LogError($"Multiple prefabs named '{APIManager}' found. Please ensure there is only one.");
                    return;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    Debug.LogError("Failed to load prefab.");
                    return;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, APIManager);
                    Selection.activeGameObject = instance;
                    Debug.Log($"Instantiated '{prefab.name}' in scene.");
                }
                else
                {
                    Debug.LogError("Failed to instantiate prefab.");
                }
            }
        }
    }
}
