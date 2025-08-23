using EzAPI.RunTime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EzAPI
{
    namespace Editor
    {   /// <summary>
        /// Contains information about an enum, including its full name and associated script.
        /// </summary>
        [System.Serializable]
        public struct EnumData
        {
            /// <summary>
            /// The full name of the enum.
            /// </summary>
            public string enumFullName;

            /// <summary>
            /// The MonoScript associated with the enum.
            /// </summary>
            public MonoScript enumFile;
        }
        /// <summary>
        /// A ScriptableObject that manages a list of enum data and provides methods to retrieve enum file paths.
        /// </summary>
        [CreateAssetMenu(fileName = "EnumRegistry", menuName = "EzAPI/Enum Registry", order = 1)]
        public class EnumRegistry : ScriptableObject
        {
            [SerializeField] private List<EnumData> enumEntries = new List<EnumData>();

            /// <summary>
            /// Attempts to find enum data by its full name and return its associated file path.
            /// </summary>
            /// <param name="enumFullName">The full name of the enum to search for.</param>
            /// <param name="fullPath">The output full file path of the enum script.</param>
            /// <returns>True if the enum was found and the path was successfully retrieved; otherwise, false.</returns>
            public bool GetEnumData(string enumFullName, out string fullPath)
            {
                try
                {
                    EnumData enumData = enumEntries.Find(e => e.enumFullName == enumFullName);
                    fullPath = GetFullPath(enumData.enumFile);
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Failed to get path and class Name {exception}");
                    fullPath = null;
                    return false;
                }
            }

            /// <summary>
            /// Retrieves the full file path for a given MonoScript.
            /// </summary>
            /// <param name="script">The MonoScript to retrieve the file path for.</param>
            /// <returns>The full file path for the given script, or null if an error occurs.</returns>
            public static string GetFullPath(MonoScript script)
            {
                if (script == null)
                {
                    Debug.LogError("Input MonoScript is null.");
                    return null;
                }

                string assetPath = AssetDatabase.GetAssetPath(script);

                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"Could not find asset path for MonoScript '{script.name}'. It might not be a project asset file.");
                    return null;
                }

                try
                {
                    string projectRoot = Directory.GetCurrentDirectory();
                    string fullPath = Path.Combine(projectRoot, assetPath);
                    fullPath = Path.GetFullPath(fullPath);
                    return ToRelativeAssetPath(fullPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error converting asset path '{assetPath}' to full path for script '{script.name}': {ex.Message}");
                    return null;
                }
            }

            /// <summary>
            /// Converts an absolute file path to a relative asset path (relative to the Unity project).
            /// </summary>
            /// <param name="fullPath">The absolute file path to convert.</param>
            /// <returns>The relative asset path starting from the "Assets" folder.</returns>
            /// <exception cref="ArgumentException">Thrown if the full path does not contain the "Assets" directory.</exception>
            public static string ToRelativeAssetPath(string fullPath)
            {
                int index = fullPath.IndexOf("Assets", System.StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                    return fullPath.Substring(index).Replace("\\", "/");
                else
                    throw new System.ArgumentException("Path does not contain 'Assets'");
            }
        }
    }
}
