#pragma warning disable CS8604
#pragma warning disable CS8602

using ConfigLocker;
using System.Text.Json;

public partial class JsonWatcher : Watcher {

    internal override Dictionary<string, object> ParseInputs() {
        var ret = new Dictionary<string, object>();
        foreach (var input in Inputs) {
            if (!input.Exists) continue;
            var inputText = input.ReadAllText();
            var inputDict = JsonSerializer.Deserialize<Dictionary<string, object>>(inputText);
            ret = ret.MergeRecursiveWith(inputDict);
        }
        CombinedInputs = ret;
        return ret;
    }

    internal override bool SetOutput(Dictionary<string, object>? inputsDict = null) {
        try {
            inputsDict = inputsDict ?? CombinedInputs;
            var outputText = Output.ReadAllText();
            var outputDict = JsonSerializer.Deserialize<Dictionary<string, object>>(outputText);
            var finalDict = outputDict.MergeRecursiveWith(inputsDict);
            var finalText = JsonSerializer.Serialize(finalDict);
            if (outputText.Length == finalText.Length) {
                Program.Log($"{this} > outputText and finalText are the same ({finalText.Length})");
                return true;
            }
            Output.Backup(true);
            Output.WriteAllText(finalText);
        } catch (Exception ex) {
            Program.Log($"{Name} SetOutput failed: {ex.Message}"); return false;
        }
        return true;
    }
}