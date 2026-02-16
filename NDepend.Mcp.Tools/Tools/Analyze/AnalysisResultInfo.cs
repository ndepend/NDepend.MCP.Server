namespace NDepend.Mcp.Tools.Analyze;

[Description("Represents information about the current analysis result and the baseline one.")]
public record AnalysisResultInfo(

    [property: Description("The date and time when the current analysis was executed.")]
    DateTime ResultDate,
    [property: Description("The name of the project associated with the current analysis result.")]
    string ResultProjectName,
    [property: Description("The absolute file path to the project file for the current analysis result.")]
    string ResultProjectFilePath,

    [property: Description("The date and time when the baseline analysis was executed.")]
    DateTime BaselineResultDate
);
