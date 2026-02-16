using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Dependency;

[Description($"This class enables pagination of the server responses returned by calls to the MCP tool `{DependencyTools.TOOL_LIST_DEPENDENCIES_NAME}`.")]
public class ListDependenciesPaginatedResult : PaginatedResult {

    public ListDependenciesPaginatedResult(
            IEnumerable<DependencyInfo> metricsPaginated,
            PaginatedResult pr) : base(pr) {
        Metrics = metricsPaginated;
    }

    [Description("Gets or sets the sequence of dependencies paginated.")]
    public IEnumerable<DependencyInfo> Metrics { get; set; } = [];
}
