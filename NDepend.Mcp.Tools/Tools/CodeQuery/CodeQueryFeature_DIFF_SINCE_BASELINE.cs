namespace NDepend.Mcp.Tools.CodeQuery;
internal partial class CodeQueryFeature {
    internal const string DIFF_SINCE_BASELINE_PROMPT =
         """
         # Diff Since Baseline Support

         NDepend proposes to compare code changes against a baseline to see what was changed/added/removed.
         
         ## Extension Methods:

         // This methods return an ICodeElement?
         Version nav: `OlderVersion()`, `NewerVersion()`, `MeInOtherBuild()`
         
         // This methods return a bool
         ICodeElement: `WasAdded()`, `WasRemoved()`, `WasChanged()`
         Build context: `IsInNewerBuild()`, `IsInOlderBuild()`, `IsPresentInBothBuilds()`
         Third-party: `IsUsedRecently()`, `IsNotUsedAnymore()`
         ICodeContainer: `CodeWasChanged()`, `CommentsWereChanged()`
         IMember: `VisibilityWasChanged()`, `BecameObsolete()`
         
         ## Domains Related to Diff Since Baseline:

         Access current or baseline codebase using:
         
         ```csharp
         from m in codeBase                // equivalent to  context.CompareContext.NewerCodeBase
         from m in codeBase.OlderVersion() // equivalent to  context.CompareContext.OlderCodeBase
         ```
         Then access codeBase sub-domains: .Methods, .Types, .Fields, etc.
         
         ## Practical Examples

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
         
         ## Key Patterns

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
