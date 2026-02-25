using NDepend.CodeModel;
using NDepend.CodeQuery;
using NDepend.Helpers;
using NDepend.Issue;
using NDepend.Mcp.Helpers;
using NDepend.Mcp.Services;
using NDepend.Mcp.Tools.Common;
using NDepend.Path;
using NDepend.TechnicalDebt;
using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.CodeQuery;


public static partial class CodeQueryTools {
    
    internal const string TOOL_RUN_QUERY_NAME = Constants.TOOL_NAME_PREFIX + "run-code-query-or-rule";

    const string MAX_PAGE_SIZE = "100";

    [McpServerTool(Name = TOOL_RUN_QUERY_NAME, Idempotent = false, ReadOnly = true, Destructive = false, OpenWorld = false),
     Description($"""
              {Constants.PROMPT_CALL_INITIALIZE}
              
              # Compile and Run
              
              Compile and execute a code query or rule on the initialized NDepend analysis.
              
              ## Output
              
              Returns matching records/issues (paginated to prevent prompt overflow)
              
              ## Response Formatting
              
              ### 1. Clickable Locations (MANDATORY)
              Always show: `(file.ext:line)`
              Example: `Null check needed in GetUser (UserService.cs:45)`
              
              ### 2. Number All Rows (1-based) (MANDATORY)
              Format: `1. `, `2. `, `3. `
              Enables: `Fix #2`, `Explain #5`
              
              ## Errors
              
              Compile errors → Throws `McpException` listing all errors
              
              Execution failure → Throws `McpException` with explanation
              
              ## Note
              
              **REQUIRED:** Call `{TOOL_GEN_QUERY_NAME}` prior generating any query or rule.
              """)]
    public static async Task<ListExecuteCodeQueryPaginatedResult> RunCodeQueryOrRule(
            INDependService service,
            ILogger<CodeQueryToolsLog> logger,

            [Description(PaginatedResult.PAGINATION_CURSOR_DESC)]
            int cursor,

            [Description($"Max number of rows per page (≤ {MAX_PAGE_SIZE}) to avoid LLM prompt overflow.")]
            int pageSize,

            [Description("Code query or rule to compile and execute")]
            string codeQueryOrRule,

            [Description("Default: `False`. Set `True` only for dependency related queries")]
            bool exportGraph,

            [Description("Default: `False`. Set `True` to check whether the query compile (do not execute)")]
            bool compileOnly,

            CancellationToken cancellationToken) {
        logger.LogInformation(
        $"""
         {LogHelpers.TOOL_LOG_SEPARATOR}
         Invoking {TOOL_RUN_QUERY_NAME} with arguments: 
            -cursor= `{cursor}`
            -pageSize= `{pageSize}`
            -codeQueryOrRule: `{codeQueryOrRule}`
            -exportGraph: `{exportGraph.ToString()}`
            -compileOnly: `{compileOnly.ToString()}`
         """);
        if (!service.IsInitialized(out Session session)) {
            logger.LogErrorAndThrow(Constants.PROMPT_CALL_INITIALIZE);
        }

        return await Task.Run(() => {
            //
            // Compile the code query or rule
            //
            IQueryCompiled queryCompiled = codeQueryOrRule.Compile(session.CompareContext);
            if (queryCompiled.HasErrors) {
                var sb = new StringBuilder();
                var errors = queryCompiled.QueryCompiledError.Errors;
                foreach (IQueryCompilationError error in errors) {
                    sb.Append($"""
                           - Starts at {error.SubStringStartPos}, Length {error.SubStringLength}: {error.Description}
                           """);
                }
                logger.LogErrorAndThrow(
                    $"""
                     The code query doesn't compile and cannot be executed.
                     {errors.Count.ToString()} compilation error{(errors.Count > 1 ? "s" : "")}:
                     {sb.ToString()}                
                     """);
            }
            if(compileOnly) {
                return new ListExecuteCodeQueryPaginatedResult([], PaginatedResult.Empty);
            }
            IQueryCompiledSuccess queryCompiledSuccess = queryCompiled.QueryCompiledSuccess;

            //
            // Execute the code query or rule
            //
            const int TIME_OUT_SECONDS = 10;
            var project = session.Project;
            IQueryExecutionResult executionResult =
                queryCompiledSuccess.Execute(
                    new QueryExecutionContext(session.CompareContext.NewerCodeBase) {
                        JustMyCode = session.JustMyCode,
                        IssuesSet = session.IssuesSet,
                        IssuesSetDiff = session.IssuesSetDiff,
                        DebtSettings = project.DebtSettings.Values,
                        CQLinqQueryExecTimeOut = TIME_OUT_SECONDS.ToSeconds()
                    });

            switch (executionResult.Status) {
                case QueryExecutionStatus.TimeOut:
                    logger.LogErrorAndThrow($"The execution of the code query or code rule timed out after {TIME_OUT_SECONDS} seconds.");
                    break;
                case QueryExecutionStatus.Exception:
                    Exception ex = executionResult.Exception!;
                    logger.LogErrorAndThrow($"""
                                         An exception occurred during the execution of the code {(queryCompiledSuccess.KindOfCodeQueryExpression == KindOfCodeQueryExpression.Rule ? "rule" : "query")}:
                                         Exception.Type: "{ex.GetType().ToString()}"
                                         Exception.Message: "{ex.Message}"
                                         """);
                    break;
            }

            IQueryExecutionSuccessResult successResult = executionResult.SuccessResult!;

            //
            // Status is QueryExecutionStatus.Success here
            //
            bool iScalarResult = successResult.IsSingleScalarResult;

            // Eventually Export graph if requested + query results is code elements
            if (exportGraph && 
                !iScalarResult &&
                !successResult.KindOfMatch.EqualsAny(RecordCellType.Issue, RecordCellType.Rule, RecordCellType.QualityGate)) {
                ExportGraph(logger, session.CompareContext.NewerCodeBase, successResult);
            }

            // Convert successResult.Records to JSON formattable and returnable List<RecordInfo>
            bool isRuleViolated = successResult.IsARuleViolated;
    
            var recordsInfo = new List<RecordInfo>();
            if (!iScalarResult) {
                IReadOnlyList<IIssue>? issues = null;
                if (isRuleViolated) {
                    issues = successResult.MatchedIssues;
                }

                var debtFormatter = project.DebtSettings.Values.CreateDebtFormatter();
                var records = successResult.Records;
                for(int i =0; i < records.Count; i++) {
                    IIssue? issue = issues != null && i < issues.Count ? issues[i] : null;
                    recordsInfo.Add(BuildFromRecord(records[i], issue, debtFormatter));
                }
            }

            // Paginate and build finalResult
            var paginatedResult = PaginatedResult.Build(logger, recordsInfo, cursor, pageSize, MAX_PAGE_SIZE, out var paginatedIssuesInfo);
            var finalResult = new ListExecuteCodeQueryPaginatedResult(paginatedIssuesInfo, paginatedResult) {
                ScalarResult = iScalarResult ? successResult.SingleScalarValue : null,

                ColumnNames = iScalarResult ? 
                    ["Scalar Result"] :
                    successResult.ColumnsNames.ToArray(),

                KindOfCodeQuery = queryCompiledSuccess.KindOfCodeQueryExpression switch {
                    KindOfCodeQueryExpression.QualityGate => CodeQueryKind.QUALITY_GATE,
                    KindOfCodeQueryExpression.Rule => CodeQueryKind.CODE_RULE,
                    KindOfCodeQueryExpression.TrendMetric => CodeQueryKind.TREND_METRIC,
                    _ => iScalarResult ? CodeQueryKind.CODE_QUERY_SCALAR :
                         successResult.KindOfMatch.EqualsAny(RecordCellType.Issue, RecordCellType.Rule, RecordCellType.QualityGate) ?
                         CodeQueryKind.QUERYING_ISSUE_AND_RULE :
                         CodeQueryKind.CODE_QUERY_LIST,
                },
                
                ExecutionStatus = queryCompiledSuccess.KindOfCodeQueryExpression == KindOfCodeQueryExpression.QualityGate ?
                    successResult.QualityGateStatus switch { 
                        QualityGateStatus.Fail => ExecutionStatus.STATUS_FAIL,
                        QualityGateStatus.Warn => ExecutionStatus.STATUS_WARN,
                        _                      => ExecutionStatus.STATUS_PASS,
                    }
                    : isRuleViolated
                        ? ExecutionStatus.STATUS_WARN
                        : ExecutionStatus.STATUS_PASS
            };
            return finalResult;
        }, cancellationToken);

    }

    private static void ExportGraph(
               ILogger<CodeQueryToolsLog> logger,
               ICodeBase codeBase,
               IQueryExecutionSuccessResult successResult) {

        var tempHtmlPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "QueryResultGraph." + System.IO.Path.GetRandomFileName() + ".html"
            ).ToAbsoluteFilePath();

        // Max 6 code elements selected for graph export to avoid overload
        if (codeBase.TryExportSvgGraphQueryResult(
                successResult.TargetCodeElements.Take(6),
                successResult.OriginalQueryString, 
                tempHtmlPath, 
                ExportGraphSettings.Default,
                out string failureReason)) {
            BrowserHelpers.OpenHtmlDiagram(logger, tempHtmlPath);
        } else {
            logger.LogCannotExportGraphError(tempHtmlPath, failureReason);
        }

    }

    private static RecordInfo BuildFromRecord(RecordBase record, IIssue? issue, IDebtFormatter debtFormatter) {
        int arity = record.RecordArity;
        var cells = new string[arity];

        // Fill cells
        for (int i = 0; i < arity; i++) {
            cells[i] = record[i].GetRecordCellValueDescription(debtFormatter);
        }

        // Eventually get issue explanation if this record represents an issue
        string issueExplanation = "";
        if (issue != null) {
            if (!issue.TryGetExplanation(StringFormattingKind.Readable, debtFormatter, out issueExplanation)) {
                issueExplanation = "";
            }
        }
        var ri = new RecordInfo(cells, issueExplanation);

        // Eventually fill source info
        object firstCell = record[0].m_UntypedValue;
        if (firstCell is ICodeElement codeElement) {
            codeElement.ExtractSourceDecl(null, out string? filePath, out uint? line);
            ri.SourceFilePath = filePath;
            ri.SourceFileLine = line;
        } else if (firstCell is IIssue { SourceFileDeclAvailable: true } issueCell) {
            ri.SourceFilePath = issueCell.SourceDecl.SourceFile.FilePathString;
            ri.SourceFileLine = issueCell.SourceDecl.Line;
        }
        return ri;
    }
}
