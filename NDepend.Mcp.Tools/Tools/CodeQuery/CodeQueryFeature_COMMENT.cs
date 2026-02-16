namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string COMMENT_PROMPT =
          """
          # LINES OF COMMENT METRICS
          
          The properties NbLinesOfComment (uint?) and PercentageComment (float?) are defined on ICodeContainer elements.
          The ICodeContainer interface is implemented by IAssembly, INamespace, IType, and IMethod.
          
          These properties returns null in the following cases:
          
          - The code element is abstract (interface, abstract method).
          - The code element is defined in a referenced or third-party assembly.
          - No PDB file nor source was resolved at analysis time for the parent assembly.
          
          ## DEFINITION:
          
          The NbLinesOfComment property represents the number of lines of comments contained in the body of this code element.
          Documentation lines placed above a code element are not included in its NbLinesOfComment count.
          
          The PercentageComment value is computed with the following formula: 
            PercentageComment = 100 * NbLinesOfComment  / ( NbLinesOfComment + NbLinesOfCode ).
          
          ## REMARKS:
          
          Interfaces, abstract methods, and enumerations have a NbLinesOfComment value of null.
          Only concrete code elements that can contain comments are considered.
          
          Notice that this metric is not an additive metric.
          For example, the number of lines of comment of a namespace can be greater than the number of lines of comment over all its types.
          
          ## RECOMMENDATIONS:
          
          Large code portion with less than 20% of comment lines may require more documentation.
          However, overly commented code (more than 40%) is not necessarily better, as it can be seen as insulting the reader’s intelligence
          
          ## USAGE PATTERNS:
          
          ```csharp
          // <Name>Methods with few comments</Name>
          from m in JustMyCode.Methods
          where (m.NbLinesOfCode ?? 0) > 10 && 
               (   (m.NbLinesOfComment ?? 0) < 5
                || (m.PercentageComment ?? 100) < 20 )
          select new {
              m,
              LOC = m.NbLinesOfCode,
              m.NbLinesOfComment,
              m.PercentageComment
          }
          ```
          
          ## BEST PRACTICES:
          
          1. Always handle null comment values using:
             - Null checks: where m.NbLinesOfComment != null
             - Null coalescing: m.PercentageComment ?? 100
          
          2. Exclude generated code from coverage analysis:
             - where !m.IsGeneratedByCompiler (skip generated code)
             - use code base view JustMyCode
          
          3. Combine with other metrics:
             - MaintainabilityIndex (overall maintainability)
             - HalsteadVolume (information density)
             - NbLinesOfCode (size)
             - CyclomaticComplexity (control flow)
             - PercentageCoverage (testing)
          """;
}
