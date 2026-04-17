namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    internal const string INDIRECT_DEPENDENCY_PROMPT =
    """
    # Indirect Dependency Analysis

    ## Key APIs

    ### Extension Methods for Indirect Dependencies (prefer these)
    IEnumerable<TUsed> IndirectlyUsedBy<TUsed,TUser>(
        this IEnumerable<TUsed>, TUser user)
        // Elements directly OR indirectly used by user.

    IEnumerable<TUsed> IndirectlyUsedByAny<TUsed,TUser>(
        this IEnumerable<TUsed>, IEnumerable<TUser> users)

    ICodeMetric<TUsed,ushort> DepthOfIsUsedBy<TUsed,TUser>(
        this IEnumerable<TUsed>, TUser user)
        // Returns metric with depth value per element.

    ICodeMetric<TUsed,ushort> DepthOfIsUsedByAny<TUsed,TUser>(
        this IEnumerable<TUsed>, IEnumerable<TUser> users)

    IEnumerable<TUser> IndirectlyUsing<TUser,TUsed>(
        this IEnumerable<TUser>, TUsed used)
        // Elements directly OR indirectly using used.

    IEnumerable<TUser> IndirectlyUsingAny<TUser,TUsed>(
        this IEnumerable<TUser>, IEnumerable<TUsed> usedElements)

    ICodeMetric<TUser,ushort> DepthOfIsUsing<TUser,TUsed>(
        this IEnumerable<TUser>, TUsed used)

    ICodeMetric<TUser,ushort> DepthOfIsUsingAny<TUser,TUsed>(
        this IEnumerable<TUser>, IEnumerable<TUsed> usedElements)

    // Overloads by full name string
    bool IsIndirectlyUsedBy(this IUsed used, string userFullName)
    bool IsIndirectlyUsing(this IUser user, string usedFullName)
    ushort? DepthOfIsUsedBy(this IUsed used, string userFullName)
    ushort? DepthOfIsUsing(this IUser user, string usedFullName)

    ### FillIterative (transitive closure helper)
    ICodeMetric<TCodeElement,ushort> FillIterative<TCodeElement>(
        this IEnumerable<TCodeElement> initialSeq,
        Func<IEnumerable<TCodeElement>, IEnumerable<TCodeElement>> func)
    // Iteratively expands a sequence until no new elements are added.
    // Use for custom transitive traversals.

    ### IUser.Level
    ushort? Level { get; }
    Level is a dependency depth metric for assemblies, namespaces, types, and methods:
    - 0 if it uses no other namespaces
    - 1 if it only depends on third-party namespaces
    - otherwise 1 + max(Level of its direct dependencies)
    - null if it is in a direct or indirect dependency cycle
    
    ## QUERY PATTERNS

    // All types used directly or indirectly by a specific type (with depth)
    from t in Types
    let depth = t.DepthOfIsUsedBy("Product.OrderService")
    where depth >= 0
    orderby depth
    select new { t, depth }

    // All types directly or indirectly using a specific type (with depth)
    from t in Types
    let depth = t.DepthOfIsUsing("Product.Customer")
    where depth >= 0
    orderby depth
    select new { t, depth }

    // Indirect circular dependencies between namespaces (A→B→…→A)
    let dico = Application.Namespaces.ToDictionary(
        n => n,
        n => n.NamespacesUsed.FillIterative(
                ns => ns.SelectMany(nx => nx.NamespacesUsed)
            ).DefinitionDomain
            .ToHashSetEx())
    from n1 in dico
    from n2 in dico
    where string.Compare(n1.Key.Name, n2.Key.Name) == 1
       && n1.Value.Contains(n2.Key)
       && n2.Value.Contains(n1.Key)
    select new { n1.Key, other = n2.Key, Issue = "Indirect mutual dependency" }

    // Direct mutual dependency (type-level cycles)
    from t1 in Types
    from t2 in t1.TypesUsed
    where t2.TypesUsed.Contains(t1)
    select new { Type1 = t1, Type2 = t2 }

    // Namespace-level direct cycles
    from n in Application.Namespaces
    from nUsed in n.NamespacesUsed
    where nUsed.NamespacesUsed.Contains(n)
    select new { Namespace1 = n, Namespace2 = nUsed }

    // Change impact: what is affected if type X changes?
    from t in Types
    let depth = t.DepthOfIsUsing("Product.IRepository")
    where depth >= 0
    orderby depth
    select new { t, depth }
    """;
}
