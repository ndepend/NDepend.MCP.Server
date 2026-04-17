namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string COMPLEXITY_PROMPT =
          """
          # Cyclomatic Complexity (CC) Metrics

          `CyclomaticComplexity` (uint?) on IMethod and IType. Returns null for abstract elements, third-party, or missing PDB.
          Use `ILCyclomaticComplexity` (uint?) as fallback when PDB is unavailable.

          ## Definition:

          Number of independent execution paths: 1 + count of `if`, `while`, `for`, `foreach`, `case`, `default`,
          `continue`, `goto`, `&&`, `||`, `catch`, `?:`, `??` in the method body.
          NOT counted: `else`, `do`, `switch`, `try`, `using`, `throw`, `finally`, `return`, object creation, method calls.
          Lambdas/anonymous methods have their own separate CC value.
          For types: sum of all method CCs.

          ## Recommendations:

          - CC > 15: difficult to understand and maintain
          - CC > 30: extremely complex, refactor into smaller methods (unless auto-generated)

          ## Usage Patterns:

          ```csharp
          // <Name>Complex methods with declining maintainability</Name>
          from m in JustMyCode.Methods
          where (m.CyclomaticComplexity ?? 0) > 15
          where (m.MaintainabilityIndex ?? 100) < 65
          select new { m, CC = m.CyclomaticComplexity, MI = m.MaintainabilityIndex }
          ```

          Null handling: `m.CyclomaticComplexity ?? 0`. Exclude generated: `!m.IsGeneratedByCompiler`.
          """;
}