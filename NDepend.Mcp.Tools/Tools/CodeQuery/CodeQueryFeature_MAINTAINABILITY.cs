namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string MAINTAINABILITY_PROMPT =
        """
        # Maintainability and Halstead Volume Metrics

        `MaintainabilityIndex` (byte?) and `HalsteadVolume` (uint?) on ICodeContainer (ICodeBase, IAssembly, INamespace, IType, IMethod).
        Returns null for abstract elements and third-party assemblies.

        ## Halstead Volume (HV):

        Quantifies cognitive complexity by counting operators (+, -, if, while...) and operands (variables, literals).

        Methods: < 150 simple | 150-400 acceptable | 400-800 complex | > 800 very high (split or simplify)
        Types:   < 800 simple | 800-3000 acceptable | 3000-8000 complex | > 8000 very high (likely SRP violation)

        ## Maintainability Index (MI):

        Composite of HV + CyclomaticComplexity + LOC, scaled 0-100. Higher = more maintainable.

        85-100: Excellent  |  65-85: Good  |  40-65: Moderate  |  0-40: Poor (refactor)
        Note: auto-generated code may have low MI.

        ## Usage Patterns:

        ```csharp
        //<Name>Find hard-to-maintain methods</Name>
        from m in JustMyCode.Methods
        where m.MaintainabilityIndex < 50
        where !m.IsGeneratedByCompiler
        orderby m.MaintainabilityIndex ascending
        select new { m, MI = m.MaintainabilityIndex, CC = m.CyclomaticComplexity, HV = m.HalsteadVolume, LOC = m.NbLinesOfCode }
        ```

        Null handling: `m.MaintainabilityIndex ?? 100`, `m.HalsteadVolume ?? 0`. Exclude generated: `!m.IsGeneratedByCompiler`.
        """;
}