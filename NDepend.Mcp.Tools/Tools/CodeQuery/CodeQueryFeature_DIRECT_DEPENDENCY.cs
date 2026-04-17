
namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    internal const string DIRECT_DEPENDENCY_PROMPT =
    """
    # Direct Dependency Analysis

    ## Key APIs

    ### Dependency Properties
    // IType
    IEnumerable<IType> TypesUsed { get; }        // outbound type deps
    IEnumerable<IType> TypesUsingMe { get; }     // inbound type deps
    uint? NbTypesUsed { get; }
    uint? NbTypesUsingMe { get; }

    // INamespace
    IEnumerable<INamespace> NamespacesUsed { get; }
    IEnumerable<INamespace> NamespacesUsingMe { get; }

    // IAssembly
    IEnumerable<IAssembly> AssembliesUsed { get; }
    IEnumerable<IAssembly> AssembliesUsingMe { get; }

    // IMethod
    IEnumerable<IMethod> MethodsCalled { get; }
    IEnumerable<IMethod> MethodsCallingMe { get; }

    // IField
    IEnumerable<IMethod> MethodsUsingMe { get; }
    IEnumerable<IMethod> MethodsAssigningMe { get; }
    IEnumerable<IMethod> MethodsReadingMeButNotAssigningMe { get; }

    // IProperty
    IEnumerable<IMethod> MethodsReadingMe { get; }
    IEnumerable<IMethod> MethodsWritingMe { get; }

    ### IUsed / IUser Interfaces
    Implemented by: IAssembly, INamespace, IType, IMethod, IField, IProperty, IEvent
    (IField does not implement IUser — fields don’t call code elements)

    IUsed — IsUsedBy(ICodeElement user|string userFullName)
           plus IsUsedByAssembly(IAssembly user) same for Namespace/Type/Method/Property/Event
    IUser — IsUsing(ICodeElement used|string usedFullName)
           plus IsUsingAssembly(IAssembly used) same for Namespace/Type/Method/Field/Property/Event

    ### Extension Methods (prefer these — optimized)
    IEnumerable<TUsed> UsedBy<TUsed,TUser>(this IEnumerable<TUsed>, TUser user)
    IEnumerable<TUsed> UsedByAny<TUsed,TUser>(this IEnumerable<TUsed>, IEnumerable<TUser>)
    IEnumerable<TUsed> UsedByAll<TUsed,TUser>(this IEnumerable<TUsed>, IEnumerable<TUser>)
    IEnumerable<TUser> Using<TUser,TUsed>(this IEnumerable<TUser>, TUsed used)
    IEnumerable<TUser> UsingAny<TUser,TUsed>(this IEnumerable<TUser>, IEnumerable<TUsed>)
    IEnumerable<TUser> UsingAll<TUser,TUsed>(this IEnumerable<TUser>, IEnumerable<TUsed>)

    ## Query Patterns

    // Types directly used by a specific type
    from t in Types
    where t.IsUsedBy("Product.OrderService")
    select new { t, t.NbLinesOfCode }

    // Types directly using a specific type
    from t in Types
    where t.IsUsing("Product.Customer")
    select new { t, t.NbLinesOfCode }

    // Coupling metrics (afferent / efferent / instability)
    from t in Types where t.IsClass
    let ce = t.TypesUsed.Count()
    let ca = t.TypesUsingMe.Count()
    let instability = (ce + ca) > 0 ? (double)ce / (ce + ca) : 0
    select new { t, Ce = ce, Ca = ca, Instability = instability }

    // Highly coupled types
    from t in Types
    where t.TypesUsed.Count() > 20
    select new { t, Dependencies = t.TypesUsed }

    // Namespace dependency graph
    from n in Application.Namespaces
    where n.NbNamespacesUsed >= 1
    orderby n.NbNamespacesUsed descending
    select new { n, n.NamespacesUsed, n.NamespacesUsingMe }

    // Types involved in coupling between two assemblies
    from aCaller in Assemblies
    from aCalled in aCaller.AssembliesUsed
    let TypesCallers = aCaller.ChildTypes.UsingAny(aCalled.ChildTypes).ToArray()
    let TypesCalled = aCalled.ChildTypes.UsedByAny(aCaller.ChildTypes).ToArray()
    select new {
        aCaller,
        aCalled,
        TypesCallers,
        TypesCalled
    }

    // Assembly dependency graph
    from a in Application.Assemblies
    where a.AssembliesUsed.Count() >= 1
    orderby a.AssembliesUsed.Count() descending
    select new { a, a.AssembliesUsed, a.AssembliesUsingMe }

    // Third-party (non-BCL) assembly dependencies
    from a in Assemblies
    let thirdParties = a.AssembliesUsed.Where(
        au => au.IsThirdParty &&
              !au.Name.StartsWithAny("System","Microsoft")).ToArray()
    where thirdParties.Any()
    select new { a, thirdParties }

    // God types (used by many)
    from t in Application.Types
    where t.TypesUsingMe.Count() > 30
    select new { t, UsedBy = t.TypesUsingMe }

    // Pairs of mutually dependent types (direct cycle)
    from t1 in Types
    from t2 in t1.TypesUsed
    where t2.TypesUsed.Contains(t1)
    select new { Type1 = t1, Type2 = t2 }
    """;
}
