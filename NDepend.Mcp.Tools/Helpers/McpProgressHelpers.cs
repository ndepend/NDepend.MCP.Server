using ModelContextProtocol;

namespace NDepend.Mcp.Helpers {
    internal static class McpProgressHelpers {

        internal static Action<int>? GetReportProgressProc(
                    McpServer server, 
                    RequestContext<CallToolRequestParams> context, 
                    CancellationToken cancellationToken) {
            var progressToken = context.Params?.ProgressToken;
            Action<int>? reportProgressProc = null;
            if (progressToken != null) {
                reportProgressProc = async (progressPercent) => {
                    await server.NotifyProgressAsync(progressToken.Value, new ProgressNotificationValue {
                        Progress = progressPercent,
                        Total = 100,
                    }, cancellationToken: cancellationToken);
                };
            }
            return reportProgressProc;
        }


    }
}
