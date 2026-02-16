namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string COVERAGE_PROMPT =
         """
         # CODE COVERAGE METRICS
         
         ## COVERAGE-ENABLED CODE ELEMENTS:
         
         The ICodeContainer interface provides code coverage metrics.
         The ICodeContainer interface is implemented by IAssembly, INamespace, IType, and IMethod.
         
         ## AVAILABLE COVERAGE PROPERTIES:
         
         PercentageCoverage → float? (nullable)
         - Returns: Percentage of code covered by tests (0.0 to 100.0)
         
         NbLinesOfCodeCovered → uint? (nullable)
         - Returns: Number of lines executed by at least one test
         
         NbLinesOfCodeNotCovered → uint? (nullable)
         - Returns: Number of lines never executed by tests
         
         These properties return null when coverage data is unavailable, such as when it hasn’t been imported, or for abstract methods or interfaces.
         
         The property ICodeContainer.CoverageDataAvailable → bool indicates if coverage data exists for the element:
         
         The property ICodeContainer.IsExcludedFromCoverage → bool indicates if the element is excluded with an attribute like
         'System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute' 'System.Diagnostics.DebuggerNonUserCodeAttribute' 
         or 'System.Diagnostics.DebuggerHiddenAttribute'.
         
         For example you can write the code query:
         ```csharp
         from m in JustMyCode.Methods
         where (m.PercentageCoverage ?? 100) < 80
         select new { m, m.PercentageCoverage, m.NbLinesOfCode, m.NbLinesOfCodeNotCovered }
         ```
         
         ## HANDLING NULL VALUES:
         
         Always handle nulls in queries:
         ```csharp
         where m.PercentageCoverage.CoverageDataAvailable && m.PercentageCoverage < 80
         where (m.PercentageCoverage ?? 0) < 80  // Treat null as 0%
         ```
         
         ## COVERAGE THRESHOLDS (common standards):
         
         - 90%+: Good coverage
         - 60-90%: Adequate coverage
         - 40-60%: Insufficient coverage
         - <40%: Poor coverage
         - 0%: Untested
         
         ## COMMON QUERY PATTERNS:
         
         PATTERN 1 - Find under-tested methods:
         
         ```csharp
         from m in JustMyCode.Methods
         where m.PercentageCoverage < 80
         orderby m.PercentageCoverage ascending
         select new { 
             m, 
             Coverage = m.PercentageCoverage,
             TotalLines = m.NbLinesOfCode,
             UncoveredLines = m.NbLinesOfCodeNotCovered
         }
         ```
         
         PATTERN 2 - Find completely untested methods:
         
         ```csharp
         from m in JustMyCode.Methods
         where m.PercentageCoverage == 0 || m.PercentageCoverage == null
         where m.NbLinesOfCode > 5  // Exclude trivial methods
         orderby m.NbLinesOfCode descending
         select new { 
             m, 
             Lines = m.NbLinesOfCode,
             IsCompletelyUntested = true
         }
         ```
         
         ## COMBINING COVERAGE WITH OTHER METRICS:
         
         Find complex, untested methods (high risk):
         
         ```csharp
         from m in JustMyCode.Methods
         where m.CyclomaticComplexity > 10
         where (m.PercentageCoverage ?? 0) < 50
         let risk = m.CyclomaticComplexity * (100 - (m.PercentageCoverage ?? 0))
         orderby risk descending
         select new {
             m,
             Complexity = m.CyclomaticComplexity,
             Coverage = m.PercentageCoverage,
             RiskScore = risk,
             UncoveredLines = m.NbLinesOfCodeNotCovered
         }
         ```
         
         ## BEST PRACTICES:
         
         1. Always handle null coverage values using:
            - Null checks: where m.PercentageCoverage != null
            - Null coalescing: m.PercentageCoverage ?? 0
         
         2. Exclude generated code from coverage analysis:
            - where !m.IsGeneratedByCompiler (skip generated code)
            - use code base view JustMyCode
         
         3. Combine coverage with complexity/maintainability for risk assessment:
            - High complexity/maintainability + low coverage = highest risk
            - Low complexity/maintainability + low coverage = lower priority
         
         4. Use appropriate aggregation level:
            - Method-level: Most granular, pinpoint specific problems
            - Type-level: Good for architectural overview
            - Namespace-level: Identify problem areas
            - Assembly-level: High-level quality gates
         
         5. Sort by coverage ascending to see worst cases first:
            - orderby m.PercentageCoverage ascending
            - This shows untested code at the top
         """;
}
