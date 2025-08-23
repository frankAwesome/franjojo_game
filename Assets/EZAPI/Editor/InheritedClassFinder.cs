using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace EzAPI
{
    namespace Editor
    {
        public class InheritedClassFinder
        {
            public static List<Type> FindInheritedClasses(Type baseType)
            {


                List<Type> types = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = assembly.FullName.ToLowerInvariant();
                    if (assemblyName.StartsWith("unity") || assemblyName.StartsWith("system") || assemblyName.StartsWith("mscorlib") || assemblyName.StartsWith("mono."))
                        continue;

                    try
                    {
                        types.AddRange(assembly.GetTypes().Where(t =>
                            t != null &&           // Ensure type is loaded
                            t.IsClass &&
                            !t.IsAbstract &&
                            baseType.IsAssignableFrom(t) && // Use IsAssignableFrom - more robust than IsSubclassOf for interfaces/complex hierarchies
                            t != baseType          // Exclude the base type itself
                        ));
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Debug.LogWarning($"Enum Updater: Could not fully load types from assembly {assembly.FullName} while searching for inheritors of '{baseType.Name}'. Errors: {string.Join(", ", ex.LoaderExceptions.Select(e => e?.Message ?? "N/A"))}");
                        // Attempt to check the types that did load
                        types.AddRange(ex.Types.Where(t =>
                           t != null &&
                           t.IsClass &&
                           !t.IsAbstract &&
                           baseType.IsAssignableFrom(t) &&
                           t != baseType
                       ));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Enum Updater: Error searching assembly {assembly.FullName} for inheritors of '{baseType.Name}': {ex.Message}");
                    }
                }

                // Remove duplicates just in case they came from different load contexts (though unlikely here)
                types = types.Distinct().ToList();

                // Sort alphabetically for consistent order
                types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

                return types;
            }
        }
    }
}