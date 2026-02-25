using NDepend.Mcp.Services;

namespace NDepend.Mcp.Tools.Initialize;

[Description("Current and baseline analysis info")]
public record AnalysisResultInfo(
    [property: Description("Current analysis date")]
    DateTime ResultDate,
    [property: Description("Current project name")]
    string ResultProjectName,
    [property: Description("Current project file path")]
    string ResultProjectFilePath,
    [property: Description("Baseline analysis date")]
    DateTime BaselineResultDate,
    [property: Description("Total codebase test coverage (%); null if unavailable")]
    float? PercentageCoverage
) {

    internal static AnalysisResultInfo FromSession(Session session) {
        var arr = session.AnalysisResult.AnalysisResultRef;

        return new AnalysisResultInfo(
            arr.Date,
            arr.Project.Properties.Name,
            arr.Project.Properties.FilePath.ToString()!,
            session.BaselineResult.AnalysisResultRef.Date,
            session.AnalysisResult.CodeBase.PercentageCoverage
        );
    }

}
