namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string COMPLEXITY_PROMPT =
          """
          # CYCLOMATIC COMPLEXITY (CC) METRICS
          
          The property CyclomaticComplexity (uint?) is defined on IMethod and IType elements.
          
          This property returns null in the following cases:
          
          - The code element is abstract (interface, abstract method).
          - The code element is defined in a referenced or third-party assembly.
          - No PDB file nor source was resolved at analysis time for the parent assembly.
          
          When CyclomaticComplexity is null due to missing PDB information, you can use the ILCyclomaticComplexity (uint?) property instead.
                  
          ## DEFINITION:
          
          Cyclomatic Complexity is a procedural software metric that represents the number of independent execution paths through a method.
          It corresponds to the number of decisions that can be taken within the method.
          
          In C#, the Cyclomatic Complexity of a method is computed as 1 plus the number of the following expressions found in the method body:
            if, while, for, foreach, case, default, continue, goto,
            &&, ||, catch, ternary operator (?:), null-coalescing operator (??)
          
          The following expressions are NOT counted when computing Cyclomatic Complexity:
          
            else, do, switch, try, using, throw, finally, return,
            object creation, method calls, field access
          
          The Cyclomatic Complexity of lambda expressions or anonymous methods is not included in the Cyclomatic Complexity of their enclosing method.
          
          For a type, the Cyclomatic Complexity is the sum of the Cyclomatic Complexities of all its methods.
          
          ## RECOMMENDATIONS:
          
          - Methods with a Cyclomatic Complexity greater than 15 are difficult to understand and maintain.
          - Methods with a Cyclomatic Complexity greater than 30 are extremely complex and should be
            refactored into smaller methods, unless they are automatically generated.       
          
          ## USAGE PATTERNS:
          
          ```csharp
          // <Name>Complex methods with declining maintainability</Name>
          from m in JustMyCode.Methods
          where (m.CyclomaticComplexity ?? 0) > 15
          where (m.MaintainabilityIndex ?? 100) < 65
          select new {
              m,
              CC = m.CyclomaticComplexity,
              MI = m.MaintainabilityIndex
          }
          ```
          
          ## BEST PRACTICES:
          
          1. Always handle null coverage values using:
             - Null checks: where m.CyclomaticComplexity != null
             - Null coalescing: m.CyclomaticComplexity ?? 0
          
          2. Exclude generated code from coverage analysis:
             - where !m.IsGeneratedByCompiler (skip generated code)
             - use code base view JustMyCode
          
          3. Combine with other metrics:
             - MaintainabilityIndex (overall maintainability)
             - HalsteadVolume (information density)
             - NbLinesOfCode (size)
             - PercentageCoverage (testing)
          """;
}
