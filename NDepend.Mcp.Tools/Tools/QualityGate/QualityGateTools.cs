using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.QualityGate;

[McpServerToolType]
public static class QualityGateTools {

    internal const string TOOL_QUALITY_GATES_STATUS_NAME = Constants.TOOL_NAME_PREFIX + "list-quality-gates-status";

    [McpServerTool(Name = TOOL_QUALITY_GATES_STATUS_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
                 {Constants.PROMPT_CALL_INITIALIZE}
                 
                 Returns the status of quality gates to assess if code meets defined standards.
                 
                 **Use when users ask:**
                 - Status: "Did code pass quality gates?", "Show gate results", "Ready to merge?"
                 - Failures: "Why did gate fail?", "Which gates failing?", "What needs fixing?"
                 - Specific gates: "Did we pass coverage?", "Is complexity within limits?"
                 - Pre-merge: "Can I merge?", "Ready for review/deploy?"
                 - Thresholds: "What are the passing criteria?", "Which metrics are gated?"
                 - Trends: "How does this compare to baseline?", "Did quality improve?"
                 - Team standards: "What quality gates do we have?", "List all checks"
                 - Debugging: "Why is build blocked?", "What failed in pipeline?"
                 """)]
    public static async Task<List<QualityGateInfo>> ListQualityGatesStatusTool(
                INDependService service,
                ILogger<QualityGateToolsLog> logger,
                
                [Description(
                    $"""
                    Search quality gates status {CodeElementApplyFilter.FROM_CURRENT_OR_BASELINE_ENUM}.
                    """)]
                string currentOrBaseline,
                
                CancellationToken cancellationToken) {

            logger.LogInformation(
                    $"""
                    {LogHelpers.TOOL_LOG_SEPARATOR}               
                    Invoking {TOOL_QUALITY_GATES_STATUS_NAME} with argument: currentOrBaseline=`{currentOrBaseline}`
                    """);
            if(!service.IsInitialized(out Session session)) {
                logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
            }
            return await Task.Run(() => {

                var issuesSet = session.IssuesSetDiff.NewerIssuesSet; // not null coz service.IsInitialized
                if (CurrentOrBaselineHelpers.GetCurrentOrBaselineVal(logger, currentOrBaseline) == CurrentOrBaseline.Baseline) {
                    issuesSet = session.IssuesSetDiff.OlderIssuesSet;
                }

                var list = issuesSet.AllQualityGates.Select(
                   qg => new QualityGateInfo {
                       Status = qg.Status,
                       Name = qg.Name,
                       Description = qg.QueryString,
                       Unit = qg.Unit,
                       Value = qg.Value ?? 0,
                       ValueString = qg.ValueString,
                       MoreIsBad = qg.MoreIsBad,
                       FailThreshold = qg.FailThreshold,
                       WarnThreshold = qg.WarnThreshold ?? 0
                   }).ToList();

                logger.LogInformation($"{list.Count} quality gates fetched.");
                return list;
            }, cancellationToken);
    }
}
