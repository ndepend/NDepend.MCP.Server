
using NDepend.Mcp.Tools.Common;

namespace NDepend.Mcp.Tools.Source {

    [Description($"Represent the result of a call to the tool `{SourceTools.TOOL_SOURCE_NAME}`.")]
    public sealed class SourceResult {

        [Description("Specify the file path from which the source code has been obtained.")]
        public string SourceFilePath { get; set; } = "";


        [Description(
             $"""
              Specify whether the source file has been obtained from the current analysis or from the baseline snapshot.
              Value can be either `{CurrentOrBaselineHelpers.CURRENT}` per default, or `{CurrentOrBaselineHelpers.BASELINE}`.
              """)]
        public string CurrentOrBaseline { get; set; } = "";


        [Description("The source code as raw text.")]
        public string SourceCode { get; set; } = "";
    }
}
