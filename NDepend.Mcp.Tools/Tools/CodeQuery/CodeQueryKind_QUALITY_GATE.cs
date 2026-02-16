namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryKind {
    internal const string QUALITY_GATE_PROMPT =
        """
        OBJECTIVE: WRITE A QUALITY GATE CODE QUERY
        Generate a NDepend quality gate code query that enforces pass/fail criteria to prevent code quality regression.
        
        WHAT IS A QUALITY GATE:
        A CQLinq query with warnif/failif directives that blocks builds or deployments when quality thresholds are violated.
        
        MANDATORY STRUCTURE:
        
        The comment header *MUST* include the <QualityGate> XML tag with Name and Unit attributes.
        
        ```csharp
        // <QualityGate Name="$Quality Gate  Name$" Unit="$unit$"/>
        failif count > Y$OPTIONAL unit$    // Failure threshold (breaks build)
        warnif count > X$OPTIONAL unit$    // Warning threshold
        
        $Here, CQLinq code query that returns a scalar, or that match a list of tuples in which case the scalar is the list count$
        
        //<Desc>
        // $OPTIONAL description explaining what the Quality Gate measures and any important details about its calculation.$
        //</Desc>
        ```
        
        KEY RULES:
        - First line MUST be `// <QualityGate Name="..." Unit="..."/>`
        - Unit should be singular (use "method" not "methods")
        - Query must return ONE number (scalar or list count)
        - Description is optional but recommended
        
        COMPLETE EXAMPLE:
        
        ```csharp
        // <QualityGate Name="Percentage Coverage" Unit="%" />
        failif value < 70%
        warnif value < 80%
        codeBase.PercentageCoverage
        
        //<Desc>
        // Code coverage is a measure used to describe the degree to which the source code of a program 
        // is tested by a particular test suite. A program with high code coverage, measured as a percentage, 
        // has had more of its source code executed during testing which suggests it has a lower chance of 
        // containing undetected software bugs compared to a program with low code coverage.
        //
        // Code coverage is certainly the most important quality code metric. But coverage is not enough
        // the team needs to ensure that results are checked at test-time. These checks can be done both 
        // in test code, and in application code through assertions. The important part is that a test
        // must fail explicitly when a check gets unvalidated during the test execution.
        //
        // This quality gate defines a warn threshold (80%) and a fail threshold (70%). These are 
        // indicative thresholds and in practice the more the better. To achieve high coverage and 
        // low risk, make sure that new and refactored classes gets 100% covered by tests and that
        // the application and test code contains as many checks/assertions as possible.
        //</Desc>
        ```
        
        ```csharp
        // <QualityGate Name="New Blocker / Critical / High Issues" Unit="issues" />
        failif count > 0 issues
        from i in Issues
        where i.Severity.EqualsAny(Severity.Blocker, Severity.Critical, Severity.High) &&  
              // Count both the new issues and the issues that became at least Critical
              (i.WasAdded() || i.OlderVersion().Severity < Severity.High)
        select new { i, i.Severity, i.Debt, i.AnnualInterest }
        
        
        //<Desc>
        // An issue with the severity **Blocker** cannot move to production, it must be fixed.
        //
        // An issue with a severity level **Critical** shouldn't move to production. 
        // It still can for business imperative needs purposes, but at worth it must be fixed 
        // during the next iterations. 
        //
        // An issue with a severity level **High** should be fixed quickly, but can wait until 
        // the next scheduled interval.
        //
        // The severity of an issue is either defined explicitly in the rule source code,
        // either inferred from the issue *annual interest* and thresholds defined in the 
        // NDepend Project Properties > Issue and Debt.
        //</Desc>
        ```
        
        ```csharp
        // <QualityGate Name="Percentage Debt" Unit="%" />
        failif value > 30%
        warnif value > 20%
        let timeToDev = codeBase.EffortToDevelop()
        let debt = Issues.Sum(i => i.Debt)
        select 100d * debt.ToManDay() / timeToDev.ToManDay()
        
        // <Desc>
        // % Debt total is defined as a percentage on:
        //
        // • the estimated total effort to develop the code base
        //
        // • and the the estimated total time to fix all issues (the Debt)
        //
        // Estimated total effort to develop the code base is inferred from 
        // # lines of code of the code base and from the 
        // *Estimated number of man-day to develop 1000 logical lines of code*
        // setting found in NDepend Project Properties > Issue and Debt.
        //
        // Debt documentation: https://www.ndepend.com/docs/technical-debt#Debt
        //
        // This quality gates fails if the estimated debt is more than 30%
        // of the estimated effort to develop the code base, and warns if the 
        // estimated debt is more than 20% of the estimated effort to develop 
        // the code base
        // </Desc>
        ```
        
        ```csharp
        // <QualityGate Name="Debt Rating per Namespace" Unit="namespaces" />
        failif count > 0 namespaces
        
        from n in Application.Namespaces
        where n.DebtRating() != null &&
              n.DebtRating().Value.EqualsAny(DebtRating.E, DebtRating.D)
        select new { 
           n, 
           debtRating = n.DebtRating(),
           debtRatio = n.DebtRatio(),  // % of debt from which DebtRating is inferred
           devTimeInManDay = n.EffortToDevelop().ToDebt(), 
           debtInManDay = n.AllDebt(),
           issues = n.AllIssues() 
        }
        
        // <Desc>
        // Forbid namespaces with a poor Debt Rating equals to **E** or **D**.
        //
        // The **Debt Rating** for a code element is estimated by the value of the **Debt Ratio**
        // and from the various rating thresholds defined in this project *Debt Settings*. 
        //
        // The **Debt Ratio** of a code element is a percentage of **Debt Amount** (in floating man-days) 
        // compared to the **estimated effort to develop the code element** (also in floating man-days).
        //
        // The **estimated effort to develop the code element** is inferred from the code elements
        // number of lines of code, and from the project *Debt Settings* parameters 
        // *estimated number of man-days to develop 1000* **logical lines of code**.
        //
        // The **logical lines of code** corresponds to the number of debug breakpoints in a method
        // and doesn't depend on code formatting nor comments.
        //
        // The Quality Gate can be modified to match assemblies, types or methods
        // with a poor Debt Rating, instead of matching namespaces.
        // </Desc>
        ```
        
        # QUERYING ISSUE AND RULE FROM A QUALITY GATE
        
        Often, Quality Gates need to examine the set of issues and their associated rules in order to enforce quality standards.
        
        Below is a prompt designed to guide the creation of CQLinq queries that retrieve issues along with their corresponding rules.
        
        **IMPORTANT**: The prompt also describes how to query Quality Gate status, but a Quality Gate cannot directly query the status of other Quality Gates—so that part should be ignored.
        
        """ + QUERYING_ISSUE_AND_RULE_PROMPT;
}

