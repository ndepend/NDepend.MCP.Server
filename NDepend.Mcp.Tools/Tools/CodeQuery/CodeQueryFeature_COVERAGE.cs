namespace NDepend.Mcp.Tools.CodeQuery;
public partial class CodeQueryFeature {
    internal const string COVERAGE_PROMPT =
         """
         # Code Coverage Metrics

         `PercentageCoverage` (float?), `NbLinesOfCodeCovered` (uint?), `NbLinesOfCodeNotCovered` (uint?) on ICodeContainer (IAssembly, INamespace, IType, IMethod).
         Returns null when coverage data is not imported, or for abstract/interface elements.
         `CoverageDataAvailable` (bool) — check before filtering on coverage.
         `IsExcludedFromCoverage` (bool) — element excluded via ExcludeFromCodeCoverageAttribute, DebuggerNonUserCodeAttribute, or DebuggerHiddenAttribute.

         ## Coverage Is About the Application Code
         Test assemblies are in the `TestCode` domain, never in `JustMyCode` / `Application`: "under-tested" queries on
         `JustMyCode.Methods` don't list test methods. The Dashboard, the default quality gates and trend metrics compute
         the coverage on `Application.Assemblies` (the test assemblies' own coverage is not counted).
         `codeBase.PercentageCoverage` is the whole code base (test assemblies included): prefer the application formula below.
         Test methods: `TestCode.Methods.Where(m => m.IsTestMethod)`, trend metric `# Tests`.


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

         ```csharp
         // Application code coverage (the Dashboard figure)
         let covered    = (double)Application.Assemblies.Sum(a => a.NbLinesOfCodeCovered ?? 0)
         let notCovered = (double)Application.Assemblies.Sum(a => a.NbLinesOfCodeNotCovered ?? 0)
         select (covered + notCovered) == 0d ? (double?)null : 100d * covered / (covered + notCovered)
         ```

         Null handling: `m.PercentageCoverage ?? 0`. Exclude generated: `!m.IsGeneratedByCompiler`.
         """;
}