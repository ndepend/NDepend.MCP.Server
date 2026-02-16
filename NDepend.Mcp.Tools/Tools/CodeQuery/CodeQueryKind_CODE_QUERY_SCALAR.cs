
namespace NDepend.Mcp.Tools.CodeQuery; 
internal partial class CodeQueryKind {
    internal const string CODE_QUERY_SCALAR_PROMPT =
         """
         You will generate an NDepend code query that returns a SINGLE SCALAR VALUE (not a collection).
         
         RETURN TYPES:
         The query must return one of these types:
         - int or int? (integer count or sum)
         - float or float? (decimal metric or percentage)
         
         QUERY PATTERNS:
         
         Pattern 1: AGGREGATE FUNCTION
         Use built-in LINQ aggregates that directly return a scalar:
         
         Domain.Count(codeElement => codeElement.Condition)
         Domain.Max(codeElement => codeElement.Property)
         Domain.Min(codeElement => codeElement.Property)
         Domain.Average(codeElement => codeElement.Property)
         Domain.Sum(codeElement => codeElement.Property)
         
         Examples:
         - JustMyCode.Methods.Count(m => m.CyclomaticComplexity > 15)
         - Application.Types.Max(t => t.NbLinesOfCode)
         - JustMyCode.Namespaces.Average(n => n.PercentageCoverage)
         
         Pattern 2: COMPLEX CALCULATION
         Use 'let' clauses to compute intermediate values, then calculate final scalar.
         The C# LINQ query can start with several 'let' clauses followed by a 'select' that returns the scalar.
         
         let variable1 = computation1
         let variable2 = computation2
         select expression_returning_scalar
         
         Example (ratio or percentage calculation):
         let justMyCodeLinesCovered = (float)JustMyCode.Methods.Sum(m => m.NbLinesOfCodeCovered)
         let justMyCodeLines = (float)JustMyCode.Methods.Sum(m => m.NbLinesOfCode)
         select 100f* justMyCodeLinesCovered  / justMyCodeLines
         
         CRITICAL RULES:
         1. The final result MUST be a single number, NOT a collection/list/set
         3. Add 'f' suffix to float literals: 100f, 0.5f
         4. Handle division by zero with 'where' clauses when necessary
         5. For percentages, multiply by 100f: 100f * (part / whole)
         """;
}
