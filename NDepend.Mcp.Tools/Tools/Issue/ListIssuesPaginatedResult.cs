using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Issue {

    [Description($"Enables pagination for `{IssueTools.TOOL_LIST_ISSUES_NAME}` MCP tool responses.")]
    public sealed class ListIssuesPaginatedResult : PaginatedResult {

        public ListIssuesPaginatedResult(
             IEnumerable<IssueInfo> issuesPaginated,
             PaginatedResult pr) : base(pr)  {
            Issues = issuesPaginated;
        }

        [Description("Sequence of paginated issues")]
        public IEnumerable<IssueInfo> Issues { get; set; } = [];
    }
}
