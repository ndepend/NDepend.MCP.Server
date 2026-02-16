namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryKind {
    internal const string TREND_METRIC_PROMPT =
        """
        OBJECTIVE:
        
        Generate a NDepend trend metric code query that tracks a scalar value over time and plots charts.
        
        WHAT IS A TREND METRIC:
        
        A special CQLinq query that returns a single numeric value to track code evolution.
        
        MANDATORY STRUCTURE:
        
        The comment header *MUST* include the <TrendMetric> XML tag with Name and Unit attributes.
        
        ```csharp
        // <TrendMetric Name="$Trend Metric Name$" Unit="$unit$"/>
        $CQLinq code query that returns a scalar, or that match a list of tuples in which case the scalar is the list count$
        
        //<Desc>
        // $OPTIONAL description explaining what the trend metric measures and any important details about its calculation.$
        //</Desc>
        ```
        
        KEY RULES:
        - First line MUST be `// <TrendMetric Name="..." Unit="..."/>`
        - Unit should be singular (use "method" not "methods")
        - Query must return ONE number (scalar or list count)
        - Description is optional but recommended
        
        COMPLETE EXAMPLE:
        
        ```csharp
        // <TrendMetric Name="# New Issues since Baseline" Unit="issues"/>
        from issue in Issues 
        where issue.WasAdded()
        select new { issue, issue.Debt, issue.AnnualInterest, issue.Severity }
        ```
        
        ```csharp
        // <TrendMetric Name="# Rules Violated" Unit="rules"/>
        from rule in Rules
        where rule.IsViolated()
        select new { 
           rule, 
           issues = rule.Issues(), 
           debt = rule.Debt(), 
           annualInterest = rule.AnnualInterest(),
           maxSeverity = rule.IsViolated() && rule.Issues().Any() ? 
                         (Severity?)rule.Issues().Max(i => i.Severity) : null
        }
        
        //<Desc>
        // This trend metric counts the number of active rules that are violated.
        // This count includes critical and non critical rules.
        //
        // When no baseline is available, rules that rely on diff are not counted.
        // If you observe that this count slightly decreases with no apparent reason,
        // the reason is certainly that rules that rely on diff are not counted
        // because the baseline is not defined.
        //</Desc>
        ```
        
        ```csharp
        // <TrendMetric Name="Percentage Debt (Metric)" Unit="%" />
        let timeToDev = codeBase.EffortToDevelop()
        let debt = Issues.Sum(i => i.Debt)
        select 100d * debt.ToManDay() / timeToDev.ToManDay()
        
        // <Desc>
        // This Trend Metric name is suffixed with (Metric)
        // to avoid query name collision with the Quality Gate with same name.
        //
        // Infer a percentage from:
        //
        // • the estimated total time to develop the code base
        //
        // • and the the estimated total time to fix all issues (the Debt).
        //
        // Estimated total time to develop the code base is inferred from 
        // # lines of code of the code base and from the 
        // *Estimated number of man-day to develop 1000 logical lines of code*
        // setting found in NDepend Project Properties > Issue and Debt.
        //
        // Debt documentation: https://www.ndepend.com/docs/technical-debt#Debt
        // </Desc>
        ```
        
        ```csharp
        // <TrendMetric Name="Percentage of Comments" Unit="%" />  
        codeBase.PercentageComment
        
        //<Desc>
        // This trend metric returns the percentage of comment
        // compared to the number of **logical**lines of code.
        //
        // So far commenting information is only extracted from C# source code
        // and VB.NET support is planned.
        //</Desc>
        ```
        
        ## TREND METRICS ABOUT ISSUES, RULES, AND QUALITY GATES:
        
        Below is a prompt designed to guide the creation of CQLinq queries that retrieve issues along with their corresponding rules.
        
        """ + QUERYING_ISSUE_AND_RULE_PROMPT;
}

