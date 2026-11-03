namespace NDepend.Mcp.Tools.CodeQuery;

public partial class CodeQueryKind {
    internal const string TREND_METRIC_PROMPT =
        """
        A trend metric is a CQLinq query returning a single numeric value tracked over time to plot charts.

        Mandatory Structure:

        ```csharp
        // <TrendMetric Name="$Name$" Unit="$unit$"/>
        $scalar query, or list query (scalar = count)$
        //<Desc> OPTIONAL description</Desc>
        ```

        Key Rules:
        - First line MUST be `// <TrendMetric Name="..." Unit="..."/>`
        - Unit should be singular ("method" not "methods")
        - Query must return ONE number (scalar or list count)

        Complete Example:

        ```csharp
        // <TrendMetric Name="# New Issues since Baseline" Unit="issues"/>
        from issue in Issues where issue.WasAdded()
        select new { issue, issue.Debt, issue.AnnualInterest, issue.Severity }
        ```

        ```csharp
        // <TrendMetric Name="# Rules Violated" Unit="rules"/>
        from rule in Rules where rule.IsViolated()
        select new { rule, issues = rule.Issues(), debt = rule.Debt(), annualInterest = rule.AnnualInterest(),
                     maxSeverity = rule.IsViolated() && rule.Issues().Any() ? (Severity?)rule.Issues().Max(i => i.Severity) : null }
        //<Desc> Counts active violated rules. Rules relying on diff are excluded when no baseline is defined.</Desc>
        ```

        ```csharp
        // <TrendMetric Name="Percentage Debt (Metric)" Unit="%" />
        let timeToDev = codeBase.EffortToDevelop()
        let debt = Issues.Sum(i => i.Debt)
        select 100d * debt.ToManDay() / timeToDev.ToManDay()
        //<Desc> Suffix "(Metric)" avoids name collision with the Quality Gate of the same name.</Desc>
        ```

        ```csharp
        // <TrendMetric Name="Percentage of Comments" Unit="%" />
        // Application code only: test code (the TestCode domain) is not counted
        let loc      = (double)Application.Assemblies.Sum(a => a.NbLinesOfCode ?? 0)
        let comments = (double)Application.Assemblies.Sum(a => a.NbLinesOfComment ?? 0)
        select (loc + comments) == 0d ? (double?)null : 100d * comments / (loc + comments)
        ```

        ```csharp
        // <TrendMetric Name="# Tests" Unit="Tests" />
        from m in TestCode.Methods
        where m.IsTestMethod
        select new { m, m.ParentType, m.NbLinesOfCode }
        //<Desc> The test methods declared in the test assemblies.</Desc>
        ```


        ## Trend Metrics About Issues, Rules, and Quality Gates:

        """ + QUERYING_ISSUE_AND_RULE_PROMPT;
}

