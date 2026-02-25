using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Dependency;

[Description($"Enables pagination for `{DependencyTools.TOOL_LIST_DEPENDENCIES_NAME}` MCP tool responses.")]
public class ListDependenciesPaginatedResult : PaginatedResult {

    public ListDependenciesPaginatedResult(
            IEnumerable<DependencyInfo> dependenciesPaginated,
            PaginatedResult pr) : base(pr) {
        Dependencies = dependenciesPaginated;
    }

    [Description("Sequence of paginated dependencies")]
    public IEnumerable<DependencyInfo> Dependencies { get; set; } = [];
}
