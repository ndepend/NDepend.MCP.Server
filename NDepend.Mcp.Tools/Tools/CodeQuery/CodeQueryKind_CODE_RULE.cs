namespace NDepend.Mcp.Tools.CodeQuery; 
internal partial class CodeQueryKind {
    internal const string CODE_RULE_PROMPT =
        """
        OBJECTIVE:
        Generate an NDepend code rule that identifies code elements violating specific quality standards or best practices.
        
        WHAT IS A CODE RULE:
        A code rule is a C# LINQ query that finds problematic code elements. Each returned element represents a potential issue that developers should review and fix.
        
        MANDATORY STRUCTURE:
        
        ```csharp
        // <Name>Descriptive Rule Name</Name>
        // <Id>ND#### (use unique 4-digit number)</Id>
        warnif count > 0   // ** IMPORTANT** 'warnif count > 0' is a code query prefix that transforms it into a code rule.
        from codeElement in Domain
        where codeElement.Conditions_That_Identify_Issues
        let debt = ... // OPTIONAL: calculate remediation time in minutes
        select new { 
            codeElement, 
            PropertyName1 = value1,  // Additional data to display
            PropertyName2 = value2,  // Additional data to display
            Debt = debt.ToMinutes().ToDebt()  // OPTIONAL: only if debt was calculated
        }
        // <Expl>Formatted explanation referencing {0}, {1}, etc.</Expl>
        // <Desc>Why this pattern is problematic</Desc>
        // <HowToFix>Step-by-step remediation guidance</HowToFix>
        ```
        
        CRITICAL REQUIREMENTS:
        
        1. MUST start with: warnif count > 0
           - This triggers a warning when violations are found
           - Never omit this line
        
        2. WHERE clause identifies violations:
           - Use conditions that return TRUE for problematic code
           - Example: where m.CyclomaticComplexity > 20
           - Example: where !m.Visibility != m.OptimalVisibility
        
        3. SELECT creates result with issue details:
           - MUST include: codeElement (the violating element)
           - SHOULD include: relevant metrics/properties/dependencies as named fields
           - MAY include: Debt = debt.ToMinutes().ToDebt() if technical debt applies
        
        4. METADATA SECTIONS IN COMMENT (all required):
        
           <Name>: Short, descriptive title (e.g., "Methods too complex")
           
           <Id>: Unique identifier ND#### (e.g., ND4501, ND4003, MUST be higher than 4000 to not collide with standard rules)
           
           <Expl>: User-facing explanation with placeholders
           - Format string using {0}, {1}, {2} for selected properties
           - {0} always refers to codeElement.ToString()
           - {1}, {2}, etc. map to additional selected properties in order
           - Should explain concisely WHY this specific instance is an issue
           - Example: "The method *{0}* has {1} lines of code and complexity of {2}, making it hard to maintain."
           
           <Desc>: General description of the rule
           - Explains the principle or best practice
           - States why this pattern is problematic
           - No placeholders, just static explanation
           
           <HowToFix>: Actionable remediation steps
           - Concrete suggestions to resolve the issue
           - Can include refactoring techniques
           - No placeholders, just static fix guidelines
           - Example: "Extract complex logic into smaller methods" or "Use dependency injection instead"
        
        TECHNICAL DEBT (OPTIONAL):
        If the issue has quantifiable remediation time:
        
        let debt = (expression_in_minutes)
        select new { 
            codeElement, 
            ...,
            Debt = debt.ToMinutes().ToDebt()
        }
        
        Example debt calculations:
        - let debt = 5.ToMinutes().ToDebt()  // Fixed 5 minutes per violation
        - let debt = (m.NbLinesOfCode*10).ToSeconds().ToDebt()  // 10 seconds per lines of code
        - let debt = (m.CyclomaticComplexity * 3).ToMinutes().ToDebt()  // 3 min per complexity point
        
        COMPLETE EXAMPLE:
        
        ```csharp
        // <Name>Methods too complex</Name>
        // <Id>ND4502</Id>
        warnif count > 0
        from m in JustMyCode.Methods
        where m.CyclomaticComplexity > 15
        let debt = (m.CyclomaticComplexity / 2).ToMinutes().ToDebt()
        select new { 
            m, 
            Complexity = m.CyclomaticComplexity,
            LinesOfCode = m.NbLinesOfCode,
            Debt = debt
        }
        // <Expl>
        // The method *{0}* has cyclomatic complexity of {1} (threshold: 15). 
        // High complexity with {2} lines makes this method difficult to test and maintain.
        // </Expl>
        // <Desc>
        // Methods with high cyclomatic complexity contain too many execution paths,
        // making them difficult to understand, test, and maintain. 
        // Complexity above 15 indicates the method should be refactored.
        // </Desc>
        // <HowToFix>
        // Split this method into smaller methods, each with a single responsibility.
        // Extract conditional logic into separate well-named methods.
        // Consider using polymorphism instead of complex switch statements.
        // </HowToFix>
        ```
        
        COMMON PATTERNS:
        
        Pattern: Code too large
        where codeElement.NbLinesOfCode > threshold
        
        Pattern: Poor test coverage
        where codeElement.PercentageCoverage < threshold
        
        Pattern: Visibility violations
        where !codeElement.IsPublic && codeElement.IsUsedOutsideAssembly
        
        Pattern: Naming violations
        where !codeElement.NameLike(@"^I[A-Z]") // Interfaces should start with I
        
        Pattern: High coupling
        where codeElement.TypesUsed.Count() > threshold
        
        Pattern: Missing attributes
        where !codeElement.HasAttribute("System.SerializableAttribute")
        """;


}
