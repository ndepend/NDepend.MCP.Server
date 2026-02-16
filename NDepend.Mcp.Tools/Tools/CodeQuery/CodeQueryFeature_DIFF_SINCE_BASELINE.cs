namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string DIFF_SINCE_BASELINE_PROMPT =
         """
         # DIFF SINCE BASELINE SUPPORT
         
         NDepend proposes to compare code changes against a baseline to see what was changed/added/removed.
         
         ## EXTENSION METHODS FOR DIFF SINCE BASELINE:
         
         In CQLinq, there are several ICodeElement extension methods to help writing queries that focus on code changes since the baseline:
         
         - Change Detection
         WasAdded() → bool  Returns true if this current code element was added since the baseline.
         WasRemoved() → bool  Returns true if this baseline code element was removed since the baseline.
         WasChanged() → bool  Returns true if this code element has been changed since the baseline.
         
         - Version Navigation
         OlderVersion() → ICodeElement?  Returns the older version (in baseline) of this current code element if available, else returns null, returns null also when this element is in baseline.
         NewerVersion() → ICodeElement?  Returns the newer version (in current) of this baseline code element if available, else returns null, returns null also when this element is in the current code snapshot.
         MeInOtherBuild() → ICodeElement? Returns the version of the code element in the other build (baseline or current) if available.
         
         - Build Context
         IsInNewerBuild() → bool Returns true if this code element belongs to the newer build (current) compared to the other build (baseline).
         IsInOlderBuild() → bool Returns true if this code element belongs to the newer build (current) compared to the other build (baseline).
         IsPresentInBothBuilds() → bool Returns true if this code element is present in both the baseline and current builds.
         
         - Third-Party Dependencies
         IsUsedRecently() → bool Returns true if the code element is in a third-party assembly (or is a third-party assembly itself), and if it is used by the newer version of the code base, but not by the older version.
         IsNotUsedAnymore() → bool Returns true if the code element is in a third-party assembly (or is a third-party assembly itself), and if it is used by the older version of the code base, but not by the newer version.
         
         - Extension methods on ICodeContainer:
         CodeWasChanged() → bool Returns true if the code of this code container, has been modified.
         CommentsWereChanged() → bool Returns true if comments of this code container, have been modified.
         
         - Extension methods on IMember:
         VisibilityWasChanged() → bool Returns true if the visibility of this member has been changed.
         BecameObsolete() → bool Returns true if the member is not tagged with 'System.ObsoleteAttribute' in the older version of the code base, but is tagged as obsolete in the newer version of the code base.
         
         ## DOMAINS RELATED TO DIFF SINCE BASELINE:
         
         Access current or baseline codebase using:
         
         ```csharp
         from m in codeBase                // equivalent to  context.CompareContext.NewerCodeBase
         from m in codeBase.OlderVersion() // equivalent to  context.CompareContext.OlderCodeBase
         ```
         Then access codeBase sub-domains: .Methods, .Types, .Fields, etc.
         
         ## PRACTICAL EXAMPLES
         
         Example 1: Methods with Increased Complexity
         ```csharp
         // <Name>Methods that became more complex</Name>
         from m in codeBase.OlderVersion().Methods 
         where m.IsPresentInBothBuilds() 
         let oldCC = m.CyclomaticComplexity
         let newCC = m.NewerVersion().CyclomaticComplexity
         where oldCC != null && newCC > oldCC
         select new { m, oldCC, newCC }
         ```
         
         Example 2: New Code with Debt Analysis
         ```csharp
         // <Name>New code elements added since baseline with their debt and issues</Name>
         from elem in Application.CodeElements 
         where elem.WasAdded() 
         select new {
            elem,
            Debt = elem.AllDebt(),
            Issues = elem.AllIssues()
         }
         ```
         
         Example 3: Modified Code with LOC Changes
         ```csharp
         // <Name>Code containers with changed code since baseline with LOC delta, debt and issues</Name>
         from codeContainer in Application.CodeContainers 
         where codeContainer.CodeWasChanged() 
         let newLoc = codeContainer.NbLinesOfCode
         let oldLoc = codeContainer.OlderVersion().NbLinesOfCode
         select new {
            codeContainer,
            newLoc,
            oldLoc,
            delta = (int?)(newLoc >= oldLoc ? newLoc - oldLoc : -(oldLoc - newLoc)),
            Debt = codeContainer.AllDebt(),
            Issues = codeContainer.AllIssues()
         }
         ```
         
         Example 4: Removed Public APIs
         ```csharp
         // <Name>Public methods removed since baseline</Name>
         from m in codeBase.OlderVersion().Methods
         where m.WasRemoved() && m.IsPublic
         select m
         ```
         
         Example 5: Elements with Changed Visibility
         ```csharp
         // <Name>Members with modified access levels</Name>
         from m in Application.Members
         where m.IsPresentInBothBuilds() && m.VisibilityWasChanged()
         let oldVis = m.OlderVersion().Visibility
         let newVis = m.Visibility
         select new { m, oldVis, newVis }
         ```
         
         ## KEY PATTERNS
         
         Always null-check when navigating older or newer versions:
         ```csharp
         let older = m.OlderVersion()
         where older != null  // Prevent null reference errors
         ```
         
         ```csharp
         Check presence before comparing:
         where m.IsPresentInBothBuilds()  // Ensures both versions exist
         ```
         
         Start from appropriate domain:
         - Start from `codeBase.OlderVersion()` to query removed elements
         - Start from current `codeBase` to query added elements
         - Use `IsPresentInBothBuilds()` to query changed elements
         """;
}
