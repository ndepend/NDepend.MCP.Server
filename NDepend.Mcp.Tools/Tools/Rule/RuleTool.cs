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

    internal const string TOOL_LIST_RULES_DETAILED_NAME = Constants.TOOL_NAME_PREFIX + "list-rules-detailed";
    internal const string TOOL_LIST_ALL_RULES_SUMMARY_NAME = Constants.TOOL_NAME_PREFIX + "list-all-rules-summary";

    internal const string TOOL_ARG_FILTER_AT_LEAST_NB_ISSUES = "filterAtLeastNbIssues";
    
    [McpServerTool(Name = TOOL_LIST_ALL_RULES_SUMMARY_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                 
                 # Code Rules Summary
                 
                 Returns hierarchical summary of all NDepend Rules, Roslyn Analyzers, and Resharper Code Inspections.
                 
                 ## Returns
                 
                 Rules grouped by parent categories with name, ID, criticality, and issue count.
                 
                 **Note**: 
                 - Returns complete rule set (no pagination). 
                 - Does NOT return issues, use the `{IssueTools.TOOL_LIST_ISSUES_NAME}` tool for that.
                 
                 ## Use-Cases
                 
                 **IMPORTANT**: Call this first to discover valid `filterRuleCategories` and `filterRuleIds` values for other tools.
                 
                 - Discovery: "List all rules", "What rules are available?", "Which rule has 10+ issues?"
                 - Categories: "Show complexity rules", "List architecture/dependency rules"
                 - Audit: "Which rules are enforced?", "List critical rules", "Review rule coverage"
                 """)]
    public static async Task<RuleCategoryInfo[]> ListAllRulesSummaryTool(
                INDependService service,
                ILogger<RuleToolsLog> logger,

                [Description("Minimum issue per rule count threshold. 0=all rules, 1=violated rules only")]
                int filterAtLeastNbIssues,

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




    const string MAX_PAGE_SIZE = "10";
    [McpServerTool(Name = TOOL_LIST_RULES_DETAILED_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                 
                 # List Some Code Rules with Description and Fix Guidance
                 
                 Returns paginated list of NDepend rules with descriptions and how-to-fix guidance.
                 
                 **Note**: Call `{TOOL_LIST_ALL_RULES_SUMMARY_NAME}` first to find valid `filterRuleCategories` and `rulesId` values.
                 
                 ## Use-Cases
                 
                 - "What does rule X check?", "How do I fix rule Y?"
                 - User needs fix guidance or rule documentation
                 - After calling `list_rules_summary` to get details on identified rules
                 """)]
    public static async Task<ListRulesPaginatedResult> ListRulesDetailedTool(
                INDependService service,
                ILogger<RuleToolsLog> logger,

                [Description(PaginatedResult.PAGINATION_CURSOR_DESC)]
                int cursor,

                [Description($"Max number of rules per page (≤ {MAX_PAGE_SIZE}) to avoid LLM prompt overflow.")]
                int pageSize,

                [Description(
                    $"""
                     Filter by rule provider: `{RuleProviderHelpers.RULE_PROVIDER_NDEPEND}`, `{RuleProviderHelpers.RULE_PROVIDER_ROSLYN_ANALYZERS}` or `{RuleProviderHelpers.RULE_PROVIDER_RESHARPER}`.
                     Null=all providers
                     """)]
                string? filterRuleProvider,

                [Description(
                    $"""
                     Filter by rule categories (case-insensitive substring)
                     Empty=all. 
                     Call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` first to see available categories
                     """)]
                string[] filterRuleCategories,

                [Description(
                    $"""
                     Filter by rule IDs (case-insensitive substring)
                     Empty=all
                     Call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` first to find possible IDs
                     """)]
                string[] filterRulesId,

                [Description(
                    "True=critical rules only, False=all")]
                bool filterCriticalOnly,

                [Description(
                    """
                    Minimum issue count. 0=all rules, 1=violated rules only
                    """)]
                int filterAtLeastNbIssues,

                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
               {LogHelpers.TOOL_LOG_SEPARATOR}
               Invoking {TOOL_LIST_RULES_DETAILED_NAME} with arguments: 
                 -cursor= `{cursor}`
                 -pageSize= `{pageSize}`
                 -filterRuleProvider= `{filterRuleProvider ?? "<any>"}
                 -filterRuleCategory= `{filterRuleCategories.Aggregate("', '")}`
                 -filterRulesId= `{filterRulesId.Aggregate("', '")}`
                 -filterCriticalOnly= `{filterCriticalOnly}`
                 -filterAtLeastNbIssues= `{filterAtLeastNbIssues}`
               """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
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

            var paginatedResult = PaginatedResult.Build(logger, rules, cursor, pageSize, MAX_PAGE_SIZE, out var paginatedRulesInfo);
            return new ListRulesPaginatedResult(paginatedRulesInfo, paginatedResult);

        }, cancellationToken);

        
    }
}
