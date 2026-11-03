namespace NDepend.Mcp.Tools.CodeQuery;
public partial class CodeQueryFeature {
    public const string PERFORMANCE_PROMPT =
          $"""
          # Query Execution Performance
          
          CQLinq runs hundreds of queries/sec; most finish in a few ms. A query exceeding the timeout
          (default 2 seconds, Tools > Options > Code Query) is aborted. Aim for linear O(n) — never a nested O(n²) loop.
          
          Here are the top 5 advice you MUST follow:
          
          ## Always Profile
          
          Measure query execution time using the MCP tool `{CodeQueryTools.TOOL_RUN_QUERY_NAME}`.  
          Read the duration from `ExecutionDurationInMilliseconds`.
          
          ## Declare sub-queries with `let` ABOVE the main `from` loop and below the eventual `warnif` `failif` (if any).

          A query nested inside the main loop is O(#elements²). Pre-compute the filtered set ONCE above the
          loop, then match against it — this turns dozens of seconds into a few ms.

          ```csharp
          // SLOW: nested query, O(#Methods²)
          from m in Application.Methods
          where m.MethodsCalled.Any(c => c.IsPublic && (c.NbLinesOfCode ?? 0) > 100)
          select m

          // FAST: pre-declare the set once (O(1) lookups), then match
          let bigPublic = Application.Methods.Where(x => x.IsPublic && (x.NbLinesOfCode ?? 0) > 100).ToHashSet()
          from m in Application.Methods
          where m.MethodsCalled.Any(bigPublic.Contains)
          select m
          ```
          
          ## Choose the more performant filter order
          
          Queries often apply a successive list of filters. Checking a boolean like `method.IsStatic` or `method.CyclomaticComplexity < 20` is much faster than checking `type.DeriveFrom("BaseClassName".AllowNoMatch())`.
          
          Make sure the filter order is optimal: check the fastest filters first and the slowest last. This can reduce the number of elements that reach the expensive filters by orders of magnitude.
          
          ## Target O(1) performance
          
          To do so prefer `HashSet` / `Dictionary` / `Lookup` for membership checks (e.g. `ToHashSetEx()`, `ToDictionary()`, `ToLookup()`).
          For complex algorithms, explicitly search for opportunities to replace O(n) or O(log n) operations with O(1) solutions using hash-based lookups or precomputed indexing.

          ## Prefer the optimized extension methods over manual loops (~10x faster)

          - `UsingAny()` / `UsedByAny()` (and `Using`/`UsedBy`/`UsingAll`/`UsedByAll`): dependency matching.
          - `ThatImplementAny()`: types implementing given interfaces.
          - `WithNameIn()` / `WithFullNameIn()`: match many names — replaces `t.Name == "A" || t.Name == "B" || …`.
          - `ToHashSet()` then `.Contains()`: O(1) membership, can be >200x faster than a linear scan.

          ## Avoid `let` evaluated per row

          Each per-row `let` allocates a new object and copies all values already obtained, so several `let`
          clauses in the main loop can run ~2x slower. Keep one combined `let`/`select`, and build any HashSet
          ONCE above the loop — never via a per-row `let` (the `let` overhead cancels the HashSet gain).
          """;
}
