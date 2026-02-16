using PaginatedResult = NDepend.Mcp.Tools.Common.PaginatedResult;

namespace NDepend.Mcp.Tools.CodeQuery {

    [Description("This class let's paginate the server's response to a list code query or rule execution result.")]
    public class ListExecuteCodeQueryPaginatedResult : PaginatedResult {

        public ListExecuteCodeQueryPaginatedResult(
                 IEnumerable<RecordInfo> recordsPaginated,
                 PaginatedResult pr) : base(pr) {
            Records = recordsPaginated;
        }

        [Description(
                  $"""
                     The kind of code query, can be one of the following:
                     `{CodeQueryKind.CODE_QUERY_LIST}`,
                     `{CodeQueryKind.CODE_RULE}`,
                     `{CodeQueryKind.QUALITY_GATE}`,
                     `{CodeQueryKind.QUERYING_ISSUE_AND_RULE}`,
                     `{CodeQueryKind.TREND_METRIC}` or
                     `{CodeQueryKind.CODE_QUERY_SCALAR}`
                     """)]
        public string KindOfCodeQuery { get; set; } = "";

        [Description($"If the parameter KindOfCodeQuery is `{CodeQueryKind.QUALITY_GATE}`, `{CodeQueryKind.CODE_QUERY_SCALAR}` or `{CodeQueryKind.TREND_METRIC}`, return the numerical scalar result.")]
        public double? ScalarResult { get; set; }

        [Description(
           $"""
             If the parameter KindOfCodeQuery is `{CodeQueryKind.QUALITY_GATE}`, returns its status, which can be one of the following:
             `{CodeQuery.ExecutionStatus.STATUS_PASS}`,
             `{CodeQuery.ExecutionStatus.STATUS_WARN}`,
             `{CodeQuery.ExecutionStatus.STATUS_FAIL}`
             
             If the parameter KindOfCodeQuery is `{CodeQueryKind.CODE_RULE}` with onr or more issues, the status is `{CodeQuery.ExecutionStatus.STATUS_WARN}`.
             
             Else the status is `{CodeQuery.ExecutionStatus.STATUS_PASS}`.
             """)]
        public string ExecutionStatus { get; set; } = NDepend.Mcp.Tools.CodeQuery.ExecutionStatus.STATUS_PASS;


        [Description("Gets or sets the column names returned by the code query or rule.")]
        public string[] ColumnNames { get; set; } = [];

        [Description("Gets or sets the sequence of records paginated.")]
        public IEnumerable<RecordInfo> Records { get; set; } = [];

    }
}
