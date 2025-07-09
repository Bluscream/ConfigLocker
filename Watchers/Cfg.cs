using System.Text;
using System.Text.RegularExpressions;

namespace ConfigLocker;

public class CfgConfigProcessor : IConfigProcessor {
    public bool SupportsMerging => true;

    public Dictionary<string, object> ParseInput(string content) {
        if (string.IsNullOrWhiteSpace(content)) {
            return new Dictionary<string, object>();
        }

        try {
            var result = new Dictionary<string, object>();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines) {
                var trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || 
                    trimmedLine.StartsWith('#') || 
                    trimmedLine.StartsWith(';') || 
                    trimmedLine.StartsWith("//")) {
                    continue;
                }

                // Parse the CFG format: <any string> variable "value"
                var match = ParseCfgLine(trimmedLine);
                if (match.HasValue) {
                    var (section, key, value) = match.Value;
                    
                    if (!result.ContainsKey(section)) {
                        result[section] = new Dictionary<string, object>();
                    }
                    
                    var sectionDict = (Dictionary<string, object>)result[section];
                    sectionDict[key] = value;
                }
            }

            return result;
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to parse CFG: {ex.Message}");
        }
    }

    public string MergeAndSerialize(Dictionary<string, object> inputs, string existingContent) {
        try {
            // Parse existing content
            var existingData = ParseInput(existingContent);
            
            // Merge with new inputs
            foreach (var kvp in inputs) {
                if (kvp.Value is Dictionary<string, object> sectionDict) {
                    var sectionName = kvp.Key;
                    
                    if (!existingData.ContainsKey(sectionName)) {
                        existingData[sectionName] = new Dictionary<string, object>();
                    }
                    
                    var existingSection = (Dictionary<string, object>)existingData[sectionName];
                    foreach (var keyValue in sectionDict) {
                        existingSection[keyValue.Key] = keyValue.Value?.ToString() ?? "";
                    }
                }
            }

            // Serialize back to string
            var stringBuilder = new StringBuilder();
            
            foreach (var kvp in existingData) {
                var sectionName = kvp.Key;
                var sectionDict = (Dictionary<string, object>)kvp.Value;
                
                foreach (var keyValue in sectionDict) {
                    var value = keyValue.Value?.ToString() ?? "";
                    // Escape quotes in the value if needed
                    if (value.Contains('"')) {
                        value = value.Replace("\"", "\\\"");
                    }
                    stringBuilder.AppendLine($"{sectionName} {keyValue.Key} \"{value}\"");
                }
            }

            return stringBuilder.ToString().TrimEnd('\r', '\n');
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to merge/serialize CFG: {ex.Message}");
        }
    }

    private (string section, string key, string value)? ParseCfgLine(string line) {
        // Pattern to match: <any string> variable "value"
        // This handles:
        // - Section names with spaces
        // - Variable names
        // - Quoted values (with optional escaping)
        
        // First, find the last quoted string (the value)
        var lastQuoteIndex = line.LastIndexOf('"');
        if (lastQuoteIndex == -1) {
            return null; // No quoted value found
        }

        // Find the opening quote for the value
        var openingQuoteIndex = line.LastIndexOf('"', lastQuoteIndex - 1);
        if (openingQuoteIndex == -1) {
            return null; // No opening quote found
        }

        // Extract the value (between the quotes)
        var value = line.Substring(openingQuoteIndex + 1, lastQuoteIndex - openingQuoteIndex - 1);
        
        // Unescape quotes in the value
        value = value.Replace("\\\"", "\"");

        // Extract the part before the value (section and key)
        var beforeValue = line.Substring(0, openingQuoteIndex).Trim();
        
        // Find the last space to separate section from key
        var lastSpaceIndex = beforeValue.LastIndexOf(' ');
        if (lastSpaceIndex == -1) {
            return null; // No space found to separate section and key
        }

        var section = beforeValue.Substring(0, lastSpaceIndex).Trim();
        var key = beforeValue.Substring(lastSpaceIndex + 1).Trim();

        if (string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(key)) {
            return null; // Invalid section or key
        }

        return (section, key, value);
    }
} 