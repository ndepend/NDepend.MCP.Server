using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Issue {

    [Description("This class let's paginate the server's response to a list issues request from the client.")]
    public sealed class ListIssuesPaginatedResult : PaginatedResult {

        public ListIssuesPaginatedResult(
             IEnumerable<IssueInfo> issuesPaginated,
             PaginatedResult pr) : base(pr)  {
            Issues = issuesPaginated;
        }

        [Description("Gets or sets the sequence of issues paginated.")]
        public IEnumerable<IssueInfo> Issues { get; set; } = [];
    }
}
