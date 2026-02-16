using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Metric {
    [Description($"This class enables pagination of the server responses returned by calls to the MCP tool `{MetricTools.TOOL_SEARCH_CODE_METRICS_NAME}`.")]
    public sealed class ListMetricsPaginatedResult : PaginatedResult {

        public ListMetricsPaginatedResult(
             IEnumerable<MetricInfo> metricsPaginated,
             PaginatedResult pr) : base(pr) {
            Metrics = metricsPaginated;
        }

        [Description("Gets or sets the sequence of metrics paginated.")]
        public IEnumerable<MetricInfo> Metrics { get; set; } = [];
    }
}
