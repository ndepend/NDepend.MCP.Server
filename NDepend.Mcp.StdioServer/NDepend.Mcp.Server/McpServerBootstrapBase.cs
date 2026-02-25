
using System.CommandLine;
using System.Reflection;
using ModelContextProtocol.Protocol;
using NDepend.Mcp.Services;
using Serilog;
using Serilog.Events;

// Those usings are only required in the StdioServer project
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace NDepend.Mcp.Server {
    internal abstract class McpServerBootstrapBase {

        internal abstract string Kind { get; }

        internal string ApplicationName => $"NDependMcp{Kind}Server";
        internal string ApplicationVersion => Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString()
            ?? "unknown";

        
        protected bool TryParseArgument(
                string[] args,
                IEnumerable<Option> extraOptions,
                out string? logDirPath,
                out LogEventLevel minimumLogLevel,
                out string? ndpProjectPath,
                out ParseResult? parseResult) {
            var logDirOption = new Option<string?>("--log-directory") {
                Description = "Optional path to a log directory. If not specified, logs only go to console."
            };

            var logLevelOption = new Option<LogEventLevel>("--log-level") {
                Description = "Minimum log level for console and file.",
                DefaultValueFactory = x => LogEventLevel.Information
            };

            var loadNdpProjectOption = new Option<string?>("--load-ndepend-project") {
                Description = "Path to an NDepend project file (.ndproj) to load immediately on startup."
            };

            var rootCommand = new RootCommand($"NDepend MCP {Kind} Server"){
                logDirOption,
                logLevelOption,
                loadNdpProjectOption
            };
            foreach(var option in extraOptions) {
                rootCommand.Add(option);
            }

            parseResult = rootCommand.Parse(args);
            if (parseResult.Errors.Any()) {
                Console.Error.WriteLine(
                   "Failed to parse command line arguments." + 
                   Environment.NewLine +
                   string.Join(Environment.NewLine, parseResult.Errors.Select(e => e.Message)));
                logDirPath = null;
                minimumLogLevel = LogEventLevel.Information;
                ndpProjectPath = null;
                return false;
            }

            logDirPath = parseResult.GetValue(logDirOption);
            minimumLogLevel = parseResult.GetValue(logLevelOption);
            ndpProjectPath = parseResult.GetValue(loadNdpProjectOption);
            return true;
        }

        internal const string LOG_OUTPUT_TEMPLATE = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        protected virtual LoggerConfiguration ExtraConfiguration(LoggerConfiguration loggerConfiguration) {
            return loggerConfiguration;
        }

        internal LoggerConfiguration InitLoggerConfiguration(LogEventLevel minimumLogLevel) {
            return ExtraConfiguration(
                    new LoggerConfiguration()
                    .MinimumLevel.Is(minimumLogLevel)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.CodeAnalysis", LogEventLevel.Information)
                    .MinimumLevel.Override("ModelContextProtocol", LogEventLevel.Warning))
                    
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.Console(
                outputTemplate: LOG_OUTPUT_TEMPLATE,
                standardErrorFromLevel: LogEventLevel.Verbose,
                restrictedToMinimumLevel: minimumLogLevel));
        }

        internal bool TryInitLogDirPath(string? logDirPath, LoggerConfiguration loggerConfiguration, LogEventLevel minimumLogLevel) {
            // Default log directory if not specified, in .\artifacts\logs
            // or %AppContext.BaseDirectory%\logs  if output dir has been modified
            if (logDirPath == null) {
                string baseDir = AppContext.BaseDirectory;
                const string artifactsDirName = "artifacts";
                const string logsDirName = "logs";
                int index = baseDir.IndexOf(artifactsDirName, StringComparison.OrdinalIgnoreCase);
                if (index > 0) { baseDir = baseDir.Substring(0, index + artifactsDirName.Length); }
                logDirPath = Path.Combine(baseDir, logsDirName);
            }

            if (!string.IsNullOrWhiteSpace(logDirPath)) {

                if (!Directory.Exists(logDirPath)) {
                    Console.Error.WriteLine($"Log directory does not exist. Creating: {logDirPath}");
                    try {
                        Directory.CreateDirectory(logDirPath);
                    } catch (Exception ex) {
                        Console.Error.WriteLine($"Failed to create log directory: {ex.Message}");
                        return false;
                    }
                }
                string logFilePath = Path.Combine(logDirPath, $"{ApplicationName}-.log");
                loggerConfiguration.WriteTo.Async(a => a.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: LOG_OUTPUT_TEMPLATE,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7,
                    restrictedToMinimumLevel: minimumLogLevel));
                Console.Error.WriteLine($"Logging to file: {Path.GetFullPath(logDirPath)} with minimum level {minimumLogLevel}");
            }

            Log.Logger = loggerConfiguration.CreateBootstrapLogger();
            return true;
        }


        internal IMcpServerBuilder InitServices(IHostApplicationBuilder builder) {
            builder.Services.WithNDependToolsServices();
            return builder.Services
                .AddMcpServer(options => {
                    options.ServerInfo = new Implementation {
                        Name = this.ApplicationName,
                        Version = this.ApplicationVersion,
                    };
                })
                .WithNDependTools();
        }

        internal Microsoft.Extensions.Logging.ILogger BuildBootstrapLogger(IHost host) {
            return host.Services.GetRequiredService<ILogger<BootstrapLogCategory>>();
        }

        internal void LoadNDependProject(string ndpProjectPath, IHost host, Microsoft.Extensions.Logging.ILogger logger) {
            try {
                var ndependService = host.Services.GetRequiredService<INDependService>();

                logger.LogInformation($"Loading NDepend project: `{ndpProjectPath}`", ndpProjectPath);
                ndependService.InitializeFromProject(ndpProjectPath, logger, _ => { }, out _);

            } catch (Exception ex) {
                logger.LogError(ex, $"Error loading NDepend project: ${ndpProjectPath}");
            }
        }
    }
}
