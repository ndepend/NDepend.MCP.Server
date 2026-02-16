using ModelContextProtocol;
using NDepend.Path;

namespace NDepend.Mcp.Helpers {
    internal static class LogHelpers {
        internal static readonly string TOOL_LOG_SEPARATOR = new string('-', 128);

        extension(ILogger logger) {
            internal void LogErrorAndThrow(string err) {
                logger.LogError(err);
                throw new McpException(err);
            }

        }

        extension<T>(ILogger<T> logger) {
            internal void LogErrorAndThrow(string err) {
                logger.LogError(err);
                throw new McpException(err);
            }

            internal McpException LogErrorAndGetException(string err) {
                logger.LogError(err);
                return new McpException(err);
            }

            internal void LogCannotExportGraphError(IAbsoluteFilePath tempHtmlPath, string failureReason) =>
                logger.LogError(
                    $"""
                     Html Svg cannot be exported to {tempHtmlPath}.
                     Reason: {failureReason}
                     """);
        }
    }
}
