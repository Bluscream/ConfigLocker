using System.Text;

namespace ConfigLocker;

public class IniConfigProcessor : IConfigProcessor {
    public bool SupportsMerging => true;

    public Dictionary<string, object> ParseInput(string content) {
        if (string.IsNullOrWhiteSpace(content)) {
            return new Dictionary<string, object>();
        }

        try {
            var result = new Dictionary<string, object>();
            var currentSection = "[Global]";
            var currentSectionDict = new Dictionary<string, object>();

            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines) {
                var trimmedLine = line.Trim();
                
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(';') || trimmedLine.StartsWith('#')) {
                    continue;
                }

                // Check if this is a section header
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']')) {
                    // Save the previous section
                    if (currentSectionDict.Count > 0) {
                        result[currentSection] = new Dictionary<string, object>(currentSectionDict);
                        currentSectionDict.Clear();
                    }
                    
                    currentSection = trimmedLine;
                }
                else {
                    // This is a key-value pair
                    var equalIndex = trimmedLine.IndexOf('=');
                    if (equalIndex > 0) {
                        var key = trimmedLine.Substring(0, equalIndex).Trim();
                        var value = trimmedLine.Substring(equalIndex + 1).Trim();
                        currentSectionDict[key] = value;
                    }
                }
            }

            // Save the last section
            if (currentSectionDict.Count > 0) {
                result[currentSection] = new Dictionary<string, object>(currentSectionDict);
            }

            return result;
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to parse INI: {ex.Message}");
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
                
                if (sectionName != "[Global]") {
                    stringBuilder.AppendLine(sectionName);
                }
                
                foreach (var keyValue in sectionDict) {
                    stringBuilder.AppendLine($"{keyValue.Key}={keyValue.Value}");
                }
                
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEnd('\r', '\n');
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to merge/serialize INI: {ex.Message}");
        }
    }
}
