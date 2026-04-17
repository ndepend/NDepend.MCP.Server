namespace NDepend.Mcp.Tools.CodeQuery;

internal partial class CodeQueryFeature {
    internal const string ARCHITECTURE_VIOLATION_PROMPT =
    """
    # Architecture Violation Detection

    Your task is to generate accurate CQLinq queries that detect
    architectural violations (layer boundaries, DIP, forbidden dependencies)
    in .NET codebases.

    ## Key APIs

    ### Dependency Properties used in violation queries
    IEnumerable<IType> TypesUsed { get; }        // on IType
    IEnumerable<IType> TypesUsingMe { get; }

    IEnumerable<INamespace> NamespacesUsed { get; }    // on INamespace

    ### IUsed / IUser Interfaces 
    Implemented by: IAssembly, INamespace, IType, IMethod, IField, IProperty, IEvent
    bool IsUsing(ICodeElement used)
    bool IsUsingType(IType used)
    bool IsUsing(string usedFullName)
    bool IsIndirectlyUsing(string usedFullName)

    ### Extension Methods (prefer these — optimized)
    IEnumerable<TUser> UsingAny<TUser,TUsed>(this IEnumerable<TUser>, IEnumerable<TUsed>)
    IEnumerable<TUser> IndirectlyUsingAny<TUser,TUsed>(this IEnumerable<TUser>, IEnumerable<TUsed>)

    ## Architectural Concepts
    - Layered architecture: UI → Business → Data (no upward or skip-layer deps)
    - Clean / Onion: dependencies point inward (domain has no outbound deps)
    - Dependency Inversion (DIP): high-level modules depend on abstractions, not implementations
    - Acyclic dependencies: no circular references between modules

    ## Query Patterns

    // Layer violation: UI depending directly on Data
    let uiTypes = Application.Namespaces.WithNameWildcardMatch("*UI*").ChildTypes()
    let dbTypes = Application.Namespaces.WithNameWildcardMatch("*Data*").ChildTypes().ToHashSetEx()
    from tUI in uiTypes.UsingAny(dbTypes)
    let dbTypesUsed = tUI.TypesUsed.Intersect(dbTypes)
    select new { tUI, dbTypesUsed , Violation = "Layer violation" }

    // DIP violation: Business/Domain depending on Infrastructure/Data concretions
    let bizTypes= Application.Namespaces.WithNameWildcardMatchIn("*Business*","*Domain*").ChildTypes()
    let infraTypes= Application.Namespaces.WithNameWildcardMatchIn("*Data*", "*Infra*").ChildTypes().ToHashSetEx()
    from tBiz in bizTypes.UsingAny(infraTypes)
    let infraUsed = tBiz.TypesUsed.Intersect(infraTypes)
    select new { tBiz, infraUsed, Violation = "DIP violation" }

    // Namespace-level layer violations
    let uiNS = Application.Namespaces.WithNameWildcardMatch("*UI*")
    let dataNS = Application.Namespaces.WithNameWildcardMatch("*Data*").ToHashSetEx()
    from n in uiNS
    where n.NamespacesUsed.Any(nu => dataNS.Contains(nu))
    select new { n, Violation = "UI namespace uses Data namespace" }

    // External library dependency audit (non-BCL, non-first-party)
    from t in Application.Types where t.IsClass
    let externalDeps = t.TypesUsed.Where(tu =>
        !tu.ParentAssembly.Name.StartsWithAny("System","Microsoft","netstandard") &&
         tu.ParentAssembly != t.ParentAssembly)
    where externalDeps.Any()
    select new { t, ExternalDependencies = externalDeps }

    // Assemblies with forbidden third-party dependencies
    from a in Application.Assemblies
    let forbidden = a.AssembliesUsed.Where(
        au => au.IsThirdParty &&
              !au.Name.StartsWithAny("System","Microsoft","netstandard")).ToArray()
    where forbidden.Any()
    select new { a, forbidden }
    """;
}
