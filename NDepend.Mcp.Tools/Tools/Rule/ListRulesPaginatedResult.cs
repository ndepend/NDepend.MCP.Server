using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Rule {

    [Description($"Enables pagination for `{RuleTools.TOOL_LIST_RULES_DETAILED_NAME}` MCP tool responses.")]
    public sealed class ListRulesPaginatedResult : PaginatedResult {

        public ListRulesPaginatedResult(
             IEnumerable<RuleDetailInfo> rulesPaginated,
             PaginatedResult pr) : base(pr) {
            Rules = rulesPaginated;
        }

        [Description("Sequence of paginated rules")]
        public IEnumerable<RuleDetailInfo> Rules { get; set; } = [];
    }
}
