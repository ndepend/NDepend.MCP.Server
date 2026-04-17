
namespace NDepend.Mcp.Tools.CodeQuery; 
internal partial class CodeQueryKind {
    internal const string CODE_QUERY_SCALAR_PROMPT =
         """
         Generate a CQLinq query returning a SINGLE SCALAR VALUE — int, int?, float, or float?.

         Pattern 1: Aggregate function (returns scalar directly):
         Domain.Count(x => x.Condition)  |  .Max  |  .Min  |  .Average  |  .Sum

         Pattern 2: Chain `let` clauses, then `select` the scalar:
         let covered = (float)JustMyCode.Methods.Sum(m => m.NbLinesOfCodeCovered)
         let total   = (float)JustMyCode.Methods.Sum(m => m.NbLinesOfCode)
         select 100f * covered / total

         Rules: result MUST be one number; use `f` suffix for float literals; guard divisions with `where`.
         """;
}
