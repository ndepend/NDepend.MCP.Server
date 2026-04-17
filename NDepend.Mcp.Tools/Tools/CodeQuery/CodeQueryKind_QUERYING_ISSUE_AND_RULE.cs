namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryKind {
    internal const string QUERYING_ISSUE_AND_RULE_PROMPT =
        """
        # Objective: Querying Issues, Rules, and Quality Gates

        Leverage CQLinq to query issue sets, rule sets, and their statuses
        
        ## Queryable Domains

        ```csharp
        IEnumerable<IIssue>       Issues
        IEnumerable<IIssue>       IssuesInBaseline
        IEnumerable<IRule>        Rules
        IEnumerable<IQualityGate> QualityGates
        ```
        First `select` column must be typed WITH IIssue, IRule, or IQualityGate.
        NOTE: A code rule cannot query these domains.

        ```csharp
        from i in Issues
        where i.$Conditions
        select new { i, i.Property1, i.Property2, ... }
        ```

        ## Interfaces properties

        ```csharp
        // IIssue
        - IRule         Rule
        - ICodeElement  CodeElement
        - ICodeElement  CodeElementInBaseline  // null if not in baseline
        - bool          CodeElementInBaselineWasRemoved
        - Debt          Debt  // estimated fix effort
        - AnnualInterest AnnualInterest  // annual cost if left unfixed
        - Severity      Severity  // Blocker|Critical|High|Medium|Low
        - bool         IsSuppressed
        - IAttributeTag SuppressAttributeTag
        }
        
        // IRule
        - string       Name
        - string       Category
        - string       Id // e.g. "ND1000"
        - string       ExplicitId
        - RuleProvider Provider // CodeQueryRule | Roslyn | ReSharper
        - bool         IsCritical
        }

        // IQualityGate
        - string            Name
        - string            Unit
        - QualityGateStatus Status // Pass | Warn | Fail
        - double?           Value
        - string            ValueString
        - bool              MoreIsBad
        - double            FailThreshold
        - double?           WarnThreshold
        }
        ```

        ## Useful Extension Methods:

        ```csharp
        // On ICodeElement
        bool HasIssues(this ICodeElement codeElement)
        IEnumerable<IIssue> Issues(this ICodeElement codeElement)
        bool HasDebt(this ICodeElement codeElement)
        Debt Debt(this ICodeElement codeElement)
        
        // All** : Applies to 'codeElement' and all its descendants (RecursiveChildren if parent).
        IEnumerable<IIssue> AllIssues(this ICodeElement codeElement)
        Debt AllDebt(this ICodeElement codeElement)
        
        // Debt rating of 'codeElement' (A=best, E=worst).
        DebtRating? DebtRating(this ICodeElement codeElement)
        
        // Estimated effort to reach 'ratingToReach' from current debt (AllDebt).
        Debt? CostToReachRating(this ICodeElement codeElement, DebtRating ratingToReach)
        
        // Estimated effort to reach the next better rating (from AllDebt).
        Debt? CostToReachBetterRating(this ICodeElement codeElement)
        
        // Technical debt ratio (%) vs. rewrite cost (based on AllDebt).
        double? DebtRatio(this ICodeElement codeElement)

        // On IRule
        IEnumerable<IIssue>   Issues(this IRule r)
        bool                  IsViolated(this IRule r)
        Debt                  Debt(this IRule r)
        bool                  IsNotViolatedAnymore(this IRule r)
        bool                  WasNotViolatedAndIsViolated(this IRule r)
        IEnumerable<IIssue>   IssuesAdded(this IRule r)
        IEnumerable<IIssue>   IssuesFixed(this IRule r)
        Debt                  DebtDiff(this IRule r)

        // On IQualityGate
        // Value difference between NewerVersion(qualityGate).Value and OlderVersion(qualityGate).Value.
        double? ValueDiff(this IQualityGate qualityGate)
        
        // Common extension methods on IIssue IRule and IQualityGate (represented by I*)
        
        // Get the equivalent item in IIssuesSetDiff.NewerIssuesSet, or null
        I* NewerVersion(this I* qualityGate)
        
        // Get the equivalent item in IIssuesSetDiff.OlderIssuesSet, or null
        I* OlderVersion(this I* qualityGate)
        bool IsInNewerIssuesSet(this I* qualityGate)
        bool IsInOlderIssuesSet(this I* qualityGate)
        bool IsPresentInBothIssuesSet(this I* qualityGate)
        ```
        
        ## Complete Example:

        ### Filtering Issues
        
        ```csharp
        // Get all issues
        from issue in Issues
        select issue
        
        // Get all issues indexed by code element
        from issue in Issues
        select new { issue.CodeElement, issue }
        
        // Get all issues grouped by code element
        from c in CodeElements
        where c.HasIssue()
        select new { c, Issues = c.Issues() }
        
        // Issues by severity
        from issue in Issues
        where issue.Severity == Severity.Critical
        select issue

        // Issues for specific rule
        from issue in Issues
        where issue.Rule.Name == "Avoid types too big"
        select new { issue, issue.Debt, issue.Severity }

        // Issues in specific namespace
        from n in Application.Namespaces.WithNameLike("^MyApp.Core")
        from e in n.MeAndRecursiveChildren
        where e.HasIssue()
        from i in e.Issues()
        select i

        // New since baseline
        from issue in Issues where issue.WasAdded() select issue

        // Fixed since baseline
        from issue in IssuesInBaseline where issue.WasFixed() select issue
        ```

        ### Listing Critical Rules Violated

        ```csharp
        // <QualityGate Name="Critical Rules Violated" Unit="rules" />
        failif count > 0 rules
        from r in Rules where r.IsCritical && r.IsViolated()
        select new { r, issues = r.Issues() }
        ```

        ### Listing Quality Gates Status Evolution Since Baseline

        ```csharp
        // <Name>Quality Gates Evolution</Name>
        from qg in QualityGates
        let qgBaseline = qg.OlderVersion()
        let relyOnDiff = qgBaseline == null
        let evolution = (relyOnDiff || qgBaseline.Value == null) ? TrendIcon.None :
                        qg.ValueDiff() == 0d ? TrendIcon.Constant :
                        (qg.ValueDiff() > 0
                          ? (qg.MoreIsBad ? TrendIcon.RedUp   : TrendIcon.GreenUp)
                          : (qg.MoreIsBad ? TrendIcon.GreenDown : TrendIcon.RedDown))
        select new { qg, Status = qg.Status, Evolution = evolution,
                     BaselineValue = relyOnDiff ? " " : qgBaseline.ValueString, Value = qg.ValueString }
        ```
        """;
}

