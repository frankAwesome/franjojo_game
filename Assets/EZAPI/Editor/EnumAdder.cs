using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace EzAPI
{
    namespace Editor
    {
        public class EnumAdder
        {
            public static bool UpdateEnumFile(string fullPath, string targetEnumName, List<String> typesToInclude, bool addNone)
            {
                try
                {
                    if (addNone)
                    {
                        typesToInclude.Insert(0, "None");
                    }
                    string fileContent = File.ReadAllText(fullPath);

                    // --- Build the new enum content ---
                    StringBuilder enumContentBuilder = new StringBuilder();
                    for (int i = 0; i < typesToInclude.Count; i++)
                    {
                        if (string.IsNullOrEmpty(typesToInclude[i]))
                        {
                            continue;
                        }

                        enumContentBuilder.Append("    "); // Indentation
                        enumContentBuilder.Append($"[DisplayName(\"{typesToInclude[i]}\")]");
                        enumContentBuilder.AppendLine();
                        enumContentBuilder.Append(ToEnumCompatible(typesToInclude[i]));
                        if (i < typesToInclude.Count - 1)
                        {
                            enumContentBuilder.Append(",");
                        }
                        enumContentBuilder.AppendLine();
                    }

                    string newEnumMembers = enumContentBuilder.ToString().TrimEnd('\r', '\n'); // Remove trailing newline


                    // --- Use Regex to find and replace the enum block ---
                    // (?<=\s|^) - Positive lookbehind for whitespace or start of line
                    // (\[.*?\]\s*)? - Optionally match attributes and whitespace (Group 1: Attributes)
                    // (public\s+|internal\s+|private\s+)? - Optionally match access modifier (Group 2: Modifier)
                    // enum\s+ - Match "enum" and whitespace
                    // ({targetEnumName}) - Match the specific enum name (Group 3: Enum Name) - Use Regex.Escape if needed, but usually okay for simple names
                    // (\s*:\s*\w+)? - Optionally match enum underlying type like : int (Group 4: Underlying Type)
                    // \s*\{ - Match optional whitespace then the opening brace
                    // ([\s\S]*?) - Non-greedily capture everything inside (Group 5: Existing Content)
                    // \} - Match the closing brace
                    string pattern = $@"((?<=\s|^)(?:\[.*?\]\s*)?(?:public\s+|internal\s+|private\s+)?enum\s+{Regex.Escape(targetEnumName)}(?:\s*:\s*\w+)?\s*\{{)[\s\S]*?(\}})";
                    Regex regex = new Regex(pattern, RegexOptions.Multiline);

                    int replacements = 0;
                    string updatedFileContent = regex.Replace(fileContent, match =>
                    {
                        replacements++;
                        // Reconstruct the enum block with the new members
                        string start = match.Groups[1].Value; // Everything before the original content (incl. attributes, modifier, name, type, opening brace)
                        string end = match.Groups[2].Value; // The closing brace

                        // Add a newline before the members if there are any, and a newline before the closing brace
                        string middle = string.IsNullOrWhiteSpace(newEnumMembers) ? "\n" : $"\n{newEnumMembers}\n";

                        // Handle potential lack of newline before start if enum is first thing in file
                        if (!start.EndsWith("\n") && !start.EndsWith("\r\n")) start += "\n";

                        return $"{start}{middle}{end}";
                    }, 1); // Replace only the first occurrence

                    if (replacements == 0)
                    {
                        Debug.LogError($"Error: Could not find enum definition matching pattern for 'enum {targetEnumName} {{ ... }}' in file '{fullPath}'. Check spelling, formatting, and potential complexity.");
                        return false;
                    }
                    if (replacements > 1)
                    {
                        Debug.LogError($"Warning: Found multiple definitions matching 'enum {targetEnumName}'. Only the first one was replaced.");
                        // Continue, as we replaced the first one.
                    }


                    // Check if content actually changed to avoid unnecessary writes/recompiles
                    if (updatedFileContent == fileContent)
                    {
                        return true; // Indicate the desired state is achieved
                    }


                    // Write the modified content back to the file
                    File.WriteAllText(fullPath, updatedFileContent, Encoding.UTF8); // Specify UTF8 encoding

                    Debug.Log(updatedFileContent);
                    return true;
                }
                catch (IOException ioEx)
                {
                    Debug.LogError($"Error accessing file '{fullPath}': {ioEx.Message}");
                    return false;
                }
                catch (ArgumentException argEx) // Can be thrown by Regex
                {
                    Debug.LogError($"Error during Regex processing for enum '{targetEnumName}': {argEx.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An unexpected error occurred while updating enum '{targetEnumName}': {ex.ToString()}"); // Log full exception details
                    return false;
                }
            }
            // Reads the file, modifies the enum section, and writes it back
            public static bool UpdateEnumFile(string fullPath, string targetEnumName, List<Type> typesToInclude, bool addNone)
            {
                List<string> namesOfTypes = new List<string>();
                for (int i = 0; i < typesToInclude.Count; i++)
                {
                    namesOfTypes.Add(typesToInclude[i].Name);
                }
                return UpdateEnumFile(fullPath, targetEnumName, namesOfTypes, addNone);
            }

            private static string ToEnumCompatible(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return "_";

                // Remove all invalid characters (keep letters, numbers, and underscore)
                var cleaned = Regex.Replace(input, @"[^a-zA-Z0-9_]+", "_");

                // Split by underscores or numbers and capitalize parts
                var parts = cleaned.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                var sb = new StringBuilder();
                foreach (var part in parts)
                {
                    if (part.Length == 0) continue;

                    // Capitalize first letter, keep rest lowercase
                    var word = char.ToUpper(part[0]) + part.Substring(1).ToLower();
                    sb.Append(word);
                }

                var result = sb.ToString();

                // Ensure it starts with a letter or underscore
                if (!Regex.IsMatch(result, @"^[A-Za-z_]"))
                    result = "_" + result;

                return result;
            }
        }
    }
}