
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Source {

    [Description($"Source code retrieval result of the tool `{SourceTools.TOOL_SOURCE_NAME}`.")]
    public sealed class SourceResult {

        [Description("File path")]
        public string SourceFilePath { get; set; } = "";

        [Description(
             $"""
              `{CurrentOrBaselineHelpers.CURRENT}` per default, or `{CurrentOrBaselineHelpers.BASELINE}` snapshot
              """)]
        public string CurrentOrBaseline { get; set; } = "";

        [Description("Source code as raw text.")]
        public string SourceCode { get; set; } = "";
    }
}
