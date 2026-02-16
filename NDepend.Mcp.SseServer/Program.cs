
using Microsoft.AspNetCore.HttpLogging;
using NDepend.Mcp.Helpers;
using Serilog;
using Serilog.Events;

namespace NDepend.Mcp.SseServer;

public class Program  {
    // --- Application ---

    // Need a static field to prevent GC collection
    static readonly AssemblyResolver s_AssemblyResolver = new(@"..\..\..\..\ndepend\Lib");

    public static async Task<int> Main(string[] args) {

        AppDomain.CurrentDomain.AssemblyResolve += s_AssemblyResolver.AssemblyResolveHandler;

        var bootstrap = new McpServerBootstrapSse();

        if (!bootstrap.TryParseArgument(args,
               out string? serverUrl,
               out string? logDirPath,
               out LogEventLevel minimumLogLevel,
               out string? ndpProjectPath)) {
            return 1;
        }

        var loggerConfiguration = bootstrap.InitLoggerConfiguration(minimumLogLevel);

        if (!bootstrap.TryInitLogDirPath(logDirPath, loggerConfiguration, minimumLogLevel)) {
            return 1;
        }

        try {
            Log.Information(
                $"Configuring {bootstrap.ApplicationName} v{bootstrap.ApplicationVersion} " +
                $"to run on {serverUrl} with minimum log level {minimumLogLevel}");

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args });

            builder.Host.UseSerilog();

            // Add W3CLogging for detailed HTTP request logging
            // This logs to Microsoft.Extensions.Logging, which Serilog will capture.
            builder.Services.AddW3CLogging(logging => {
                logging.LoggingFields = W3CLoggingFields.All; // Log all available fields
                logging.FileSizeLimit = 5 * 1024 * 1024; // 5 MB
                logging.RetainedFileCountLimit = 2;
                logging.FileName = "access-"; // Prefix for log files
                                              // By default, logs to a 'logs' subdirectory of the app's content root.
                                              // Can be configured: logging.RootPath = ...
            });

            bootstrap.InitServices(builder).WithHttpTransport();

            WebApplication app = builder.Build();

            var logger = bootstrap.BuildBootstrapLogger(app);
            if (!string.IsNullOrEmpty(ndpProjectPath)) {
                bootstrap.LoadNDependProject(ndpProjectPath, app, logger);
            }

            // --- ASP.NET Core Middleware ---

            // 1. W3C Logging Middleware (if enabled and configured to log to a file separate from Serilog)
            //    If W3CLogging is configured to write to files, it has its own middleware.
            //    If it's just for ILogger, Serilog picks it up.
            // app.UseW3CLogging(); // This is needed if W3CLogging is writing its own files.
            // If it's just feeding ILogger, Serilog handles it.

            // 2. Custom Request Logging Middleware (very early in the pipeline)
            app.Use(async (context, next) => {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogDebug("Incoming Request: {Method} {Path} {QueryString} from {RemoteIpAddress}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Connection.RemoteIpAddress);

                // Log headers for more detail if needed (can be verbose)
                // foreach (var header in context.Request.Headers) {
                //     logger.LogTrace("Header: {Key}: {Value}", header.Key, header.Value);
                // }
                try {
                    await next(context);
                } catch (Exception ex) {
                    logger.LogError(ex, "Error processing request: {Method} {Path}", context.Request.Method, context.Request.Path);
                    throw; // Re-throw to let ASP.NET Core handle it
                }

                logger.LogDebug("Outgoing Response: {StatusCode} for {Method} {Path}",
                    context.Response.StatusCode,
                    context.Request.Method,
                    context.Request.Path);
            });


            // 3. Standard ASP.NET Core middleware (HTTPS redirection, routing, auth, etc. - not used here yet)
            // if (app.Environment.IsDevelopment()) { }
            // app.UseHttpsRedirection(); 

            // 4. MCP Middleware
            app.MapMcp(); // Maps the MCP endpoint (typically "/mcp")

            Log.Information($"Starting {bootstrap.ApplicationName} server...");
            await app.RunAsync(serverUrl);

            return 0;

        } catch (Exception ex) {
            Log.Fatal(ex, $"{bootstrap.ApplicationName} terminated unexpectedly.");
            return 1;
        } finally {
            Log.Information($"{bootstrap.ApplicationName} shutting down.");
            await Log.CloseAndFlushAsync();
        }
    }
}