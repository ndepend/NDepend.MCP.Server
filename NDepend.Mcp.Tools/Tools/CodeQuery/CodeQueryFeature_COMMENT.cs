namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string COMMENT_PROMPT =
          """
          # Lines of Comment Metrics

          `NbLinesOfComment` (uint?) and `PercentageComment` (float?) on ICodeContainer (IAssembly, INamespace, IType, IMethod).
          Returns null for abstract elements, third-party, or missing PDB/source.

          ## Definition:

          `NbLinesOfComment` = comment lines inside the code element body (excludes doc-comments above the element).
          `PercentageComment` = 100 * NbLinesOfComment / (NbLinesOfComment + NbLinesOfCode).
          Not an additive metric: namespace comment count can exceed the sum of its types.

          ## Recommendations:

          - < 20% comment lines on large code sections: may need more documentation
          - > 40%: excessive comments can obscure intent

          ## Usage Patterns:

          ```csharp
          // <Name>Methods with few comments</Name>
          from m in JustMyCode.Methods
          where (m.NbLinesOfCode ?? 0) > 10 &&
               (   (m.NbLinesOfComment ?? 0) < 5
                || (m.PercentageComment ?? 100) < 20 )
          select new { m, LOC = m.NbLinesOfCode, m.NbLinesOfComment, m.PercentageComment }
          ```

          Null handling: `m.NbLinesOfComment ?? 0`, `m.PercentageComment ?? 100`. Exclude generated: `!m.IsGeneratedByCompiler`.
          """;
}