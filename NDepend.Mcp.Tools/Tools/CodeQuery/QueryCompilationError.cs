
namespace NDepend.Mcp.Tools.CodeQuery {

    [Description("Represents a query compilation error.")]
    public sealed class QueryCompilationError {

        [Description("Gets the description of the error.")]
        public string Description { get; set; } = "";

        [Description("Gets the start position of the error highlight in the original-query-string.")]
        public int SubStringStartPos { get; set; }

        [Description("Gets the length of the error highlight in the original-query-string.")]
        public int SubStringLength { get; set; }
    }
}
