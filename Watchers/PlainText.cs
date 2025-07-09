using System.Text;

namespace ConfigLocker;

public class PlainTextConfigProcessor : IConfigProcessor {
    public bool SupportsMerging => false; // Plaintext overwrites the entire file

    public Dictionary<string, object> ParseInput(string content) {
        if (string.IsNullOrWhiteSpace(content)) {
            return new Dictionary<string, object>();
        }

        // For plaintext, we store the content as a single key-value pair
        // The key represents the filename or identifier, and the value is the content
        return new Dictionary<string, object> {
            ["content"] = content
        };
    }

    public string MergeAndSerialize(Dictionary<string, object> inputs, string existingContent) {
        // For plaintext, we don't merge - we just return the combined content from all inputs
        var stringBuilder = new StringBuilder();
        
        foreach (var kvp in inputs) {
            if (kvp.Value is string strValue) {
                stringBuilder.AppendLine(strValue);
            } else if (kvp.Value is Dictionary<string, object> dict) {
                // If it's a nested dictionary (from ParseInput), extract the content
                if (dict.ContainsKey("content") && dict["content"] is string content) {
                    stringBuilder.AppendLine(content);
                }
            } else {
                stringBuilder.AppendLine(kvp.Value?.ToString() ?? "");
            }
        }

        return stringBuilder.ToString().TrimEnd('\r', '\n');
    }
} 