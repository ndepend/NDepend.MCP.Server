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


    const int MAX_PAGE_SIZE = 100;

    [McpServerTool(Name = TOOL_LIST_ISSUES_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                   {Constants.PROMPT_CALL_INITIALIZE}
                   
                   # LIST ISSUES
                   
                   Returns a list of issues scoped by the user's request, providing comprehensive details for each issue to enable quick navigation, understanding, and resolution.
                   
                   # USE-CASES
                   
                   **Code Quality & Review:**
                   - "List issues in this source file"
                   - "Which serious issues in this class"
                   - "Show me all critical code quality issues in the project"
                   - "List issues with a high or above severity"
                   - "Show issues introduced since the baseline"
                   - "Find all security vulnerabilities in the authentication module"
                   
                   **Technical Debt Management:**
                   - "Show me all technical debt items sorted by priority"
                   - "List issues related to obsolete or deprecated APIs or outdated dependencies"
                   - "Get issues related to code smell"
                   
                   **Compliance & Standards:**
                   - "List all coding standard violations in the codebase"
                   - "Show me architectural rule violations"
                   - "Get all issues where naming conventions are not followed"
                   
                   **Reporting & Analytics:**
                   - "Generate a report of all issues by severity and type"
                   - "List the top 10 most common code violations"
                   - "Get issue metrics for our weekly team standup"
                   
                   # FILTER
                   
                   - Initializes filter parameters based on the user's request to limit and scope the issues returned. 
                   - When requesting NDepend issues, use `{RuleProviderHelpers.RULE_PROVIDER_NDEPEND}` as the provider filter.
                   
                   ## FILTER ISSUES BY CATEGORY OR RULE KIND
                   
                   If the user wants to filter issues by a specific category or type of rule, first call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` to retrieve the available categories and rule IDs that have issues.
                   With this information, you can accurately set the `filterRuleCategories` and `rulesId` filters.
                   
                   Such user requests can look like:
                   - "List issues related to code complexity"
                   - "Show me all naming convention violations"
                   - "What design issues exist in this project?"
                   - "Find architecture problems"
                   - "Show security-related issues"
                   - "What API breaking changes are there?"
                   - "List all code smells"
                   - "Find coupling issues"
                   
                   # RESPONSE STRUCTURE
                   
                   **CRITICAL: You MUST format ALL issue responses according to these requirements. Non-compliance reduces tool utility.**
                   
                   ## 1. ENABLE SINGLE-CLICK NAVIGATION (MANDATORY)
                   
                   **Every single issue MUST include ALL of the following information in this exact order:**
                   
                   1. **Clickable source location** - Format: `(file.ext:line)` - Example: `(UserService.cs:42)`
                   2. **Rule Name** - The full, official rule name
                   3. **Explanation** - Clear description of what the issue is
                   4. **Amount of technical debt** - Time/effort estimate from the tool
                   5. **Severity** - Criticality level (Blocker, Critical, High, Medium, Low)
                   6. **Rule Provider** - ONLY include if issues from multiple providers are present in the response
                   
                   **WRONG - Missing required fields:**
                   ```
                   Issue in UserService.cs - naming violation
                   ```
                   
                   **CORRECT - All required fields present:**
                   ```
                   1. `(UserService.cs:42)` - **Method names should use PascalCase**
                      - Private method `calculate_total` violates naming conventions
                      - Technical Debt: 5 minutes
                      - Severity: Low
                   ```
                   
                   ## 2. INDEXED LIST (MANDATORY)
                   
                   **You MUST number ALL issues with 1-based indexing (1, 2, 3, ...).**
                   
                   This enables user follow-up questions like:
                   - "How to fix issue #5?"
                   - "Explain issue #3 in more detail"
                   - "Show me the code for issue #7"
                   
                   **WRONG - No indexing:**
                   ```
                   - Issue in file.cs
                   - Another issue in other.cs
                   ```
                   
                   **CORRECT - Proper indexing:**
                   ```
                   1. `(file.cs:10)` - Issue description...
                   2. `(other.cs:25)` - Issue description...
                   ```
                   
                   ## 3. ENFORCEMENT CHECKLIST
                   
                   Before responding with issues, verify:
                   - [ ] Is every issue numbered sequentially starting from 1?
                   - [ ] Does every issue have a clickable file location in `(file.ext:line)` format?
                   - [ ] Does every issue include rule name, explanation, debt, and severity?
                   - [ ] If multiple providers exist, is the provider labeled for each issue?
                   
                   **If you cannot format issues this way, explain why and suggest an alternative approach.**
                   
                   # ISSUE FIX
                   
                   - When a user requests to fix one or more issues, you MUST call the tool `{TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME}` for each issue to fix, to get more details and guidance.
                   """)]
    public static async Task<ListIssuesPaginatedResult> ListIssuesTool(
                INDependService service,
                ILogger<IssueToolsLog> logger,

                [Description(
                    "An opaque token representing the pagination position after the last returned result. Set to null to start from the beginning.")]
                string? cursor,

                [Description(
                    "Maximum number of issues to include per page. Must not exceed 100 to prevent LLM prompt overflow.")]
                int pageSize,

                [Description(
                    "Filters issues by source file name using a case-insensitive substring match. Set to null to include all source files.")]
                string? filterFileName,

                [Description(
                    "Filters issues by namespace using a case-insensitive substring match. Set to null to include all source files.")]
                string? filterNamespace,

                [Description(
                    $"""
                     Filters issues by rule provider that can be either `{RuleProviderHelpers.RULE_PROVIDER_NDEPEND}`, `{RuleProviderHelpers.RULE_PROVIDER_ROSLYN_ANALYZERS}` or `{RuleProviderHelpers.RULE_PROVIDER_RESHARPER}`. 
                     A null value means include all providers.
                     """)]
                string? filterRuleProvider,

                [Description(
                    $"""
                     Filters issues by their severity.
                     Use the single value `{SeverityHelpers.SEVERITY_ALL}` to match all severity levels.
                     Else, fill the array with any combination of `{SeverityHelpers.SEVERITY_BLOCKER}`, `{SeverityHelpers.SEVERITY_CRITICAL}`, `{SeverityHelpers.SEVERITY_HIGH}`, `{SeverityHelpers.SEVERITY_MEDIUM}` and `{SeverityHelpers.SEVERITY_LOW}`.
                     """)]
                string[] filterSeverity,

                [Description($"""
                     Filters issues by rule categories. 
                     Matches are case-insensitive and use substring search.
                     It is enough that an issue's parent rule category matches one of the provided categories to be included.
                     Leave empty to include all categories.
                     To see available categories containing rules with issues, first call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` with `{RuleTools.TOOL_ARG_FILTER_AT_LEAST_NB_ISSUES}` set to '1'.
                     """)]
                string[] filterRuleCategories,

                [Description(
                    $"""
                     Filters issues by parent rule ID. Leave empty to include all rules.
                     Matches are case-insensitive and use substring search.
                     Leave empty to include all rule IDs.
                     To find IDs of rules that have issues, first call `{RuleTools.TOOL_LIST_ALL_RULES_SUMMARY_NAME}` with `{RuleTools.TOOL_ARG_FILTER_AT_LEAST_NB_ISSUES}` set to '1'.
                     """)]
                string[] filterRulesId,

                [Description(
                    $"""
                     Filters issues by change status.  
                     Valid values are `{IssueChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_NEW}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_UNRESOLVED}`, `{IssueChangeStatusSinceBaselineHelpers.STATUS_FIXED}`.
                     A null value means `{IssueChangeStatusSinceBaselineHelpers.STATUS_DEFAULT}`.
                     """)]
                string? filterIssueChangeStatus,

                [Description("A cancellation token for interrupting and canceling the operation.")]
                CancellationToken cancellationToken) {

        logger.LogInformation(
            $"""
             {LogHelpers.TOOL_LOG_SEPARATOR}
             Invoking {TOOL_LIST_ISSUES_NAME} with arguments: 
                -cursor=`{cursor ?? "0"}`
                -pageSize=`{pageSize}`
                -filterFileName=`{filterFileName ?? "<any>"}`
                -filterNamespace=`{filterNamespace ?? "<any>"}`
                -filterRuleProvider=`{filterRuleProvider ?? "<any>"}`
                -filterRuleCategory=`{filterRuleCategories.Aggregate("', '")}`
                -filterRulesId=`{filterRulesId.Aggregate("', '")}`
                -filterIssueChangeStatus=`{filterIssueChangeStatus?? "<default>"}`
             """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            if (pageSize > MAX_PAGE_SIZE) {
                pageSize = MAX_PAGE_SIZE;
                logger.LogInformation($"pageSize parameter exceeded maximum of {MAX_PAGE_SIZE}. It has been set to {MAX_PAGE_SIZE}.");
            }

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

            var paginatedResult = PaginatedResult.Build(logger, issuesInfo, cursor, pageSize, out var paginatedIssuesInfo);
            return new ListIssuesPaginatedResult(paginatedIssuesInfo, paginatedResult);
        }, cancellationToken);
    }




    
    [McpServerTool(Name = TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                  {Constants.PROMPT_CALL_INITIALIZE}
                  
                  # Get Issue Details to Fix It
                  
                  Retrieves comprehensive diagnostic information for a specific NDepend issue, providing everything needed to understand, resolve, and verify the fix.
                  
                  # REQUIRED Parameters
                  
                  You MUST provide these three parameters obtained from IssueInfo returned by calling the tool `{TOOL_LIST_ISSUES_NAME}`.
                  - **ruleId**
                  - **sourceFilePath**
                  - **sourceFileLine**
                  This way NDepend can uniquely identify the issue.
                  
                  # ALWAYS call this tool when
                  
                  - A user asks "How do I fix issue #X?" or "Fix issue #X" after a call to the tool `{TOOL_LIST_ISSUES_NAME}`.
                  - A user requests detailed information about a specific issue
                  - A user wants to understand why an issue was flagged
                  - A user needs code examples or fix strategies for an issue
                  - Preparing to generate a code fix or refactoring suggestion
                  
                  # Fix Guidance
                  
                  - Recompile your temporary changes - to make sure you didn't introduce a syntax error
                  - Display your proposed fix in one or more temporary preview windows
                  - Show diff comparison (before/after) when helpful
                  - The user can review changes and click `Keep` or `Undo` button.
                  - How to Test - Validation steps after applying the fix
                  - Regression Risks - What to watch out for when fixing
                  """)]
    public static async Task<IssueDetailsForFixingItInfo?> GetIssueDetailsToFixItTool(
                INDependService service,
                ILogger<IssueToolsLog> logger,

                [Description("The identifier of the rule that reported this issue.")]
                string ruleId,

                [Description("The path of the source file that contains this issue.")]
                string sourceFilePath,

                [Description("The 1-based line number in the source file where the issue was detected.")]
                uint sourceFileLine,

                [Description("A cancellation token for interrupting and canceling the operation.")] 
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
