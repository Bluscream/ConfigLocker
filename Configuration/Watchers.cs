﻿using ConfigLocker;
using Microsoft.Extensions.FileSystemGlobbing;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using Menelabs;

#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

public enum ConfigType {
    Unknown,
    JSON,
    INI,
    VDF
}
public partial class Configuration {
    // <auto-generated />
    //
    // To parse this JSON data, add NuGet 'System.Text.Json' then do:
    //
    //    using ConfigLocker;
    //
    //    var sourceList = SourceList.FromJson(jsonString);
    #region NLog
    [JsonPropertyName("watchers")]
    public virtual List<Watcher> Watchers { get; set; } = new();
    #endregion NLog
}
public partial class Watcher {
    [JsonPropertyName("enabled")]
    public virtual bool Enabled { get; set; } = true;
    [JsonPropertyName("name")]
    public virtual string Name { get; set; }
    [JsonPropertyName("description")]
    public virtual string Description { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public virtual ConfigType? Type { get; set; }
    [JsonPropertyName("inputs")]
    [ConfigurationKeyName("inputs")]
    public virtual List<string> InputPaths { get; set; } = new();
    [JsonPropertyName("output")]
    [ConfigurationKeyName("output")]
    public virtual string OutputPath { get; set; }
    [JsonPropertyName("checkonchange")]
    public virtual bool CheckOnChange { get; set; } = true;
    [JsonPropertyName("checkonstartup")]
    public virtual bool CheckOnStartup { get; set; } = true;
    [JsonPropertyName("checkevery")]
    public virtual TimeSpan CheckEvery { get; set; } = TimeSpan.FromMinutes(30);

    [JsonIgnore]
    public virtual IEnumerable<FileInfo> Inputs { get { return InputPaths.Select(x => new FileInfo(x)); } }
    [JsonIgnore]
    public Dictionary<string, object?> CombinedInputs { get; private set; }
    [JsonIgnore]
    public virtual FileInfo Output { get { return new FileInfo(OutputPath); } }
    [JsonIgnore]
    public FileSystemSafeWatcher? OutputWatcher { get; private set; }
    //[JsonIgnore]
    //private DateTime LastRead = DateTime.MinValue;

    private ConfigType GetFileTypeFromExtension(FileInfo file) {
        var ext = file.Extension.ToLowerInvariant();
        switch (ext) {
            case ".json": return ConfigType.JSON;
            case ".ini": case ".cfg": return ConfigType.INI;
            case ".vdf": return ConfigType.VDF;
        }
        return ConfigType.Unknown;
    }
    public ConfigType GetType() {
        if (Type.HasValue && Type != ConfigType.Unknown) return Type.Value;
        if (OutputPath != null) return GetFileTypeFromExtension(Output);
        foreach (var inputFile in Inputs) {
            var fileType = GetFileTypeFromExtension(inputFile);
            if (fileType != ConfigType.Unknown) return fileType;
        }
        return ConfigType.Unknown;
    }
    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Enabled ? "✅ " : "❌ ");
        sb.Append(Name?.Quote() ?? Output?.Name?.Quote() ?? $"Unnamed Watcher");
        sb.Append($" [{ConfigType.GetName(GetType())}]");
        //if (OutputPath != null) sb.Append(" " + OutputPath.ToString().Quote());
        return sb.ToString();
    }
    public string DebugString() {
        var sb = new StringBuilder("[DEBUG] " + ToString());
        if (OutputPath != null) sb.Append($"{Environment.NewLine}Output: {Output.FullName.Quote()} {Output.StatusString(true)}");
        if (Inputs.Count() < 1) sb.Append($"{Environment.NewLine}⚠️ No Inputs defined");
        var i = 0;
        foreach (var inputFile in Inputs) {
            i++;
            sb.Append($"{Environment.NewLine}Input #{i}: {inputFile.FullName.Quote()} {inputFile.StatusString(true)}");
        }
        return sb.ToString();
    }

    public bool Check() {
        var ret = true;
        if (!Enabled ||
            string.IsNullOrWhiteSpace(OutputPath) ||
            Output is null || Directory.Exists(OutputPath) || !Output.Exists ||
            InputPaths is null || InputPaths.Count < 1 || InputPaths.Any(string.IsNullOrWhiteSpace)
        ) ret = false;
        if (!ret) Enabled = false;
        return ret;
    }

    public bool Toggle() {
        if (OutputWatcher != null) {
            Stop();
            return false;
        } else {
            Start();
            return true;
        }
    }
    public void Start() {
        if (!Check()) {
            Program.Log($"❌ Something is wrong with watcher {this}, disabling it...");
            return;
        }
        if (OutputWatcher != null) {
            Program.Log($"⚠️ Tried to start watcher {this} which is already running, restarting it instead...");
            Stop(); Start(); return;
        }
        ParseInputs();
        OutputWatcher = new FileSystemSafeWatcher(Output.DirectoryName);
        OutputWatcher.Filter = Output.Name;
        OutputWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
        //OutputWatcher.Created += OnOutputWatcherTriggered;
        OutputWatcher.Changed += OnOutputWatcherTriggered;
        //OutputWatcher.Deleted += OnOutputWatcherTriggered;
        OutputWatcher.EnableRaisingEvents = true;
    }
    public void Stop() {
        if (OutputWatcher is null) {
            Program.Log($"⚠️ Tried to stop watcher {this} which is not running, discarding...");
        }
        OutputWatcher.EnableRaisingEvents = false;
        OutputWatcher.Created -= OnOutputWatcherTriggered;
        OutputWatcher.Changed -= OnOutputWatcherTriggered;
        OutputWatcher.Deleted -= OnOutputWatcherTriggered;
        OutputWatcher.Dispose();
        OutputWatcher = null;
        Program.Log($"No longer watching {this}");
    }

    private void OnOutputWatcherTriggered(object sender, FileSystemEventArgs e) {
        if (!OutputWatcher.EnableRaisingEvents) return;
        OutputWatcher.EnableRaisingEvents = false;
        //Program.Log($"OnOutputWatcherTriggered: {e.FullPath} {e.ChangeType}");
        OnChanged(Output.ReadAllText());
        OutputWatcher.EnableRaisingEvents = true;
    }

    internal void OnChanged(string NewContent) {
        Program.Log($"OnChanged: {NewContent.Length} chars");
        OutputWatcher.EnableRaisingEvents = false;
        SetOutput(CombinedInputs);
        OutputWatcher.EnableRaisingEvents = true;
    }

    internal Dictionary<string, object?> ParseInputs() {
        var ret = new Dictionary<string, object?>();
        foreach (var input in Inputs) {
            var text = input.ReadAllText();
            // ret.MergeRecursiveWith(input);
        }
        CombinedInputs = ret;
        return ret;
    }

    internal bool SetOutput(Dictionary<string, object?> inputsDict) {
        try {
            var text = Output.ReadAllText();
            // text.MergeRecursiveWith(inputsDict);
        } catch (Exception ex) {
            Program.Log($"{Name} SetOutput failed: {ex.Message}"); return false;
        }
        return true;
    }
}

#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603