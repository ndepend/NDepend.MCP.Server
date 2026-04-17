namespace NDepend.Mcp.Tools.CodeQuery; 
internal partial class CodeQueryKind {
    internal const string CODE_RULE_PROMPT =
        """
        What Is a Code Rule:
        
        A code rule is a C# LINQ query prefixed with `warnif count > 0` that finds problematic code elements.
        Each returned element represents a potential issue that developers should review and fix.

        Mandatory Structure:

        ```csharp
        // <Name>Rule Name</Name>
        // <Id>ND#### (must be > ND4000 to avoid collision with built-in rules)</Id>
        warnif count > 0
        from codeElement in Domain
        where codeElement.Condition (that identifies issues)
        let debt = (minutes_expr).ToMinutes().ToDebt()  // OPTIONAL
        select new { codeElement, Prop = value, Debt = debt }
        // <Expl>The {0}=element, {1},{2}...=other selected props in order. Explain why this instance is an issue.</Expl>
        // <Desc>Why this pattern is problematic (no placeholders).</Desc>
        // <HowToFix>Steps to resolve (no placeholders).</HowToFix>
        ```

        Rules:
        - `warnif count > 0` is mandatory — never omit
        - WHERE: conditions returning TRUE identify violations  ex: m.CyclomaticComplexity > 20, m.Visibility != m.OptimalVisibility
        - SELECT: Tuple must starts with codeElement; add metrics/deps/debt as named fields
        - `<Id>`: unique ND#### greater than ND4000
        - `<Expl>`: {0}=codeElement.ToString(), {1},{2}...=additional selected props in order
        - `<Desc>`: General description of the rule
        - `<HowToFix>`: Actionable remediation steps

        Example debt calculations:
        - let debt = 5.ToMinutes().ToDebt()  // Fixed 5 minutes per violation
        - let debt = (m.NbLinesOfCode*10).ToSeconds().ToDebt()  // 10 seconds per lines of code
        - let debt = (m.CyclomaticComplexity * 3).ToMinutes().ToDebt()  // 3 min per complexity point
        
        Complete Example:

        ```csharp
        // <Name>Methods too complex</Name>
        // <Id>ND4502</Id>
        warnif count > 0
        from m in JustMyCode.Methods
        where m.CyclomaticComplexity > 15
        let debt = (m.CyclomaticComplexity / 2).ToMinutes().ToDebt()
        select new { m, Complexity = m.CyclomaticComplexity, LinesOfCode = m.NbLinesOfCode, Debt = debt }
        // <Expl>Method *{0}* has cyclomatic complexity {1} (threshold: 15) with {2} lines — hard to maintain.</Expl>
        // <Desc>High cyclomatic complexity means too many execution paths, making code hard to test and maintain.</Desc>
        // <HowToFix>Split into smaller methods. Extract conditional logic. Prefer polymorphism over switch.</HowToFix>
        ```

        Common Patterns:
        - Code too large:    `where codeElement.NbLinesOfCode > threshold`
        - Poor coverage:     `where codeElement.PercentageCoverage < threshold`
        - Visibility issues: `where m.Visibility != m.OptimalVisibility`
        - Naming violation:  `where !codeElement.NameLike(@"^I[A-Z]")`
        - High coupling:     `where codeElement.TypesUsed.Count() > threshold`
        - Missing attribute: `where !codeElement.HasAttribute("System.SerializableAttribute")`
        """;


}
