namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string COVERAGE_PROMPT =
         """
         # Code Coverage Metrics

         `PercentageCoverage` (float?), `NbLinesOfCodeCovered` (uint?), `NbLinesOfCodeNotCovered` (uint?) on ICodeContainer (IAssembly, INamespace, IType, IMethod).
         Returns null when coverage data is not imported, or for abstract/interface elements.
         `CoverageDataAvailable` (bool) — check before filtering on coverage.
         `IsExcludedFromCoverage` (bool) — element excluded via ExcludeFromCodeCoverageAttribute, DebuggerNonUserCodeAttribute, or DebuggerHiddenAttribute.

         ## Coverage Thresholds (common standards):
         90%+: Good  |  60-90%: Adequate  |  40-60%: Insufficient  |  <40%: Poor  |  0%: Untested

         ## Common Query Patterns:

         ```csharp
         // Under-tested methods
         from m in JustMyCode.Methods
         where (m.PercentageCoverage ?? 100) < 80
         orderby m.PercentageCoverage ascending
         select new { m, Coverage = m.PercentageCoverage, UncoveredLines = m.NbLinesOfCodeNotCovered }
         ```

         ```csharp
         // High-risk: complex and under-tested
         from m in JustMyCode.Methods
         where m.CyclomaticComplexity > 10
         where (m.PercentageCoverage ?? 0) < 50
         let risk = m.CyclomaticComplexity * (100 - (m.PercentageCoverage ?? 0))
         orderby risk descending
         select new { m, Complexity = m.CyclomaticComplexity, Coverage = m.PercentageCoverage, RiskScore = risk }
         ```

         Null handling: `m.PercentageCoverage ?? 0`. Exclude generated: `!m.IsGeneratedByCompiler`.
         """;
}