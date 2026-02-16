using NDepend.Mcp.Tools.Common;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Search {
    [Description($"This class enables pagination of the server responses returned by calls to the MCP tool `{SearchTools.TOOL_SEARCH_CODE_ELEMENTS_NAME}`.")]
    public sealed class SearchPaginatedResult : PaginatedResult {

        public SearchPaginatedResult(
             IEnumerable<CodeElementInfo> codeElementsPaginated,
             PaginatedResult pr) : base(pr) {
            CodeElementInfos = codeElementsPaginated;
        }

        [Description("Gets or sets the sequence of code elements matched paginated.")]
        public IEnumerable<CodeElementInfo> CodeElementInfos { get; set; } = [];
    }
}
