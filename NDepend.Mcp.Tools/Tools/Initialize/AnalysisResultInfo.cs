using NDepend.CodeModel;
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
    [property: Description("Application code test coverage (%)")]
    float? PercentageCoverage,
    [property: Description("# application assemblies")]
    int NbApplicationAssemblies,
    [property: Description("# test assemblies analyzed")]
    int NbTestAssemblies,
    [property: Description("# third-party assemblies referenced")]
    int NbThirdPartyAssemblies,
    [property: Description("# test methods")]
    int NbTests
) {

    internal static AnalysisResultInfo FromSession(Session session) {
        var arr = session.AnalysisResult.AnalysisResultRef;
        ICodeBase codeBase = session.AnalysisResult.CodeBase;

        return new AnalysisResultInfo(
            arr.Date,
            arr.Project.Properties.Name,
            arr.Project.Properties.FilePath.ToString()!,
            session.BaselineResult.AnalysisResultRef.Date,
            GetApplicationPercentageCoverage(codeBase),
            codeBase.Application.Assemblies.Count(),
            codeBase.TestCode.Assemblies.Count(),
            codeBase.ThirdParty.Assemblies.Count(),
            codeBase.TestCode.Methods.Count(m => m.IsTestMethod)
        );
    }

    internal static float? GetApplicationPercentageCoverage(ICodeBase codeBase) {
        if (codeBase.NbLinesOfCodeCovered == null) { return null; }  // no coverage data imported
        double covered = 0, notCovered = 0;
        foreach (IAssembly assembly in codeBase.Application.Assemblies) {
            covered += assembly.NbLinesOfCodeCovered ?? 0;
            notCovered += assembly.NbLinesOfCodeNotCovered ?? 0;
        }
        double coverable = covered + notCovered;
        if (coverable == 0d) { return null; }
        return (float)(100d * covered / coverable);
    }

}
