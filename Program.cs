using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.ILoggerFactory;
using NLog;
using NLog.Extensions;
using NLog.Extensions.Logging;
using System.IO;
using System.Text;
using NLog.Targets;
namespace ConfigLocker;
static class Program {
    private const string ConfigFileName = "ConfigLocker.json";
    private const string WatchersDirectory = "config-watchers";
    //private static readonly Encoding EncodingUTF8 = Encoding.GetEncoding(65001);
    internal static Logger Logger { get; private set; } = LogManager.GetCurrentClassLogger();
    internal static Configuration Config;
    internal static List<Watcher> Watchers { get; private set; } = new();

    internal static void Log(object? arg) {
        if (arg is null) return;
        Logger.Info(arg.ToString());
        Console.WriteLine($"[{DateTime.Now}] {arg}");
    }

    static void Main(string[] args) {
        try {
            Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;
            var currentDir = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(currentDir); //From NuGet Package Microsoft.Extensions.Configuration.Json
            builder.AddJsonFile(ConfigFileName, optional: false, reloadOnChange: true);
            Config = builder.Build().Get<Configuration>();
            if (Config == null) { throw new ArgumentNullException("config"); }

            using var servicesProvider = new ServiceCollection()
                .AddTransient<Runner>() // Runner is the custom class
                .AddLogging(loggingBuilder => {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    //loggingBuilder.AddNLog(builder);
                }).BuildServiceProvider();
            
            Log("🔧 ConfigLocker starting up...");
            Log($"📁 Working directory: {currentDir}");
            Log($"📄 Config file: {ConfigFileName}");
            
            // Load watchers from the watchers directory if it exists
            var watchersFromDirectory = LoadWatchersFromDirectory();
            if (watchersFromDirectory.Any()) {
                Log($"📂 Loaded {watchersFromDirectory.Count} watchers from {WatchersDirectory} directory");
                foreach (var watcher in watchersFromDirectory) {
                    ProcessWatcher(watcher);
                }
            } else {
                // Fallback to loading from the main config file
                Log($"📂 No watchers found in {WatchersDirectory} directory, using main config file");
                foreach (Watcher watcher in Config.Watchers) {
                    ProcessWatcher(watcher);
                }
            }
            
            Log($"🚀 Starting {Watchers.Count} watchers...");
            foreach (var watcher in Watchers) {
                watcher.Start();
            }

            //var runner = servicesProvider.GetRequiredService<Runner>();
            //runner.DoAction("Action1");

            Console.WriteLine("✅ ConfigLocker is running in the background now, press ANY key to exit");
            Console.WriteLine("📊 Supported formats: JSON, XML, INI, CFG, PlainText");
            Console.WriteLine("🔄 Watchers will automatically merge/overwrite config files when changes are detected");
            Console.WriteLine($"📂 Watchers can be configured in individual files in the '{WatchersDirectory}' directory");
            Console.ReadLine();
            
            Log("🛑 Shutting down ConfigLocker...");
            foreach (var watcher in Watchers) {
                watcher.Stop();
            }
            //Config.JsonToFile(ConfigFileName);
        } catch (Exception ex) {
            Logger.Error(ex, "Stopped program because of exception");
            throw;
        } finally {
            LogManager.Shutdown();
        }
    }

    private static List<Watcher> LoadWatchersFromDirectory() {
        var watchers = new List<Watcher>();
        var watchersDir = Path.Combine(Directory.GetCurrentDirectory(), WatchersDirectory);
        
        if (!Directory.Exists(watchersDir)) {
            Log($"📂 {WatchersDirectory} directory does not exist");
            return watchers;
        }

        var jsonFiles = Directory.GetFiles(watchersDir, "*.json", SearchOption.TopDirectoryOnly);
        Log($"📂 Found {jsonFiles.Length} JSON files in {WatchersDirectory} directory");

        foreach (var jsonFile in jsonFiles) {
            try {
                var fileName = Path.GetFileName(jsonFile);
                Log($"📄 Loading watchers from: {fileName}");
                
                var jsonContent = File.ReadAllText(jsonFile);
                var watcherConfig = System.Text.Json.JsonSerializer.Deserialize<WatcherConfig>(jsonContent);
                
                if (watcherConfig?.Watchers != null) {
                    foreach (var watcher in watcherConfig.Watchers) {
                        watchers.Add(watcher);
                        Log($"✅ Loaded watcher '{watcher.Name}' from {fileName}");
                    }
                } else {
                    Log($"⚠️ No watchers found in {fileName}");
                }
            } catch (Exception ex) {
                Log($"❌ Failed to load watchers from {Path.GetFileName(jsonFile)}: {ex.Message}");
            }
        }

        return watchers;
    }

    private static void ProcessWatcher(Watcher watcher) {
        Log($"🔍 Processing watcher: {watcher}");
        Log(watcher.DebugString());
        
        if (watcher.OutputPath is null) { 
            Log($"❌ Watcher {watcher} Output path is invalid"); 
            return; 
        }
        
        if (watcher.Output is null || !watcher.Output.Exists) { 
            Log($"❌ Watcher {watcher} Output path {watcher.OutputPath?.Quote()} does not exist"); 
            return; 
        }
        
        var configType = watcher.GetConfigType();
        if (configType == ConfigType.Unknown) {
            Log($"❌ Watcher type for {watcher} could not be determined, please set it manually");
            return;
        }
        
        Log($"✅ Adding watcher {watcher} with type {configType}");
        Watchers.Add(watcher);
    }
}

// Configuration class for individual watcher files
public class WatcherConfig {
    public List<Watcher> Watchers { get; set; } = new();
}

public class Runner {
    private readonly ILogger<Runner> _logger;
    public Runner(ILogger<Runner> logger) {
        _logger = logger;
    }
    public void DoAction(string name) {
        _logger.LogDebug(20, "Doing hard work! {Action}", name);
    }
}