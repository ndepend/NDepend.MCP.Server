using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Rule {

    [Description("This class let's paginate the server's response to a list rules request from the client.")]
    public sealed class ListRulesPaginatedResult : PaginatedResult {

        public ListRulesPaginatedResult(
             IEnumerable<RuleDetailInfo> rulesPaginated,
             PaginatedResult pr) : base(pr) {
            Rules = rulesPaginated;
        }

        [Description("Gets or sets the sequence of rules paginated.")]
        public IEnumerable<RuleDetailInfo> Rules { get; set; } = [];
    }
}
