using NDepend.Helpers;
using NDepend.Issue;
using NDepend.Mcp.Tools.Common;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Issue;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Rule;


[McpServerToolType]
public static class RuleTools {

    const int MAX_PAGE_SIZE = 10;

    internal const string TOOL_LIST_RULES_DETAILED_NAME = Constants.TOOL_NAME_PREFIX + "list-rules-detailed";
    internal const string TOOL_LIST_ALL_RULES_SUMMARY_NAME = Constants.TOOL_NAME_PREFIX + "list-all-rules-summary";

    internal const string TOOL_ARG_FILTER_AT_LEAST_NB_ISSUES = "filterAtLeastNbIssues";
    

    [McpServerTool(Name = TOOL_LIST_ALL_RULES_SUMMARY_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                 
                 # LIST CODE RULES SUMMARY
                 
                 Returns a hierarchical summary of all NDepend Rules, Roslyn Analyzers and Resharper Code Inspections in the current project.
                 
                 Structure:
                 - Rules are grouped by parent categories (hierarchical/recursive structure)
                 - Each rule includes: name, identifier, criticality, and issue count
                 
                 Note: This tool returns all results without pagination. The complete rule set fits within the LLM context window.
                 
                 **IMPORTANT**:
                 
                 This tool does not return the issues raised by a rule.
                 To retrieve rule issues, use the tool `{IssueTools.TOOL_LIST_ISSUES_NAME}`.
                 
                 # USE-CASES
                 
                 Call this tool first to discover available rules in the current project analysis before calling `{TOOL_LIST_RULES_DETAILED_NAME}` or `{IssueTools.TOOL_LIST_ISSUES_NAME}`.
                 Use the results to identify valid values for the `filterRuleCategories` and `filterRuleIds` filter parameters in subsequent tool calls.
                 
                 This tool is also appropriate for any rule-related request where the rule description or fix guidance is not needed.
                 
                 **Discover Available Code Quality Rules:**
                 - "List all NDepend rules in this project."
                 - "What code quality rules are available?"
                 - "Show me the first 50 rules defined in the analysis."
                 - "Which rule has at least 10 issues?"
                 
                 **Filter Rules by Category or Metric Type:**
                 - "Show rules related to cyclomatic complexity."
                 - "List only architecture-related rules."
                 - "Give me all dependency-related rules."
                 
                 **Audit and Governance:**
                 - "Which architectural rules are enforced in this project?"
                 - "List all critical rules currently enabled."
                 - "Review rule coverage for governance purposes."
                 """)]
    public static async Task<RuleCategoryInfo[]> ListAllRulesSummaryTool(
                INDependService service,
                ILogger<RuleToolsLog> logger,

                [Description(
                    """
                    Filters only rules that have at least the specified number of issues.
                    Set to 0 to include all rules.
                    Set to 1 to include all rules violated (e.g. with issues).
                    """)]
                int filterAtLeastNbIssues,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_LIST_ALL_RULES_SUMMARY_NAME} with parameters: 
                -filterAtLeastNbIssues=`{filterAtLeastNbIssues}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {

            var issuesSet = session.IssuesSetDiff.NewerIssuesSet;

            var allRules = issuesSet.AllRules.Select(r => 
                new RuleSummaryInfoTmp(r.Category.Split([" \\ "], StringSplitOptions.RemoveEmptyEntries)) {
                    Id = r.Id,
                    Name = r.Name,
                    IsCritical = r.IsCritical,
                    NbIssues = session.IssuesSet.Issues(r).Count
                }).ToList();

            // filterAtLeastNbIssues
            if (filterAtLeastNbIssues > 0) {
                allRules.RemoveAll(r => r.NbIssues < filterAtLeastNbIssues);
            }
            
            return BuildCategoryHierarchy(allRules);
        }, cancellationToken);
    }

    // used only by BuildCategoryHierarchy() to ease building hierarchy
    private sealed record RuleCategoryInfoTmp(string Name) : RuleCategoryInfo(Name) {
        internal List<RuleSummaryInfo> RuleSummaryTmp { get; set; } = new List<RuleSummaryInfo>();
        internal List<RuleCategoryInfoTmp> ChildCategoriesTmp { get; set; } = new List<RuleCategoryInfoTmp>();
        internal void Seal() {
            this.RuleSummary = RuleSummaryTmp.ToArray();
            this.ChildCategories = ChildCategoriesTmp.ToArray();
            foreach(var c in ChildCategoriesTmp) {
                c.Seal();
            }
        }
    }
    private sealed record RuleSummaryInfoTmp(string[] ParentCategories) : RuleSummaryInfo {
        internal string[] ParentCategories { get; } = ParentCategories;
    }

    private static RuleCategoryInfo[] BuildCategoryHierarchy(IEnumerable<RuleSummaryInfoTmp> rules) {
        var roots = new Dictionary<string, RuleCategoryInfoTmp>();

        foreach (var rule in rules) {
            var parts = rule.ParentCategories;
            if (parts.Length == 0) { continue; }

            // Get or create the root category
            string rootCategoryName = parts[0];
            if (!roots.TryGetValue(rootCategoryName, out var rootCategory)) {
                rootCategory = new RuleCategoryInfoTmp(rootCategoryName);
                roots[rootCategoryName] = rootCategory;
                // Rules with different providers cannot be under the same root category
                rootCategory.Provider = rootCategoryName switch {
                    "Roslyn Analyzers" => RuleProviderHelpers.RULE_PROVIDER_ROSLYN_ANALYZERS,
                    "R# Code Inspections" => RuleProviderHelpers.RULE_PROVIDER_RESHARPER,
                    _ => RuleProviderHelpers.RULE_PROVIDER_NDEPEND
                };
            }

            // Navigate/create the category hierarchy
            RuleCategoryInfoTmp currentCategory = rootCategory;
            for (int i = 1; i < parts.Length; i++) {
                var partName = parts[i];
                var childCategory = currentCategory.ChildCategoriesTmp.FirstOrDefault(c => c.Name == partName);
                if (childCategory == null) {
                    childCategory = new RuleCategoryInfoTmp(partName);
                    currentCategory.ChildCategoriesTmp.Add(childCategory);
                    childCategory.Provider = currentCategory.Provider;
                }
                currentCategory = childCategory;
            }

            // Add the rule to the leaf category
            currentCategory.RuleSummaryTmp.Add(rule);
        }

        foreach(var root in roots.Values) {
            root.Seal();
        }
        return roots.Values.Cast<RuleCategoryInfo>().ToArray();
    }





    [McpServerTool(Name = TOOL_LIST_RULES_DETAILED_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                 
                 # LIST CODE RULES WITH DESCRIPTION AND FIX GUIDANCE COLLECTION
                 
                 Return a paginated list of NDepend rules available on the current project with their descriptions and fix guidances (how-to-fix).
                 Use the filter parameters to avoid having too many issues returned.
                 
                 If the user wants to filter rules by a specific category or type of rule, first call `{TOOL_LIST_ALL_RULES_SUMMARY_NAME}` to retrieve the available categories and rule IDs that have issues.
                 With this information, you can accurately set the `filterRuleCategories` and `rulesId` filters.
                 
                 **IMPORTANT**:
                 
                 This tool does not return the issues raised by a rule.
                 To retrieve rule issues, use the tool `{IssueTools.TOOL_LIST_ISSUES_NAME}`.  
                 
                 # PURPOSE AND USE-CASES
                 
                 **Primary use cases:**
                 - Understanding what a specific rule checks for
                 - Learning how to fix violations of a rule
                 - Exploring rule documentation before investigating issues
                 - Getting comprehensive rule details after identifying rules of interest
                 
                 **When to use this tool:**
                 - User asks "what does rule X check?" or "how do I fix rule Y?"
                 - User wants to understand available rules in a specific category
                 - User needs fix guidance for one or more rules
                 - After calling `{TOOL_LIST_ALL_RULES_SUMMARY_NAME}` to get detailed information about identified rules
                 
                 **When NOT to use this tool:**
                 - User wants to see actual issues/violations (use `{IssueTools.TOOL_LIST_ISSUES_NAME}` instead)
                 - User only needs rule names and counts (use `{TOOL_LIST_ALL_RULES_SUMMARY_NAME}` instead)
                 - User wants to analyze or fix specific code issues (use `{IssueTools.TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME}` instead)
                 """)]
    public static async Task<ListRulesPaginatedResult> ListRulesDetailedTool(
                INDependService service,
                ILogger<RuleToolsLog> logger,

                [Description(
                    "An opaque token representing the pagination position after the last returned result. Set to null to start from the beginning.")]
                string? cursor,

                [Description(
                    "Maximum number of rules to include per page. Must not exceed 10 to prevent LLM prompt overflow.")]
                int pageSize,

                [Description(
                    $"""
                     Filters by rule provider that can be either `{RuleProviderHelpers.RULE_PROVIDER_NDEPEND}`, `{RuleProviderHelpers.RULE_PROVIDER_ROSLYN_ANALYZERS}` or `{RuleProviderHelpers.RULE_PROVIDER_RESHARPER}`.
                     Set to null to include all providers.
                     """)]
                string? filterRuleProvider,

                [Description(
                    $"""
                     Filters rules by rule categories. 
                     Matches are case-insensitive and use substring search.
                     It is enough that a rule category matches one of the provided categories to be included.
                     Leave empty to include all categories.
                     To see available categories, first call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}`.
                     """)]
                string[] filterRuleCategories,

                [Description(
                    $"""
                     Filters issues by parent rule ID. Leave empty to include all rules.
                     Matches are case-insensitive and use substring search.
                     Leave empty to include all rule IDs.
                     To find available IDs of rules, first call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}`.
                     """)]
                string[] filterRulesId,

                [Description(
                    "Filters only critical rules. Set to false to include all rules.")]
                bool filterCriticalOnly,

                [Description(
                    """
                    Filters only rules that have at least the specified number of issues.
                    Set to 0 to include all rules.
                    Set to 1 to include all rules violated (e.g. with issues).
                    """)]
                int filterAtLeastNbIssues,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
               {LogHelpers.TOOL_LOG_SEPARATOR}
               Invoking {TOOL_LIST_RULES_DETAILED_NAME} with arguments: 
                 -cursor=`{cursor ?? "0"}`
                 -pageSize=`{pageSize}`
                 -filterRuleProvider=`{filterRuleProvider ?? "<any>"}
                 -filterRuleCategory=`{filterRuleCategories.Aggregate("', '")}`
                 -filterRulesId=`{filterRulesId.Aggregate("', '")}`
                 -filterCriticalOnly=`{filterCriticalOnly}`
                 -filterAtLeastNbIssues=`{filterAtLeastNbIssues}`
               """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            if (pageSize > MAX_PAGE_SIZE) {
                pageSize = MAX_PAGE_SIZE;
                logger.LogInformation($"pageSize parameter exceeded maximum of {MAX_PAGE_SIZE}. It has been set to {MAX_PAGE_SIZE}.");
            }
            var issuesSet = session.IssuesSetDiff.NewerIssuesSet;

            var allRules = issuesSet.AllRules.ToList();

            // filterRuleProvider
            if (filterRuleProvider.IsValid()) {
                RuleProvider ruleProvider = RuleProviderHelpers.GetRuleProviderVal(logger, filterRuleProvider);
                allRules.RemoveAll(r => r.Provider != ruleProvider);
            }

            // filterRuleCategories
            if (filterRuleCategories.Length > 0) {
                allRules.RemoveAll(r => !r.Category.ContainsAny(filterRuleCategories, StringComparison.OrdinalIgnoreCase));
            }

            // filterRulesId
            if (filterRulesId.Length > 0) {
                allRules.RemoveAll(r => !r.Id.ContainsAny(filterRulesId, StringComparison.OrdinalIgnoreCase));
            }

            // filterCriticalOnly
            if (filterCriticalOnly) {
                allRules.RemoveAll(r => !r.IsCritical);
            }

            // filterAtLeastNbIssues
            if (filterAtLeastNbIssues > 0) {
                allRules.RemoveAll(r => session.IssuesSet.Issues(r).Count < filterAtLeastNbIssues);
            }

            var rules = allRules.Select(
                r => {
                    if (!r.TryGetDescription(StringFormattingKind.Readable, out string ruleDescription)) {
                        ruleDescription = Constants.NOT_AVAILABLE;
                    }
                    if (!r.TryGetHowToFix(StringFormattingKind.Readable, out string ruleHowToFix)) {
                        ruleHowToFix = Constants.NOT_AVAILABLE;
                    }
                    int nbIssues = session.IssuesSet.Issues(r).Count;
                    return new RuleDetailInfo() {
                        Id = r.Id,
                        Name = r.Name,
                        Provider = r.Provider.GetString(),
                        Description = ruleDescription,
                        RuleHowToFix = ruleHowToFix,
                        Category = r.Category,
                        IsCritical = r.IsCritical,
                        NbIssues = nbIssues
                    };
                }).ToList();

            var paginatedResult = PaginatedResult.Build(logger, rules, cursor, pageSize, out var paginatedRulesInfo);
            return new ListRulesPaginatedResult(paginatedRulesInfo, paginatedResult);

        }, cancellationToken);

        
    }
}
