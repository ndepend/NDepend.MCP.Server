namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryKind {
    internal const string QUERYING_ISSUE_AND_RULE_PROMPT =
        """
        # OBJECTIVE: QUERYING ISSUES, RULES, AND QUALITY GATES
        
        Leverage CQLinq to query issue sets, rule sets, and their statuses—including NDepend CQLinq Rules, Roslyn Analyzers, ReSharper Code Inspections—as well as quality gate statuses.
        This guide presents patterns and examples to help you work effectively with issues, rules, and quality gates.
        
        ## QUERYABLE DOMAINS
        
        CQLinq exposes the following queryable domains:
        
        ```csharp
        IEnumerable<IIssue> Issues
        IEnumerable<IIssue> IssuesInBaseline
        IEnumerable<IRule> Rules
        IEnumerable<IQualityGate> QualityGates
        ```
        
        A typical query follows this structure:
        
        ```csharp
        from i in Issues
        where i.$Conditions
        select new { i, i.Property1, i.Property2, ... }
        ```
        
        In this context, the first column of the select clause must be typed as IIssue, IRule, or IQualityGate, depending on the domain being queried.
        
        NOTE: Obviously a code rule cannot query any of these domains since rules define the issues.
        
        ## The IIssue INTERFACE API
        
        ```csharp
        // An instance of this class represents an issue detected by an NDepend CQLinq rule, a Roslyn Analyzers or a ReSharper Code Inspections.
        
        public interface IIssue {
           // Rule that produced this issue.
           IRule Rule { get; }
        
           // Code element in the current code base affected by the issue.
           ICodeElement CodeElement { get; }
        
           // Matching code element in the baseline snapshot, if any.
           ICodeElement CodeElementInBaseline { get; }
        
           // True if the baseline code element was removed in the current snapshot.
           bool CodeElementInBaselineWasRemoved { get; }
        
           // Estimated effort required to fix the issue.
           Debt Debt { get; }
        
           // Estimated annual cost of leaving the issue unfixed.
           AnnualInterest AnnualInterest { get; }
        
           // Severity inferred from debt and annual interest.
           // Valid values are `Blocker`, `Critical`, `High`, `Medium`, `Low`.
           Severity Severity { get; }
        
           // Time when cost-to-fix equals cost-to-not-fix.
           TimeSpan BreakingPoint { get; }
        
           // Annual interest expressed as a percentage of the debt.
           double AnnualInterestPercent { get; }
        
           // Indicates whether the issue references a query record.
           IssueReferenceRecord ReferenceRecord { get; }
        
           // Query execution record associated with this issue, if available.
           IRecord Record { get; }
        
           // Column names corresponding to the issue query record.
           IReadOnlyList<string> ColumnsNames { get; }
        
           // Indicates whether a source file declaration is available.
           bool SourceFileDeclAvailable { get; }
        
           // Source file declaration of the issue, if available.
           ISourceDecl SourceDecl { get; }
        
           // Indicates whether the issue has been suppressed.
           bool IsSuppressed { get; }
        
           // SuppressMessage attribute that suppressed the issue, if any.
           IAttributeTag SuppressAttributeTag { get; }
        }
        ```
        
        ## THE IRule INTERFACE API
        
        ```csharp
        // Represents a rule from which IIssue are inferred.
        // The rule can be an NDepend CQLinq rule, a Roslyn Analyzers or a ReSharper Code Inspections.
        public interface IRule {
           // Name of the rule.
           string Name { get; }
        
           // Category or group the rule belongs to.
           string Category { get; }
        
           // Short rule identifier (e.g. "ND1000"), if defined.
           string Id { get; }
        
           // Explicit rule identifier without the numeric prefix.
           string ExplicitId { get; }
        
           // Provider that defines the rule.
           // Valid values are `CodeQueryRule` (NDepend CQLinq Rule), `Roslyn` or `ReSharper`.
           RuleProvider Provider { get; }
        
           // Indicates whether the rule is considered critical.
           bool IsCritical { get; }
        }
        ```
        
        ## THE IQualityGate INTERFACE API
        
        ```csharp
        // Represents a quality gate with its status.
        public interface IQualityGate {
           // Name of the quality gate.
           string Name { get; }
        
           // Unit of measurement for the quality gate value.
           string Unit { get; }
        
           // Code query string defining the quality gate.
           string QueryString { get; }
        
           // Current status: `Pass`, `Warn`, or `Fail`.
           QualityGateStatus Status { get; }
        
           // True if the quality gate passes (neither warn nor fail).
           bool Pass { get; }
        
           // True if the quality gate meets its warn conditions.
           bool Warn { get; }
        
           // True if the quality gate meets its fail conditions.
           bool Fail { get; }
        
           // Numeric value of the quality gate; null always passes.
           double? Value { get; }
        
           // Value formatted as string with unit.
           string ValueString { get; }
        
           // True if higher values are considered worse.
           bool MoreIsBad { get; }
        
           // Fail threshold value.
           double FailThreshold { get; }
        
           // Fail threshold formatted as string (no unit).
           string FailThresholdString { get; }
        
           // Warn threshold value, or null if not defined.
           double? WarnThreshold { get; }
        }
        ```
        
        ## USEFUL EXTENSION METHODS:
        
        ### Extension Methods on ICodeElement:
        
        ```csharp
        // Gets a value indicating whether the 'codeElement' has issues or not.
        bool HasIssues(this ICodeElement codeElement)
        
        // Gets all issues relative to 'codeElement'.
        IEnumerable<IIssue> Issues(this ICodeElement codeElement)
        
        // Gets a value indicating whether the 'codeElement' has debt or not.
        bool HasDebt(this ICodeElement codeElement)
        
        // Gets the summed debt of all issues relative to 'codeElement'.
        Debt Debt(this ICodeElement codeElement)
        
        // Gets all issues relative to 'codeElement', and relative to all its child code elements, defined by 'codeElement.RecursiveChildren', if 'codeElement' is a ICodeElementParent.
        IEnumerable<IIssue> AllIssues(this ICodeElement codeElement)
        
        // Gets the summed debt of all issues relative to 'codeElement', and relative to all its children code elements, defined by 'codeElement.RecursiveChildren', if 'codeElement' is a ICodeElementParent.
        Debt AllDebt(this ICodeElement codeElement)
        
        // Gets the debt rating of 'codeElement' in the range DebtRating.A (very good) to DebtRating.E (very bad).
        DebtRating? DebtRating(this ICodeElement codeElement)
        
        // Returns the estimated effort to reach 'ratingToReach', based on the fact that 'codeElement' has actually a total debt estimated to AllDebt('codeElement').
        Debt? CostToReachRating(this ICodeElement codeElement, DebtRating ratingToReach)
        
        // Returns the estimated cost, to reach a better rating, based on the fact that 'codeElement' has actually a total debt estimated to AllDebt('codeElement').
        Debt? CostToReachBetterRating(this ICodeElement codeElement)
        
        // Estimate the ratio of technical debt, measured through AllDebt('codeElement'). 
        // This ratio is expressed in percentage, of estimated debt, compared to the estimated time it would take to rewrite 'codeElement' from scratch.
        double? DebtRatio(this ICodeElement codeElement)
        ```
        
        ### Extension Methods on IIssue:
        ```csharp
        // Gets the 'issue' in IIssuesSetDiff.NewerIssuesSet, or null if 'issue' has no equivalent in IIssuesSetDiff.NewerIssuesSet.
        IIssue NewerVersion(this IIssue issue)
        
        // Gets the 'issue' in IIssuesSetDiff.OlderIssuesSet, or null if 'issue' has no equivalent in IIssuesSetDiff.OlderIssuesSet.
        IIssue OlderVersion(this IIssue issue)
        
        // Gets a value that indicates if 'issue' is in IIssuesSetDiff.NewerIssuesSet.
        bool IsInNewerIssuesSet(this IIssue issue)
        
        // Gets a value that indicates if 'issue' is in IIssuesSetDiff.OlderIssuesSet.
        bool IsInOlderIssuesSet(this IIssue issue)
        ```
        
        ### Extension Methods on IRule:
        ```csharp
        // Gets all issues of 'rule'.
        IEnumerable<IIssue> Issues(this IRule rule)
        
        // Gets a value that indicates if the 'rule' is violated.
        bool IsViolated(this IRule rule)
        
        // Gets the summed debt of all issues of 'rule'.
        Debt Debt(this IRule rule)
        
        // Gets a value that indicates if 'rule' is present in both IIssuesSetDiff.NewerIssuesSet and IIssuesSetDiff.OlderIssuesSet and is violated in IIssuesSetDiff.OlderIssuesSet and not in IIssuesSetDiff.NewerIssuesSet.
        bool IsNotViolatedAnymore(this IRule rule)
        
        // Gets a value that indicates if 'rule' is present in both IIssuesSetDiff.NewerIssuesSet and IIssuesSetDiff.OlderIssuesSet and is violated in IIssuesSetDiff.NewerIssuesSet and not in IIssuesSetDiff.OlderIssuesSet.
        bool WasNotViolatedAndIsViolated(this IRule rule);
        
        // Gets all issues of 'rule' that have no equivalent in IIssuesSetDiff.OlderIssuesSet. These issues are considered as added.
        IEnumerable<IIssue> IssuesAdded(this IRule rule);
        
        // Gets all issues of 'rule' that have no equivalent in IIssuesSetDiff.NewerIssuesSet. These issues are considered as fixed.
        IEnumerable<IIssue> IssuesFixed(this IRule rule);
        
        // Gets IIssuesSetDiff.NewerIssuesSet.Debt of NewerVersion('rule') minus IIssuesSetDiff.OlderIssuesSet.Debt of OlderVersion('rule').
        Debt DebtDiff(this IRule rule);
        
        // Gets the 'rule' in IIssuesSetDiff.NewerIssuesSet, or null if 'rule' has no equivalent in IIssuesSetDiff.NewerIssuesSet.
        IRule NewerVersion(this IRule rule)
        
        // Gets the 'rule' in IIssuesSetDiff.OlderIssuesSet, or null if 'rule' has no equivalent in IIssuesSetDiff.OlderIssuesSet.
        IRule OlderVersion(this IRule rule)
        
        // Gets a value that indicates if 'rule' is in IIssuesSetDiff.NewerIssuesSet.
        bool IsInNewerIssuesSet(this IRule rule)
        
        // Gets a value that indicates if 'rule' is in IIssuesSetDiff.OlderIssuesSet.
        bool IsInOlderIssuesSet(this IRule rule)
        
        // Gets a value that indicates if 'rule' is in IIssuesSetDiff.NewerIssuesSet or IIssuesSetDiff.OlderIssuesSet and has an equivalent in the other issues-set.
        bool IsPresentInBothIssuesSet(this IRule rule)
        ```
        
        ### Extension Methods on IQualityGate:
        ```csharp
        
        // Gets the <see cref="IQualityGate.Value" /> of <see cref="NewerVersion(IQualityGate)" /> of <paramref name="qualityGate" />, 
        // minus the <see cref="IQualityGate.Value" /> of <see cref="OlderVersion(IQualityGate)" /> of <paramref name="qualityGate" />.
        double? ValueDiff(this IQualityGate qualityGate)
        // Gets the <paramref name="qualityGate" /> in <see cref="IIssuesSetDiff.NewerIssuesSet" />, or <i>null</i> if <paramref name="qualityGate" /> has no equivalent in <see cref="IIssuesSetDiff.NewerIssuesSet" />.
        IQualityGate NewerVersion(this IQualityGate qualityGate)
        
        // Gets the <paramref name="qualityGate" /> in <see cref="IIssuesSetDiff.OlderIssuesSet" />, or <i>null</i> if <paramref name="qualityGate" /> has no equivalent in <see cref="IIssuesSetDiff.OlderIssuesSet" />.
        IQualityGate OlderVersion(this IQualityGate qualityGate)
        
        // Gets a value that indicates if <paramref name="qualityGate" /> is in <see cref="IIssuesSetDiff.NewerIssuesSet" />.
        bool IsInNewerIssuesSet(this IQualityGate qualityGate)
        
        // Gets a value that indicates if <paramref name="qualityGate" /> is in <see cref="IIssuesSetDiff.OlderIssuesSet" />.
        bool IsInOlderIssuesSet(this IQualityGate qualityGate)
        
        // Gets a value that indicates if <paramref name="qualityGate" /> is in <see cref="IIssuesSetDiff.NewerIssuesSet" /> or <see cref="IIssuesSetDiff.OlderIssuesSet" /> and has an equivalent in the other issues-set.
        bool IsPresentInBothIssuesSet(this IQualityGate qualityGate)
        ```
        
        ## COMPLETE EXAMPLE:
        
        ### FILTERING ISSUES
        ```csharp
        // Get all issues
        from issue in Issues
        select issue
        
        // Issues by severity
        from issue in Issues
        where issue.Severity == Severity.Critical
        select issue
     
        // Issues for specific rule
        from issue in Issues
        where issue.Rule.Name == "Avoid types too big"
        select new { 
            issue,
            issue.Debt,
            issue.Severity
        }
        
        // Issues in specific namespace
        from n in Application.Namespaces.WithNameLike("^MyApp.Core")
        from e in n.MeAndRecursiveChildren
        where e.HasIssue()
        from i in e.Issues()
        select i
        
        // Recent issues (from baseline)
        from issue in Issues
        where issue.WasAdded()  // New since baseline
        select issue
        
        // Fixed issues (from baseline)
        from issue in IssuesInBaseline
        where issue.WasFixed()  // Fixed since baseline
        select issue
        ```
        
        ### LISTING CRITICAL RULES VIOLATED
        
        ```csharp
        // <QualityGate Name="Critical Rules Violated" Unit="rules" />
        failif count > 0 rules
        from r in Rules where r.IsCritical && r.IsViolated()
        select new { r, issues = r.Issues() }
        ```
        
        ### LISTING QUALITY GATES STATUS EVOLUTION SINCE BASELINE
        
        ```csharp
        // <Name>Quality Gates Evolution</Name>
        from qg in QualityGates
        let qgBaseline = qg.OlderVersion()
        let relyOnDiff = qgBaseline == null
        let evolution = (relyOnDiff || qgBaseline.Value == null) ? TrendIcon.None :
                        // When a quality gate relies on diff between now and baseline
                        // it is not executed against the baseline
                        qg.ValueDiff() == 0d ?
                        TrendIcon.Constant :
                        (qg.ValueDiff() > 0 ? 
                          ( qg.MoreIsBad ?  TrendIcon.RedUp: TrendIcon.GreenUp) :
                          (!qg.MoreIsBad ?  TrendIcon.RedDown: TrendIcon.GreenDown))
        select new { qg, 
        // BaselineStatus =  relyOnDiff? (QualityGateStatus?) null : qgBaseline.Status,
           Status         =  qg.Status,
           Evolution      =  evolution,
        
           BaselineValue  =  relyOnDiff? " " : qgBaseline.ValueString,
           Value          =  qg.ValueString,
        }
        ```
        """;
}

