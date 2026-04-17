namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryKind {
    internal const string QUALITY_GATE_PROMPT =
        """
        A quality gate is a CQLinq query with `warnif`/`failif` directives that enforces pass/warn/fail thresholds.

        Mandatory Structure:

        ```csharp
        // <QualityGate Name="$Name$" Unit="$unit$"/>
        failif count > Y $OPTIONAL unit$    // failure threshold (breaks build)
        warnif count > X $OPTIONAL unit$    // warning threshold
        $Here, CQLinq code query that returns a scalar, or that match a list of tuples in which case the scalar is the list count$
        //<Desc> OPTIONAL description</Desc>
        ```

        Key Rules:
        - First line MUST be `// <QualityGate Name="..." Unit="..."/>`
        - Unit should be singular ("method" not "methods")
        - Query must return ONE number (scalar or list count)

        Complete Example:

        ```csharp
        // <QualityGate Name="Percentage Coverage" Unit="%" />
        failif value < 70%
        warnif value < 80%
        codeBase.PercentageCoverage
        //<Desc> Percentage of code executed by tests. High coverage reduces undetected bug risk.</Desc>
        ```

        ```csharp
        // <QualityGate Name="New Blocker / Critical / High Issues" Unit="issues" />
        failif count > 0 issues
        from i in Issues
        where i.Severity.EqualsAny(Severity.Blocker, Severity.Critical, Severity.High) &&
              // Count both the new issues and the issues that became at least Critical
              (i.WasAdded() || i.OlderVersion().Severity < Severity.High)
        select new { i, i.Severity, i.Debt, i.AnnualInterest }
        //<Desc> Blockers must be fixed before release; Critical/High should be resolved promptly.</Desc>
        ```

        ```csharp
        // <QualityGate Name="Percentage Debt" Unit="%" />
        failif value > 30%
        warnif value > 20%
        let timeToDev = codeBase.EffortToDevelop()
        let debt = Issues.Sum(i => i.Debt)
        select 100d * debt.ToManDay() / timeToDev.ToManDay()
        //<Desc> Ratio of total issue debt to estimated development effort. Fails at 30%, warns at 20%.</Desc>
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
        //<Desc> Forbids namespaces rated D or E. DebtRatio = debt / estimated-dev-effort (%). Can be adapted to types or methods.</Desc>
        ```

        # Querying Issue and Rule from a Quality Gate

        Quality Gates can query Issues, Rules, and their statuses. Note: a Quality Gate cannot query the status of other Quality Gates.

        """ + QUERYING_ISSUE_AND_RULE_PROMPT;
}

