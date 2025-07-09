#pragma warning disable CS8604
#pragma warning disable CS8602

using ConfigLocker;
using System.Text.Json;

public class JsonConfigProcessor : IConfigProcessor {
    public bool SupportsMerging => true;

    public Dictionary<string, object> ParseInput(string content) {
        if (string.IsNullOrWhiteSpace(content)) {
            return new Dictionary<string, object>();
        }
        
        try {
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            return result ?? new Dictionary<string, object>();
        } catch (JsonException ex) {
            throw new InvalidOperationException($"Failed to parse JSON: {ex.Message}");
        }
    }

    public string MergeAndSerialize(Dictionary<string, object> inputs, string existingContent) {
        try {
            var existingDict = string.IsNullOrWhiteSpace(existingContent) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(existingContent) ?? new Dictionary<string, object>();
            
            var finalDict = existingDict.MergeRecursiveWith(inputs);
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(finalDict, options);
        } catch (JsonException ex) {
            throw new InvalidOperationException($"Failed to merge/serialize JSON: {ex.Message}");
        }
    }
}

// Legacy class for backward compatibility
public partial class JsonWatcher : Watcher {
    // This class is now deprecated in favor of the new interface-based approach
    // It's kept for backward compatibility but the functionality is now in JsonConfigProcessor
}