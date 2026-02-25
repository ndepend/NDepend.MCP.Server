
namespace NDepend.Mcp.Tools.CodeQuery;

[Description("Record of string cell values for tabular/query results")]
public record RecordInfo(string[] Cells, string IssueExplanation) {
    [Description("Cell values")]
    public string[] Cells { get; set; } = Cells;
    [Description("Issue explanation if available, else empty")]
    public string IssueExplanation { get; set; } = IssueExplanation;
    [Description("Source file path if available")]
    public string? SourceFilePath { get; set; }
    [Description("Line number if available")]
    public uint? SourceFileLine { get; set; }
}
