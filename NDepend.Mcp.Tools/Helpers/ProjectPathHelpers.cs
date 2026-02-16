
using System.Diagnostics;
using System.Xml.Linq;
using NDepend.Path;

namespace NDepend.Mcp.Helpers; 
internal static class ProjectPathHelpers {
    internal static bool TryGetSideBySideProjectFile(IAbsoluteFilePath solutionFilePathTyped, out IAbsoluteFilePath projectFilePath) {
        projectFilePath = GetSideBySideProjectFile(solutionFilePathTyped);
        return projectFilePath.Exists;
    }

    internal static IAbsoluteFilePath GetSideBySideProjectFile(IAbsoluteFilePath solutionFilePath) {
        var projectFilePath = solutionFilePath.ParentDirectoryPath.GetChildFileWithName(solutionFilePath.FileNameWithoutExtension + ".ndproj");
        return projectFilePath;
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



    internal static bool TryGetProjectFromSlnx(
            IAbsoluteFilePath solutionFilePathTyped,
            out IAbsoluteFilePath? projectFilePath) {
        Debug.Assert(solutionFilePathTyped.FileExtension.Equals(".slnx", StringComparison.OrdinalIgnoreCase));
        //<Solution>
        //  <Project Path="ConsoleAppX/ConsoleAppX.csproj" />
        //  <Properties Name="NDepend">
        //    <Property Name="Project" Value="&quot;.\_ConsoleAppX.ndproj&quot;" />
        //  </Properties>
        //</Solution>
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
        return ProjectPathHelpers.TryGetProjectFromRelativePath(solutionFilePathTyped, projectRelativePathString, out projectFilePath);
    }

    internal static bool TryGetProjectFromSln(
            IAbsoluteFilePath solutionFilePathTyped,
            out IAbsoluteFilePath? projectFilePath) {
        Debug.Assert(solutionFilePathTyped.FileExtension.Equals(".sln", StringComparison.OrdinalIgnoreCase));
        //Global
        // ...
        //    GlobalSection(NDepend) = preSolution
        //        Project = ".\AnalysisResultOfThisVersion\NDependIn.ndproj"
        //    EndGlobalSection
        //EndGlobal

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



}
