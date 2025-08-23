using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EzAPI
{
    /// <summary>
    /// Provides utility methods for finding types and retrieving display names for enums.
    /// </summary>
    public static class TypeFinder
    {
        /// <summary>
        /// Attribute used to assign a display name to an enum field.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class DisplayNameAttribute : Attribute
        {
            /// <summary>
            /// The display name for the enum field.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayNameAttribute"/> class with the specified display name.
            /// </summary>
            /// <param name="Name">The display name for the enum field.</param>
            public DisplayNameAttribute(string Name)
            {
                this.Name = Name;
            }
        }

        /// <summary>
        /// Gets the display name of an enum value, if it has a <see cref="DisplayNameAttribute"/> applied.
        /// Otherwise, returns the enum value's string representation.
        /// </summary>
        /// <param name="value">The enum value whose display name is to be retrieved.</param>
        /// <returns>The display name of the enum value, or its string representation if no display name is found.</returns>
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            var member = type.GetMember(value.ToString());

            if (member.Length > 0)
            {
                var attr = member[0].GetCustomAttribute<DisplayNameAttribute>();
                return attr?.Name ?? value.ToString();
            }
            else
            {
                Debug.LogWarning($"Could not find member for enum value '{value}' of type '{type}'. Returning ToString().");
                return value.ToString();
            }
        }

        /// <summary>
        /// Finds and returns a <see cref="Type"/> by its name, searching through the assemblies of the current app domain.
        /// </summary>
        /// <param name="typeName">The name of the type to search for.</param>
        /// <returns>The <see cref="Type"/> with the specified name, or null if not found.</returns>
        /// <remarks>
        /// This method searches all loaded assemblies except those from Unity, System, or Mono.
        /// It first attempts to find the type case-sensitively, and if not found, tries case-insensitive search.
        /// </remarks>
        public static Type FindTypeByName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.FullName.ToLowerInvariant();
                if (assemblyName.StartsWith("unity") || assemblyName.StartsWith("system") || assemblyName.StartsWith("mscorlib") || assemblyName.StartsWith("mono."))
                    continue;

                try
                {
                    // Try case-sensitive first
                    var type = assembly.GetType(typeName, false, false);
                    if (type != null) return type;

                    // Try case-insensitive if not found
                    type = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                    if (type != null) return type;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"Enum Updater: Could not fully load types from assembly {assembly.FullName} while searching for '{typeName}'. Errors: {string.Join(", ", ex.LoaderExceptions.Select(e => e?.Message ?? "N/A"))}");
                    // Check the types that *did* load
                    foreach (var type in ex.Types)
                    {
                        if (type != null && type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                        {
                            return type;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Enum Updater: Error searching assembly {assembly.FullName} for '{typeName}': {ex.Message}");
                }
            }
            return null; // Type not found
        }
    }
}
