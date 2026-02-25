using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Metric {
    [Description($"Enables pagination for `{MetricTools.TOOL_SEARCH_CODE_METRICS_NAME}` MCP tool responses.")]
    public sealed class ListMetricsPaginatedResult : PaginatedResult {

        public ListMetricsPaginatedResult(
             IEnumerable<MetricInfo> metricsPaginated,
             PaginatedResult pr) : base(pr) {
            Metrics = metricsPaginated;
        }

        [Description("Sequence of paginated metrics")]
        public IEnumerable<MetricInfo> Metrics { get; set; } = [];
    }
}
