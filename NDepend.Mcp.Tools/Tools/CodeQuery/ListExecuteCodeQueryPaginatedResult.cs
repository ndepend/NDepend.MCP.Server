using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.CodeQuery {

    
    [Description($"Enables pagination for `{CodeQueryTools.TOOL_RUN_QUERY_NAME}` MCP tool responses.")]
    public class ListExecuteCodeQueryPaginatedResult : PaginatedResult {

        public ListExecuteCodeQueryPaginatedResult(
                 IEnumerable<RecordInfo> recordsPaginated,
                 PaginatedResult pr) : base(pr) {
            Records = recordsPaginated;
        }

        [Description(
                  $"""
                     Query kind, one of:
                     `{CodeQueryKind.CODE_QUERY_LIST}`,
                     `{CodeQueryKind.CODE_RULE}`,
                     `{CodeQueryKind.QUALITY_GATE}`,
                     `{CodeQueryKind.QUERYING_ISSUE_AND_RULE}`,
                     `{CodeQueryKind.TREND_METRIC}` or
                     `{CodeQueryKind.CODE_QUERY_SCALAR}`
                     """)]
        public string KindOfCodeQuery { get; set; } = "";

        [Description($"Scalar result if query kind is `{CodeQueryKind.QUALITY_GATE}`, `{CodeQueryKind.CODE_QUERY_SCALAR}` or `{CodeQueryKind.TREND_METRIC}`, return the numerical scalar result.")]
        public double? ScalarResult { get; set; }

        [Description(
           $"""
             If query kind is `{CodeQueryKind.QUALITY_GATE}` returns `{CodeQuery.ExecutionStatus.STATUS_PASS}`, `{CodeQuery.ExecutionStatus.STATUS_WARN}`, or `{CodeQuery.ExecutionStatus.STATUS_FAIL}`.
             If query kind is `{CodeQueryKind.CODE_RULE}` with issues returns `{CodeQuery.ExecutionStatus.STATUS_WARN}`.
             Otherwise returns `{CodeQuery.ExecutionStatus.STATUS_PASS}`.
             """)]
        public string ExecutionStatus { get; set; } = NDepend.Mcp.Tools.CodeQuery.ExecutionStatus.STATUS_PASS;


        [Description("Column names")]
        public string[] ColumnNames { get; set; } = [];

        [Description("Paginated records")]
        public IEnumerable<RecordInfo> Records { get; set; } = [];

    }
}
