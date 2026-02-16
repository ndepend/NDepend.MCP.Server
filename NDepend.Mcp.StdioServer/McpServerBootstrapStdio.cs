
using System.CommandLine;
using NDepend.Mcp.Server;
using Serilog.Events;

namespace NDepend.Mcp.StdioServer {
    internal sealed class McpServerBootstrapStdio : McpServerBootstrapBase {

        internal override string Kind => "Stdio";

        internal bool TryParseArgument(
             string[] args,
             out string? logDirPath,
             out LogEventLevel minimumLogLevel,
             out string? ndpProjectPath) {

            return TryParseArgument(
                args, 
                Array.Empty<Option>(),
                out logDirPath,
                out minimumLogLevel,
                out ndpProjectPath,
                out _);
        }
    }
}
