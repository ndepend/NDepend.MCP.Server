namespace NDepend.Mcp.Tools.CodeQuery;

internal static partial class CodeQueryFeature {

    // Define NDepend query features.
    // LLM identifies relevant features for the user's request and retrieves their prompts.
    internal const string ESSENTIAL = "code-query-essential";
    internal const string ESSENTIAL_EXPL =
        "ALWAYS REQUIRED: Provides fundamental structure, syntax, and available domains (JustMyCode, Application, ThirdParty). " +
        "MUST be included for every query generation.";

    internal const string LINE_OF_CODE = "line-of-code";
    internal const string LINE_OF_CODE_EXPL =
        "Use for counting/filtering by lines of code. " +
        "Applies to: methods, types, namespaces, projects, or entire codebase.";

    internal const string MAINTAINABILITY = "maintainability";
    internal const string MAINTAINABILITY_EXPL =
        "Use for maintainability index and Halstead volume (0-100 scale). " +
        "Applies to: methods, types, namespaces, projects/assemblies.";

    internal const string COMPLEXITY = "complexity";
    internal const string COMPLEXITY_EXPL =
        "Use for cyclomatic complexity (code paths/branches). " +
        "Applies to: methods, types.";

    internal const string COVERAGE = "coverage";
    internal const string COVERAGE_EXPL =
        "Use for test coverage metrics (% of code executed by tests). " +
        "Applies to: methods, types, namespaces, projects, entire codebase.";

    internal const string COMMENT = "comment";
    internal const string COMMENT_EXPL =
        "Use for comment metrics (percentage, documentation). " +
        "Applies to: methods, types, namespaces, projects, entire codebase.";

    internal const string USAGE_DEPENDENCY = "usage-dependency";
    internal const string USAGE_DEPENDENCY_EXPL =
        "Use for analyzing relationships between code elements (usage, coupling). " +
        "Includes: method/type/namespace/project dependencies and references.";

    internal const string PARENT_CHILDREN_RELATIONSHIP = "parent-children-relationship";
    internal const string PARENT_CHILDREN_RELATIONSHIP_EXPL =
        "Hierarchical relationship between parent and direct children. " +
        "Examples: namespace→types, type→members, type→nested types.";

    internal const string INHERITANCE_AND_BASE_CLASS = "inheritance-and-base-class";
    internal const string INHERITANCE_AND_BASE_CLASS_EXPL =
        "Use for base/derived classes and inheritance hierarchies. " +
        "Analyzes: inheritance depth, abstract classes, polymorphism.";

    internal const string INTERFACE = "interface";
    internal const string INTERFACE_EXPL =
        "Use for interface implementations and contracts. " +
        "Covers: implementing types, interface methods, segregation.";

    // SOLID and CleanCode query generation better works with related features
    internal const string SOLID_PRINCIPLES = "solid-principles";
    internal const string SOLID_PRINCIPLES_EXPL =
        "SOLID design principles: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion. " +
        "Analyzes Object Oriented design quality and architectural robustness. " +
       $"Also select: `{NAMING}`, `{PARENT_CHILDREN_RELATIONSHIP}`, `{USAGE_DEPENDENCY}`, `{INTERFACE}`, `{INHERITANCE_AND_BASE_CLASS}`";

    internal const string CLEAN_ARCHITECTURE = "clean-architecture";
    internal const string CLEAN_ARCHITECTURE_EXPL =
        "Clean Architecture principles: separation of concerns, independent layers, business rules independent from frameworks/UI/infrastructure. " +
        "Analyzes architectural boundaries and dependency directions. " +
       $"Also select: `{NAMING}`, `{PARENT_CHILDREN_RELATIONSHIP}`, `{USAGE_DEPENDENCY}`, `{INTERFACE}`";

    internal const string DDD = "domain-driven-design";
    internal const string DDD_EXPL =
         "Domain-Driven Design principles: Bounded Contexts, Entities, Value Objects, Aggregate Roots, domain layer purity, encapsulation invariants. " +
         "Analyzes tactical DDD pattern compliance and aggregate boundary integrity. " + 
        $"Also select: `{NAMING}`, `{PARENT_CHILDREN_RELATIONSHIP}`, `{USAGE_DEPENDENCY}`, `{INTERFACE}`, `{INHERITANCE_AND_BASE_CLASS}`";


    internal const string ENCAPSULATION_AND_VISIBILITY = "encapsulation-and-visibility";
    internal const string ENCAPSULATION_AND_VISIBILITY_EXPL =
        "Use for access modifiers and visibility analysis. " +
        "Covers: public/private/protected/internal members, optimal visibility, API surface.";

    internal const string STATE_MUTABILITY = "state-mutability";
    internal const string STATE_MUTABILITY_EXPL =
        "Use for field and property mutability analysis. " +
        "Identifies: readonly fields, mutable state, immutability patterns.";
    
    internal const string DIFF_SINCE_BASELINE = "diff-since-baseline";
    internal const string DIFF_SINCE_BASELINE_EXPL =
        "Use for comparing code against baseline snapshot. " +
        "Identifies: new/removed/modified elements, dependency changes, metric changes.";

    internal const string NAMING = "naming";
    internal const string NAMING_EXPL =
        "Use for naming conventions and identifier patterns. " +
        "Analyzes: naming rules, prefixes/suffixes, casing (PascalCase, camelCase).";

    internal const string ATTRIBUTE = "attribute";
    internal const string ATTRIBUTE_EXPL =
        "Use for filtering by attributes and examining attribute parameter values. " +
        "Examples: [Obsolete], [MaxLength(50)], [Category(\"Important\")], custom attributes.";

    internal const string SOURCE_FILE_DECLARATION = "source-file-declaration";
    internal const string SOURCE_FILE_DECLARATION_EXPL =
        "Use for source file location and path analysis. " +
        "Covers: file paths, file names, elements per file, file organization.";

    internal const string EVENT_PATTERN = "event-pattern";
    internal const string EVENT_PATTERN_EXPL =
        "Event-based pattern where objects communicate through events/subscribers. " +
        "Promotes loose coupling. Analyzes notification flows and reactive behavior.";

    internal const string CONSTRUCTOR_INSTANTIATION = "constructor-instantiation";
    internal const string CONSTRUCTOR_INSTANTIATION_EXPL =
        "Pattern where objects are instantiated directly via constructors. " +
        "Analyzes creation dependencies and adherence to dependency inversion.";
}