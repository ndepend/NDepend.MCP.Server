
namespace NDepend.Mcp.Tools.CodeQuery {
    [Description("Represents a record of string cell values, typically used for tabular or query result data.")]
    [method: Description("Initializes a new instance of the RecordInfo class with the specified cell values.")]
    public record RecordInfo(string[] Cells, string IssueExplanation) {
        [Description("The array of string cell values for this record.")]
        public string[] Cells { get; set; } = Cells;

        [Description("If this record represents an issue and its explanation is available, returns the issue explanation as string. Else returns an empty string")]
        public string IssueExplanation { get; set; } = IssueExplanation;

        [Description("The file path where the code element is declared, if available.")]
        public string? SourceFilePath { get; set; }
        
        [Description("The line number in the source file where the code element is declared, if available.")]
        public uint? SourceFileLine { get; set; }
    }
}
