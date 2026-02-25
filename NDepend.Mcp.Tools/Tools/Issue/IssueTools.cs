using System.Diagnostics;
using NDepend.CodeModel;
using NDepend.CodeQuery;
using NDepend.Helpers;
using NDepend.Issue;
using NDepend.TechnicalDebt;
using NDepend.Mcp.Tools.Common;
using NDepend.Mcp.Services;
using NDepend.Path;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Tools.Rule;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.Issue;


[McpServerToolType]
public static class IssueTools {

    
    internal const string TOOL_LIST_ISSUES_NAME = Constants.TOOL_NAME_PREFIX + "list-issues";

    internal const string TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME = Constants.TOOL_NAME_PREFIX + "get-issue-details-to-fix-it";


    const string MAX_PAGE_SIZE = "50";
    [McpServerTool(Name = TOOL_LIST_ISSUES_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                   {Constants.PROMPT_CALL_INITIALIZE}
                   
                   # List Issues
                   
                   Returns issues scoped by user's request with details for navigation and resolution.
                   
                   ## Use-Cases
                   
                   - Quality: "List issues in file/class", "Show critical issues"
                   - Security: "Find vulnerabilities"
                   - Debt: "Show technical debt by priority", "List code smells", "Find deprecated APIs"
                   - Compliance: "Show coding standard violations", "Get architecture rule violations"
                   - Reporting: "Generate issue report by severity", "Top 10 violations", "Get issue metrics"
                   
                   ## Response Formatting
                   
                   ### 1. Clickable Locations (MANDATORY)
                   
                   Every issue MUST include in order:
                   1. Clickable location: `(file.ext:line)`
                   2. Rule name
                   3. Explanation
                   4. Technical debt
                   5. Severity
                   6. Provider (only if multiple providers present)
                   
                   **Example:**
                   ```
                   1. `(UserService.cs:42)` - **Method names should use PascalCase**
                      - Private method `calculate_total` violates naming conventions
                      - Technical Debt: 5 minutes
                      - Severity: Low
                   ```
                   
                   ### 2. Number All Issues (1-based) (MANDATORY)
                   
                   Format: `1. `, `2. `, `3. `
                   Enables: `Fix #2`, `Explain #5`
                          
                   **IMPORTANT:** When user requests fix, call the tool `{TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME}`
                   """)]

    public static async Task<ListIssuesPaginatedResult> ListIssuesTool(
                INDependService service,
                ILogger<IssueToolsLog> logger,

                [Description(PaginatedResult.PAGINATION_CURSOR_DESC)]
                int cursor,

                [Description($"Max number of issues per page (≤ {MAX_PAGE_SIZE}) to avoid LLM prompt overflow.")]
                int pageSize,

                [Description("Filter by file name (case-insensitive substring). Null=all")]
                string? filterFileName,

                [Description("Filter by namespace (case-insensitive substring). Null=all")]
                string? filterNamespace,

                [Description(
                    $"""
                     Filter by provider `{RuleProviderHelpers.RULE_PROVIDER_NDEPEND}`, `{RuleProviderHelpers.RULE_PROVIDER_ROSLYN_ANALYZERS}` or `{RuleProviderHelpers.RULE_PROVIDER_RESHARPER}`.
                     Null=all
                     """)]
                string? filterRuleProvider,

                [Description(
                    $"""
                     Filter by severity: `{SeverityHelpers.SEVERITY_ALL}` or combination of `{SeverityHelpers.SEVERITY_BLOCKER}`, `{SeverityHelpers.SEVERITY_CRITICAL}`, `{SeverityHelpers.SEVERITY_HIGH}`, `{SeverityHelpers.SEVERITY_MEDIUM}` and `{SeverityHelpers.SEVERITY_LOW}`.
                     """)]
                string[] filterSeverity,

                [Description(
                    "Filter by rule categories (case-insensitive substring). Empty=all. " +
                   $"Call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` with  `{RuleTools.TOOL_ARG_FILTER_AT_LEAST_NB_ISSUES}`=1 to see available categories")]
                string[] filterRuleCategories,

                [Description(
                    "Filter by rule IDs (case-insensitive substring). Empty=all. " +
                   $"Call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` with  `{RuleTools.TOOL_ARG_FILTER_AT_LEAST_NB_ISSUES}`=1 to see available IDs")]
                string[] filterRulesId,

                [Description(
                    $"""
                     Filter by change status: `{IssueChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_NEW}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_UNRESOLVED}` or `{IssueChangeStatusSinceBaselineHelpers.STATUS_FIXED}`.
                     Null=`{IssueChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`
                     """)]
                string? filterIssueChangeStatus,

                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_LIST_ISSUES_NAME} with arguments: 
                -cursor= `{cursor}`
                -pageSize= `{pageSize}`
                -filterFileName= `{filterFileName ?? "<any>"}`
                -filterNamespace= `{filterNamespace ?? "<any>"}`
                -filterRuleProvider= `{filterRuleProvider ?? "<any>"}`
                -filterRuleCategory= `{filterRuleCategories.Aggregate("', '")}`
                -filterRulesId= `{filterRulesId.Aggregate("', '")}`
                -filterIssueChangeStatus= `{filterIssueChangeStatus?? "<default>"}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            // filterIssueChangeStatus
            var issues = new List<IIssue>();
            var issuesSetDiff = session.IssuesSetDiff;

            var filterIssueChangeStatusVal = IssueChangeStatusSinceBaseline.Default;
            if (filterIssueChangeStatus.IsValid()) {
                filterIssueChangeStatusVal = IssueChangeStatusSinceBaselineHelpers.GetIssueChangeStatusVal(filterIssueChangeStatus);
            }

            if (filterIssueChangeStatusVal.HasFlag(IssueChangeStatusSinceBaseline.Unresolved)) {
                issues.AddRange(issuesSetDiff.NewerIssuesSet.AllIssues.Except(issuesSetDiff.AllIssuesAdded));
            }
            if (filterIssueChangeStatusVal.HasFlag(IssueChangeStatusSinceBaseline.New)) {
                issues.AddRange(issuesSetDiff.AllIssuesAdded);
            }
            if (filterIssueChangeStatusVal.HasFlag(IssueChangeStatusSinceBaseline.Fixed)) {
                issues.AddRange(issuesSetDiff.AllIssuesFixed);
            }
        
            // filterFileName
            if (filterFileName.IsValid()) {
                issues.RemoveAll(issue =>
                    !issue.SourceFileDeclAvailable ||
                    !issue.SourceDecl.SourceFile.FileName.Contains(filterFileName, StringComparison.OrdinalIgnoreCase)
                );
            }

            // filterNamespace
            if (filterNamespace.IsValid()) {
                issues.RemoveAll(issue => {
                    var c = issue.CodeElement;
                    INamespace? parentNamespace = c.AsNamespace;
                    if (parentNamespace == null) {
                        var member = c.AsMember;
                        var type = c.AsType;
                        parentNamespace = member?.ParentNamespace ??
                                          type?.ParentNamespace ??
                                          null;
                    }
                    if (parentNamespace == null) { return true; }
                    return !parentNamespace.FullName.Contains(filterNamespace, StringComparison.OrdinalIgnoreCase);
                });
            }

            // filterRuleProvider
            if (filterRuleProvider.IsValid()) {
                RuleProvider ruleProvider = RuleProviderHelpers.GetRuleProviderVal(logger, filterRuleProvider);
                issues.RemoveAll(i => i.Rule.Provider != ruleProvider);
            }

            // filterSeverity
            if (filterSeverity.Length > 0) {
                if (filterSeverity.Length > 1 || filterSeverity[0] != SeverityHelpers.SEVERITY_ALL) {
                    Severity[] severityValues = filterSeverity
                        .Where(s => s.IsValid())
                        .Select(s => SeverityHelpers.GetSeverity(logger, s))
                        .ToArray();
                    issues.RemoveAll(i => !severityValues.Contains(i.Severity));
                }
            }

            // filterRuleCategories
            if (filterRuleCategories.Length > 0) {
                issues.RemoveAll(i => !i.Rule.Category.ContainsAny(filterRuleCategories, StringComparison.OrdinalIgnoreCase));
            }

            // filterRulesId
            if (filterRulesId.Length > 0) {
                issues.RemoveAll(i => !i.Rule.Id.ContainsAny(filterRulesId, StringComparison.OrdinalIgnoreCase));
            }

            var project = session.Project;
            var debtFormatter = project.DebtSettings.Values.CreateDebtFormatter();
            var issuesInfo = issues.Select(i => {
                var issueInfo = new IssueInfo();
                FillIssueInfo(i, issueInfo, debtFormatter, filterIssueChangeStatusVal);
                return issueInfo;
            }).ToList();

            var paginatedResult = PaginatedResult.Build(logger, issuesInfo, cursor, pageSize, MAX_PAGE_SIZE, out var paginatedIssuesInfo);
            return new ListIssuesPaginatedResult(paginatedIssuesInfo, paginatedResult);
        }, cancellationToken);
    }

    [McpServerTool(Name = TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Get Issue Details to Fix It
                  
                  Retrieves diagnostic info for a specific NDepend issue to understand and resolve it.
                  **MANDATORY:** You MUST call this tool anytime the user wants to fix an issue.
                  
                  ## REQUIRED 
                  
                  Parameters obtained from IssueInfo returned by calling the tool `{TOOL_LIST_ISSUES_NAME}`.
                  - ruleId
                  - sourceFilePath  
                  - sourceFileLine
                  
                  ## Use-Cases
                  
                  - "How do I fix issue #X?" or "Fix issue #X"
                  - User wants detailed issue info or fix strategies
                  - Preparing code fix/refactoring
                  
                  ## Fix Guidance
                  
                  - Recompile changes to check syntax
                  - Show proposed fix in preview with before/after diff
                  - User reviews and clicks Keep/Undo
                  - Provide test steps and regression risks
                  """)]
    public static async Task<IssueDetailsForFixingItInfo?> GetIssueDetailsToFixItTool(
                INDependService service,
                ILogger<IssueToolsLog> logger,

                [Description("Rule ID")]
                string ruleId,
                [Description("Source file path")]
                string sourceFilePath,
                [Description("Line number (1-based)")]
                uint sourceFileLine,

                CancellationToken cancellationToken
        ) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME} with arguments: 
                -ruleId=`{ruleId}`
                -sourceFilePath=`{sourceFilePath}`
                -sourceFileLine=`{sourceFileLine}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            // MUST be newer set, cannot fix an issue in baseline, it is either fixed already or unresolved
            var issuesSet = session.IssuesSetDiff.NewerIssuesSet;
            if (!TryFindIssue(issuesSet, ruleId, sourceFilePath, sourceFileLine, out IIssue? issueFound)) {
                logger.LogErrorAndThrow("No issue found with the information provided");
                return null;
            }

            // FillIssueInfo
            Debug.Assert(issueFound != null);
            var project = session.Project;
            var debtFormatter = project.DebtSettings.Values.CreateDebtFormatter();
            var issueDetails = new IssueDetailsForFixingItInfo();
            var issueChangeStatus = IssueChangeStatusSinceBaseline.Unresolved;
            var issuesSetDiff = session.IssuesSetDiff;
            if (issuesSetDiff.AllIssuesAdded.Contains(issueFound)) {
                issueChangeStatus = IssueChangeStatusSinceBaseline.New;
            }
        

            FillIssueInfo(issueFound, issueDetails, debtFormatter, issueChangeStatus);

            // Fill properties specifics to IssueDetailsForFixingItInfo
            if (issueFound.Rule.TryGetDescription(StringFormattingKind.Readable, out string ruleDescription)) {
                issueDetails.RuleDescription = ruleDescription;
            }
            if (issueFound.Rule.TryGetHowToFix(StringFormattingKind.Readable, out string ruleHowToFix)) {
                issueDetails.RuleHowToFix = ruleHowToFix;
            }
            if (issueFound.Record is RecordBase record) {
                var columnNames = issueFound.ColumnsNames;
                Debug.Assert(columnNames.Count == record.RecordArity);

                var extraInfo = new Dictionary<string, string>();
                for(int i=1; i < record.RecordArity; i++) { // First column is always the code element we already have in IssueInfo.CodeElement
                    RecordCellValue cellValue = record[i];
                    if(cellValue.m_RecordCellType == RecordCellType.Debt) { continue; } // Already available in IssueInfo.Debt
                    string itemDesc = cellValue.GetRecordCellValueDescription(debtFormatter);
                    extraInfo.Add(columnNames[i], itemDesc);
                }
                issueDetails.ExtraInfo = extraInfo;
            }

            return issueDetails;
        }, cancellationToken);
    }




    //
    // Helpers
    //


    // IssueDetailsForFixingItInfo derives from IssueInfo so we can reuse this method in both cases
    private static void FillIssueInfo(IIssue issue, IssueInfo issueInfo, IDebtFormatter debtFormatter, IssueChangeStatusSinceBaseline issueChangeStatus) {

        if (!issue.TryGetExplanation(StringFormattingKind.Readable, debtFormatter, out string explanation)) {
            explanation = Constants.NOT_AVAILABLE;
        }

        issueInfo.RuleProvider = issue.Rule.Provider.GetString();
        issueInfo.RuleId = issue.Rule.Id;
        issueInfo.RuleName = issue.Rule.Name;
        issueInfo.Explanation = explanation;
        issueInfo.Severity = issue.Severity.GetString();
        issueInfo.CodeElement = issue.CodeElement.FullyQualifiedName();
        issueInfo.SourceFilePath = issue.SourceDecl?.SourceFile?.FilePathString ?? string.Empty;
        issueInfo.SourceFileLine = issue.SourceDecl?.Line ?? 0;
        issueInfo.IssueChangeStatus = issueChangeStatus.GetString();
        issueInfo.Debt = debtFormatter.ToString(issue.Debt.Value);
    }

    private static bool TryFindIssue(
                IIssuesSet issuesSet, 
                string ruleId, 
                string sourceFilePath, 
                uint sourceFileLine,
                out IIssue? issueFound) {
        ruleId = ruleId.Trim();

        // Don't assert that the path is absolute, extract file name from sourceFilePath, 
        string fileName;
        if(sourceFilePath.TryGetAbsoluteFilePath(out IAbsoluteFilePath sourceFilePathTyped)) {
            fileName = sourceFilePathTyped.FileName;
        } else {
            int sepIndex = sourceFilePath.LastIndexOfAny(['\\', '/']);
            fileName = sepIndex > 0 ? sourceFilePath[(sepIndex + 1)..] : sourceFilePath;
        }


        // Match issues with same ruleId and similar source file name
        var issues = issuesSet.AllIssues.Where(i =>
            string.Compare(i.Rule.Id, ruleId, StringComparison.CurrentCultureIgnoreCase) == 0 &&
            i.SourceFileDeclAvailable &&
            i.SourceDecl.SourceFile.FileName.Contains(fileName, StringComparison.CurrentCultureIgnoreCase)).ToArray();

        issueFound = null;
        switch (issues.Length) {
            case 0: return false;
            case 1: issueFound = issues[0]; return true;
        }

        // Multiple issues found, heuristic to disambiguate using sourceFileLine and file path
        int bestScore = int.MinValue;
        foreach (var i in issues) {

            var iSourceDecl = i.SourceDecl;

            int iScore = 0;
            if (sourceFilePathTyped != null && iSourceDecl.SourceFile.FilePath.Equals(sourceFilePathTyped)) {
                iScore += 11;
            } else if(string.Compare(iSourceDecl.SourceFile.FileName, fileName, StringComparison.CurrentCultureIgnoreCase) == 0) {
                iScore += 7;
            }

            // If same line add 19 to the iScore
            uint iLine = iSourceDecl.Line;
            long delta = 19 - Math.Abs((long)iLine - (long)sourceFileLine);
            if(delta < 0) { delta = 0; }
            iScore += (int)delta;

            if(iScore > bestScore) {
                issueFound = i;
                bestScore = iScore;
            }
        }

        // Since issues contains multiple issues, issueFound cannot be null here
        Debug.Assert(issueFound != null);
        return true;
    }
}
