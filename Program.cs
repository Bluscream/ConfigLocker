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
            
            foreach (Watcher watcher in Config.Watchers) {
                Log($"🔍 Processing watcher: {watcher}");
                Log(watcher.DebugString());
                
                if (watcher.OutputPath is null) { 
                    Log($"❌ Watcher {watcher} Output path is invalid"); 
                    continue; 
                }
                
                if (watcher.Output is null || !watcher.Output.Exists) { 
                    Log($"❌ Watcher {watcher} Output path {watcher.OutputPath?.Quote()} does not exist"); 
                    continue; 
                }
                
                var configType = watcher.GetConfigType();
                if (configType == ConfigType.Unknown) {
                    Log($"❌ Watcher type for {watcher} could not be determined, please set it manually");
                    continue;
                }
                
                Log($"✅ Adding watcher {watcher} with type {configType}");
                Watchers.Add(watcher);
            }
            
            Log($"🚀 Starting {Watchers.Count} watchers...");
            foreach (var watcher in Watchers) {
                watcher.Start();
            }

            //var runner = servicesProvider.GetRequiredService<Runner>();
            //runner.DoAction("Action1");

            Console.WriteLine("✅ ConfigLocker is running in the background now, press ANY key to exit");
            Console.WriteLine("📊 Supported formats: JSON, XML, INI, PlainText");
            Console.WriteLine("🔄 Watchers will automatically merge/overwrite config files when changes are detected");
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