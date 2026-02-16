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
                 
                 # Quality Gates Status
                 
                 Return the status of each quality gate to assess whether code meets defined quality standards.
                 
                 # Purpose and Use-Cases
                 
                 Use this tool when the user asks questions like:
                 
                 **Status Checks:**
                 - "Did the code pass quality gates?"
                 - "What's the quality gate status?"
                 - "Are all quality checks passing?"
                 - "Show me the gate results"
                 - "Check quality gate status"
                 - "Is this code ready to merge?"
                 
                 **Failure Investigation:**
                 - "Why did the quality gate fail?"
                 - "Which quality gates are failing?"
                 - "Show me failed quality checks"
                 - "What needs to be fixed?"
                 - "Which standards aren't met?"
                 
                 **Specific Gate Queries:**
                 - "Did we pass the test coverage gate?"
                 - "What's the maintainability gate status?"
                 - "Is code complexity within limits?"
                 - "Did we meet the coverage threshold?"
                 - "Are there any critical issues?"
                 
                 **Pre-Merge Validation:**
                 - "Can I merge this PR?"
                 - "Is this code ready for review?"
                 - "Are we good to deploy?"
                 - "Check if quality standards are met"
                 - "Validate code quality"
                 - "Ready for production?"
                 
                 **Threshold Details:**
                 - "What are the quality gate thresholds?"
                 - "Show me the passing criteria"
                 - "What standards do we enforce?"
                 - "What are the quality requirements?"
                 - "Which metrics are gated?"
                 
                 **Comparison & Trends:**
                 - "How does this compare to the baseline?"
                 - "Did quality improve?"
                 - "Are we getting better or worse?"
                 - "Compare the quality gate status now and baseline"
                 - "Which quality gate got its value worsen since the baseline?"
                 
                 **Team Standards:**
                 - "What quality gates do we have?"
                 - "List all quality checks"
                 - "What are our quality policies?"
                 - "Which quality gates are enabled?"
                 
                 **Debugging Build Issues:**
                 - "Why is the build blocked?"
                 - "What's stopping deployment?"
                 - "Check gate violations"
                 - "What failed in the pipeline?"
                 """)]
    public static async Task<List<QualityGateInfo>> ListQualityGatesStatusTool(
                INDependService service,
                ILogger<QualityGateToolsLog> logger,
                [Description(
                    $"""
                      Specify whether to search for quality gates from the current analysis or from the baseline snapshot.
                      Value can be either `{CurrentOrBaselineHelpers.CURRENT}` per default, or `{CurrentOrBaselineHelpers.BASELINE}`.
                      """)]
                string currentOrBaseline,
                [Description("A cancellation token for interrupting and canceling the operation.")] 
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
                       WarnThreshold = qg.WarnThreshold ?? 0,
                   }).ToList();

                logger.LogInformation($"{list.Count} quality gates fetched.");
                return list;
            }, cancellationToken);
    }
}
