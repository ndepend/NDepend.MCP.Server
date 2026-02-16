namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string MAINTAINABILITY_PROMPT =
        """
        # MAINTAINABILITY AND HALSTEAD VOLUME METRICS
        
        The properties MaintainabilityIndex (byte?) and HalsteadVolume (uint?) are defined on ICodeContainer elements.
        The ICodeContainer interface is implemented by ICodeBase, IAssembly, INamespace, IType, and IMethod.
        
        These properties return null for abstract code elements and code elements defined in referenced or third-party assemblies.
        
        ## HALSTEAD VOLUME (HV)
        
        ### DEFINITION:
        Halstead Volume quantifies the information density and cognitive complexity of code by analyzing:
        - Operators: +, -, *, /, if, while, etc.
        - Operands: Variables, constants, literals
        
        Higher values indicate more information to process and understand.
        
        ### INTERPRETATION BY ELEMENT TYPE:
        
        FOR METHODS:
        - < 150: Simple, easy to understand
        - 150 - 400: Moderate complexity, acceptable
        - 400 - 800: Complex, consider simplification
        - > 800: Very high cognitive load, should be split or simplified
        
        Exception: Auto-generated methods (e.g., by tools, scaffolding) can have high HV
        
        FOR TYPES:
        - < 800: Simple, well-focused type
        - 800 - 3,000: Moderate complexity, acceptable
        - 3,000 - 8,000: Complex, may benefit from refactoring
        - > 8,000: Very complex, likely violates Single Responsibility Principle
        
        High type HV often indicates:
        - Too many responsibilities (SRP violation)
        - Unrelated logic grouped together
        - God classes or utility classes with unrelated methods
        - Need to split into smaller, cohesive types
        
        ## MAINTAINABILITY INDEX (MI)
        
        ### DEFINITION:
        The Maintainability Index is a Microsoft-defined composite metric that estimates how easy code is to maintain. It combines:
        - Halstead Volume (information density)
        - Cyclomatic Complexity (control flow complexity)
        - Lines of Code (size)
        
        Formula produces a value from 0 to 100, where:
        - Higher values = More maintainable
        - Lower values = Harder to maintain
        
        ### INTERPRETATION SCALE:
        
        - 85 - 100: Excellent maintainability, easy to work with
        - 65 - 85: Good maintainability, acceptable
        - 40 - 65: Moderate maintainability, watch for degradation
        - 0 - 40: Poor maintainability, refactoring recommended
        
        Exception: Auto-generated code (by tools, scaffolding, ORMs) can have low MI
        
        ### USAGE PATTERNS:
        
        ```csharp
        //<Name>Find hard-to-maintain methods</Name>
        from m in JustMyCode.Methods
        where m.MaintainabilityIndex < 50
        where !m.IsGeneratedByCompiler
        orderby m.MaintainabilityIndex ascending
        select new {
            m,
            MI = m.MaintainabilityIndex,
            Complexity = m.CyclomaticComplexity,
            Volume = m.HalsteadVolume,
            Lines = m.NbLinesOfCode,
            Status = m.MaintainabilityIndex < 30 ? "Critical" : "Poor"
        }
        ```
        
        ## BEST PRACTICES:
        
        1. Always handle null coverage values using:
           - Null checks: where m.MaintainabilityIndex != null
           - Null coalescing: m.HalsteadVolume ?? 0
        
        2. Exclude generated code from coverage analysis:
           - where !m.IsGeneratedByCompiler (skip generated code)
           - use code base view JustMyCode
        
        3. Combine with other metrics:
           Use MI and HV alongside:
           - CyclomaticComplexity (control flow)
           - NbLinesOfCode (size)
           - PercentageCoverage (testing)
        """;
}
