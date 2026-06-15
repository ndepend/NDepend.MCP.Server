using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using NDepend.Mcp.Helpers;
using Serilog.Events;

namespace NDepend.Mcp.StdioServer;




public static class Program {
  
    // Need a static field to prevent GC collection. Creating it also hooks AssemblyResolve and picks
    // ..\Lib (dev tree) or ..\..\..\..\ndepend\Lib (redistributable) via NDependRuntimeContext.

    static readonly AssemblyResolver s_AssemblyResolver = NDependRuntimeContext.RegisterAssemblyResolver();
	
    public static async Task<int> Main(string[] args) {
        AppDomain.CurrentDomain.AssemblyResolve += s_AssemblyResolver.AssemblyResolveHandler;

        var bootstrap = new McpServerBootstrapStdio();

        if(!bootstrap.TryParseArgument(args, 
               out string? logDirPath,
               out LogEventLevel minimumLogLevel,
               out string? ndpProjectPath)) {
            return 1;
        }
        
        var loggerConfiguration = bootstrap.InitLoggerConfiguration(minimumLogLevel);

        if (!bootstrap.TryInitLogDirPath(logDirPath, loggerConfiguration, minimumLogLevel)) {
            return 1;
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        bootstrap.InitServices(builder).WithStdioServerTransport();
        
        var host = builder.Build();

        var logger = bootstrap.BuildBootstrapLogger(host);
        try {
            logger.LogInformation(new string('-', 128));
            logger.LogInformation(
                $"Starting {bootstrap.ApplicationName} v{bootstrap.ApplicationVersion} " +
                $"PID:{Process.GetCurrentProcess().Id}");

            if (!string.IsNullOrEmpty(ndpProjectPath)) {
                bootstrap.LoadNDependProject(ndpProjectPath, host, logger);
            }

            await host.RunAsync();
            return 0;
        } catch (Exception ex) {
            logger.LogError(ex, $"{bootstrap.ApplicationName} terminated unexpectedly.");
            return 1;
        } finally {
            logger.LogInformation($"{bootstrap.ApplicationName} shutting down.");
            await Log.CloseAndFlushAsync();
        }
    }

    
}

