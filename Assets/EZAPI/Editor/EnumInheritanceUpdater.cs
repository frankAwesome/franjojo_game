using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace EzAPI
{
    namespace Editor
    {
        public static class EnumInheritanceUpdater
        {
            /// <summary>
            /// Updates an enum definition in a specified file to include members
            /// representing all non-abstract classes inheriting from a given base class.
            /// </summary>
            /// <param name = "enumName" > The name of the enum type to update.</param>
            /// <param name = "enumFilePath" > The project-relative path to the .cs file containing the enum (e.g., "Assets/Scripts/MyEnum.cs").</param>
            /// <param name = "baseClassName" > The name of the base class whose inheritors should be added to the enum.</param>
            /// <param name = "logOutput" > Optional action to redirect logging messages(e.g., to an editor window).</param>
            /// <returns>True if the enum was updated successfully, false otherwise.</returns>
            public static bool UpdateEnumFromInheritance(
                string enumName,
                string enumFilePath,
                string baseClassName, bool addNone, bool addSelf)
            {
                if (string.IsNullOrWhiteSpace(enumName) || string.IsNullOrWhiteSpace(enumFilePath) || string.IsNullOrWhiteSpace(baseClassName))
                {
                    Debug.LogError("Error: Enum Name, File Path, and Base Class Name cannot be empty.");
                    return false;
                }

                if (!enumFilePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) || !enumFilePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"Error: File path '{enumFilePath}' must start with 'Assets/' and end with '.cs'.");
                    return false;
                }

                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), enumFilePath);

                if (!File.Exists(fullPath))
                {
                    Debug.LogError($"Error: File not found at '{enumFilePath}' (Full Path: {fullPath})");
                    return false;
                }

                // --- Reflection ---
                Type baseType = TypeFinder.FindTypeByName(baseClassName);
                if (baseType == null)
                {
                    Debug.LogError($"Error: Base class '{baseClassName}' not found. Make sure it compiles and is in the project.");
                    return false;
                }
                if (!baseType.IsClass)
                {
                    Debug.LogError($"Error: '{baseClassName}' is not a class.");
                    return false;
                }

                List<Type> inheritedTypes = InheritedClassFinder.FindInheritedClasses(baseType);
                if (inheritedTypes.Count == 0)
                {
                    Debug.LogWarning($"Warning: No non-abstract classes found inheriting from '{baseClassName}'. Enum will be updated to be empty (or contain only manually added members if the regex preserves them).");
                }
                else
                {
                    Debug.Log($"Found {inheritedTypes.Count} classes inheriting from '{baseClassName}'.");
                }

                foreach (var item in inheritedTypes)
                {
                    foreach (var itemNew in inheritedTypes)
                    {
                        if (item.Name == itemNew.Name && item != itemNew)
                        {
                            Debug.LogError($"Name of two Response or Payload classes can not be same. Same Class Name is {item.Name}");
                            return false;
                        }
                    }
                }

                if (addSelf)
                {
                    inheritedTypes.Add(baseType);
                }
                // --- File Update ---
                bool success = EnumAdder.UpdateEnumFile(fullPath, enumName, inheritedTypes, addNone);

                if (success)
                {
                    Debug.Log($"Successfully updated enum '{enumName}' in file '{enumFilePath}'. Triggering asset refresh.");
                    return true;
                }
                else
                {
                    // Specific error logged within UpdateEnumFile
                    return false;
                }
            }
        }
    }
}