
using System.Diagnostics;
using System.Xml.Linq;
using NDepend.Path;
using NDepend.Project;

namespace NDepend.Mcp.Helpers; 
internal static class ProjectFromSolutionHelpers {

    internal static bool TryGetNDependProjectFromSolution(
            ILogger<InitializeToolsLog> logger,
            IAbsoluteFilePath solutionFilePathTyped,
            out IAbsoluteFilePath? projectFilePath) {
        bool isSlnxExt = solutionFilePathTyped.HasExtension(".slnx");
        if ((isSlnxExt && TryGetNDependProjectAttachedToSlnx(solutionFilePathTyped, out projectFilePath))
            || (!isSlnxExt && TryGetNDependProjectAttachedToSln(solutionFilePathTyped, out projectFilePath))) {
            logger.LogInformation($"Found the NDepend project `{projectFilePath!.ToString()}` attached to the solution.");
            return true;
        }
        logger.LogInformation($"Cannot get an NDepend project attached to the solution `{solutionFilePathTyped.ToString()}`.");

        // Try get the .ndproj side by side with the solution file
        projectFilePath = GetSideBySideProjectFilePath(solutionFilePathTyped);
        if (projectFilePath.Exists) {
            logger.LogInformation($"Found the NDepend project `{projectFilePath.ToString()}` side-by-side with the solution.");
            return true;
        }
        return false;
    }

    internal static IProject CreateNDependProjectSideBySideWithTheSolution(
            ILogger logger, 
            IAbsoluteFilePath solutionFilePath) {
        var projectFilePath = GetSideBySideProjectFilePath(solutionFilePath);
        logger.LogInformation(
    $"""
             No NDepend project side-by-side with the solution `{solutionFilePath.ToString()}` found.
             Create the project `{projectFilePath.ToString()}` and run analysis.
             """);
        Debug.Assert(!projectFilePath.Exists); // Coz we searched for side-by-side file before calling this method
        var projectManager = new NDependServicesProvider().ProjectManager;
        var project = projectManager.CreateBlankProject(projectFilePath, solutionFilePath.FileNameWithoutExtension);
        project.CodeToAnalyze.SetIDEFiles([new IDEFile(solutionFilePath, "", IDEFileRootDirResolvingInfo.Default)]);
        projectManager.SaveProject(project);
        return project;
    }

    private static IAbsoluteFilePath GetSideBySideProjectFilePath(IAbsoluteFilePath solutionFilePath) {
        var projectFilePath = solutionFilePath.ParentDirectoryPath.GetChildFileWithName(
            solutionFilePath.FileNameWithoutExtension + ".ndproj");
        return projectFilePath;
    }





    // .slnx content looks like
    //<Solution>
    //  <Project Path="ConsoleAppX/ConsoleAppX.csproj" />
    //  <Properties Name="NDepend">
    //    <Property Name="Project" Value="&quot;.\_ConsoleAppX.ndproj&quot;" />
    //  </Properties>
    //</Solution>
    private static bool TryGetNDependProjectAttachedToSlnx(
            IAbsoluteFilePath solutionFilePathTyped,
            out IAbsoluteFilePath? projectFilePath) {
        Debug.Assert(solutionFilePathTyped.FileExtension.Equals(".slnx", StringComparison.OrdinalIgnoreCase));

        projectFilePath = null;
        var solutionFilePathTypedStr = solutionFilePathTyped.ToString();
        Debug.Assert(solutionFilePathTypedStr != null);
        string xml = File.ReadAllText(solutionFilePathTypedStr);
        var doc = XDocument.Parse(xml);

        // Select <Properties Name="NDepend">
        var ndependProps = doc.Root?
                   .Elements("Properties")
                   .FirstOrDefault(p =>
                       string.Equals(
                           p.Attribute("Name")?.Value,
                           "NDepend",
                           StringComparison.OrdinalIgnoreCase));
        if (ndependProps == null) { return false; }

        // Select Value in <Property Name="Project" Value="&quot;.\_ConsoleAppX.ndproj&quot;" />
        var projectValueRaw = ndependProps.Elements("Property")
                        .FirstOrDefault(prop =>
                            string.Equals(
                                prop.Attribute("Name")?.Value,
                                "Project",
                                StringComparison.OrdinalIgnoreCase))
                        ?.Attribute("Value")
                        ?.Value;
        if (projectValueRaw == null) { return false; }

        // Decode XML entities (it might contains &quot;)
        string projectRelativePathString = System.Net.WebUtility.HtmlDecode(projectValueRaw);
        return ProjectFromSolutionHelpers.TryGetProjectFromRelativePath(solutionFilePathTyped, projectRelativePathString, out projectFilePath);
    }


    // .sln content looks like
    //Global
    // ...
    //    GlobalSection(NDepend) = preSolution
    //        Project = ".\AnalysisResultOfThisVersion\NDependIn.ndproj"
    //    EndGlobalSection
    //EndGlobal
    private static bool TryGetNDependProjectAttachedToSln(
            IAbsoluteFilePath solutionFilePathTyped,
            out IAbsoluteFilePath? projectFilePath) {
        Debug.Assert(solutionFilePathTyped.FileExtension.Equals(".sln", StringComparison.OrdinalIgnoreCase));

        string? solutionFilePathTypedStr = solutionFilePathTyped.ToString();
        Debug.Assert(solutionFilePathTypedStr != null);
        string[] lines = File.ReadAllLines(solutionFilePathTypedStr);

        bool inNDependSection = false;
        foreach (string rawLine in lines) {
            if (inNDependSection) {
                if (rawLine.Contains("EndGlobalSection", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }
                const string PROJECT_EQUALS = "Project =";
                if (rawLine.Contains(PROJECT_EQUALS, StringComparison.OrdinalIgnoreCase)) {
                    string projectRelativePathString = rawLine.Replace(PROJECT_EQUALS, "", StringComparison.Ordinal);
                    return TryGetProjectFromRelativePath(solutionFilePathTyped, projectRelativePathString, out projectFilePath);
                }
            } else if (rawLine.Contains("GlobalSection(NDepend)", StringComparison.OrdinalIgnoreCase)) {
                inNDependSection = true;
            }
        }
        projectFilePath = null;
        return false; // Not found
    }


    private static bool TryGetProjectFromRelativePath(
              IAbsoluteFilePath solutionFilePathTyped,
              string? projectRelativePathString,
              out IAbsoluteFilePath? projectFilePath) {
        projectFilePath = null;
        if (string.IsNullOrEmpty(projectRelativePathString)) { return false; }

        projectRelativePathString = projectRelativePathString.Replace("\"", "").Trim();
        return projectRelativePathString.TryGetRelativeFilePath(out var projectRelativePath) &&
               projectRelativePath.TryGetAbsolutePathFrom(solutionFilePathTyped.ParentDirectoryPath, out projectFilePath, out _);
    }
}
