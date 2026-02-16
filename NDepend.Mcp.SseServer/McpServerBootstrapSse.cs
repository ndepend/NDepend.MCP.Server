
using System.CommandLine;
using NDepend.Mcp.Server;
using Serilog;
using Serilog.Events;

namespace NDepend.Mcp.SseServer {
    internal sealed class McpServerBootstrapSse : McpServerBootstrapBase {

        internal override string Kind => "Sse";

        internal bool TryParseArgument(
                string[] args,
                out string? serverUrl,
                out string? logDirPath,
                out LogEventLevel minimumLogLevel,
                out string? ndpProjectPath) {

            var portOption = new Option<int>("--port") {
                Description = "The port number for the NDepend MCP server to listen on.",
                DefaultValueFactory = x => 3001
            };

            if(!TryParseArgument(
                    args, 
                    [portOption],
                    out logDirPath,
                    out minimumLogLevel,
                    out ndpProjectPath,
                    out ParseResult? parseResult)) {
                serverUrl = null;
                return false;
            }

            int port = parseResult!.GetValue(portOption);
            serverUrl = $"http://localhost:{port}";
            return true;
        }


        protected override LoggerConfiguration ExtraConfiguration(LoggerConfiguration loggerConfiguration) {
            return loggerConfiguration
                // For debugging connection issues, set AspNetCore to Information or Debug
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", LogEventLevel.Debug); // Kestrel connection logs;
        }
    }
}
