namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    internal const string LINE_OF_CODE_PROMPT =
          """
          # Lines Of Code(LOC) Metrics

          `NbLinesOfCode` (uint?) on ICodeContainer (ICodeBase, IAssembly, INamespace, IType, IMethod).
          Returns null for abstract elements, third-party assemblies, or missing PDB. Fallback: `NbILInstructions` (uint?). NbILInstructions/LOC ratio ~= 7.

          ## Definition:

          LOC = logical (executable) lines from PDB sequence points, not physical lines.
          Excludes blank lines, comments, braces, and declarations.
          For type/namespace/assembly: sum of all contained method LOC.

          ## Recommendations:

          - LOC > 20: hard to maintain
          - LOC > 40: too complex, split into smaller methods

          ## Usage:

          ```csharp
          //<Name>Large methods with declining maintainability</Name>
          from m in JustMyCode.Methods
          where (m.NbLinesOfCode ?? 0) > 30
          where (m.MaintainabilityIndex ?? 100) < 65
          select new { m, LOC = m.NbLinesOfCode, MI = m.MaintainabilityIndex }
          ```
          
          ## BEST PRACTICES:
          
          1. Always handle null coverage values using:
             - Null checks: where m.NbLinesOfCode != null
             - Null coalescing: m.NbLinesOfCode ?? 0
          
          2. Exclude generated code from coverage analysis:
             - where !m.IsGeneratedByCompiler (skip generated code)
             - use code base view JustMyCode
          
          3. Combine with other metrics:
             - MaintainabilityIndex (overall maintainability)
             - HalsteadVolume (information density)
             - CyclomaticComplexity (control flow)
             - PercentageCoverage (testing)
          """;
}
