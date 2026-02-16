namespace NDepend.Mcp.Tools.CodeQuery;

internal static partial class CodeQueryFeature {

    // Describe the NDepend code querying features, so depending on the user request
    // the LLM can choose the most relevant ones it should become an expert in by fetching a feature prompt.
    internal const string ESSENTIAL = "code-query-essential";
    internal const string ESSENTIAL_EXPL =
        "ALWAYS REQUIRED: Provides the fundamental structure and syntax for NDepend code queries, " +
        "including available domains (JustMyCode, Application, ThirdParty) and query patterns. " +
        "MUST be included for every code query generation request.";

    internal const string LINE_OF_CODE = "line-of-code";
    internal const string LINE_OF_CODE_EXPL =
        "Use when counting or filtering by lines of code (LOC). " +
        "Applies to: methods, types (classes/structs), namespaces, projects, or entire codebase. " +
        "Relevant for queries about code size or volume metrics.";

    internal const string MAINTAINABILITY = "maintainability";
    internal const string MAINTAINABILITY_EXPL =
        "Use for maintainability index, Halstead volume and related quality metrics. " +
        "Measures code maintainability on a 0-100 scale. " +
        "Applies to: methods, types, namespaces, projects / assemblies. " +
        "Relevant for queries about code quality, technical debt, or ease of maintenance.";

    internal const string COMPLEXITY = "complexity";
    internal const string COMPLEXITY_EXPL =
        "Use for cyclomatic complexity metrics (measure of code paths/branches). " +
        "Applies to: methods, types (classes/structs). " +
        "Relevant for queries about code complexity, decision points, or potential testing difficulty.";

    internal const string COVERAGE = "coverage";
    internal const string COVERAGE_EXPL =
        "Use for test coverage metrics (percentage of code executed by tests). " +
        "Applies to: methods, types, namespaces, projects and the entire code base. " +
        "Relevant for queries about testing, untested code, or coverage thresholds.";

    internal const string COMMENT = "comment";
    internal const string COMMENT_EXPL =
        "Use for comment-related metrics (comment percentage, documentation). " +
        "Applies to: methods, types, namespaces, projects and the entire code base. " +
        "Relevant for queries about documentation level or comment ratios.";

    internal const string USAGE_DEPENDENCY = "usage-dependency";
    internal const string USAGE_DEPENDENCY_EXPL =
        "Use when analyzing relationships between code elements (what uses what, coupling metrics). " +
        "Includes: type dependencies, namespace dependencies, project / assembly references. " +
        "Relevant for queries about coupling, architecture violations, or dependency graphs.";

    internal const string PARENT_CHILDREN_RELATIONSHIP = "parent-children-relationship";
    internal const string PARENT_CHILDREN_RELATIONSHIP_EXPL =
         "Represents the hierarchical relationship between a parent code element and its direct children. " +
         "For example, a namespace and its contained types, a type and its contained members or a type and its nested types. " +
         "Useful to navigate containment structures in code.";
    internal const string INHERITANCE_AND_BASE_CLASS = "inheritance-and-base-class";
    internal const string INHERITANCE_AND_BASE_CLASS_EXPL =
        "Use for base class, derived class, class inheritance hierarchies and relationships. " +
        "Analyzes: base classes, derived classes, inheritance depth, abstract classes. " +
        "Relevant for queries about object-oriented design, inheritance chains, or polymorphism.";

    internal const string INTERFACE = "interface";
    internal const string INTERFACE_EXPL =
        "Use for interface implementations and contracts. " +
        "Covers: types implementing interfaces, interface methods, interface segregation. " +
        "Relevant for queries about abstraction, contracts, or interface usage patterns.";

    // SOLID and CleanCode query generation better works with related features
    internal const string SOLID_PRINCIPLES = "solid-principles";
    internal const string SOLID_PRINCIPLES_EXPL =
        "Refers to the SOLID design principles: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion. " +
        "These principles help design code that is easier to maintain, extend, and test. " +
        "Used to analyze object-oriented design quality and architectural robustness." +
       $"If you select this feature make sure to also select thse features: `{NAMING}`, `{PARENT_CHILDREN_RELATIONSHIP}`, `{USAGE_DEPENDENCY}`, `{INTERFACE}`, `{INHERITANCE_AND_BASE_CLASS}`";

    internal const string CLEAN_ARCHITECTURE = "clean-architecture";
    internal const string CLEAN_ARCHITECTURE_EXPL =
        "Refers to Clean Architecture principles that promote clear separation of concerns and independent layers. " +
        "Business rules are kept independent from frameworks, UI, and infrastructure details. " +
        "Used to analyze architectural boundaries, dependency directions, and long-term maintainability." +
       $"If you select this feature make sure to also select thse features: `{NAMING}`, `{PARENT_CHILDREN_RELATIONSHIP}`, `{USAGE_DEPENDENCY}`, `{INTERFACE}`";

    internal const string ENCAPSULATION_AND_VISIBILITY = "encapsulation-and-visibility";
    internal const string ENCAPSULATION_AND_VISIBILITY_EXPL =
        "Use for access modifiers and visibility analysis. " +
        "Covers: public/private/protected/internal members, visibility violations, encapsulation principles, non-optimal visibility. " +
        "Relevant for queries about API surface, exposed internals, or access level issues.";

    internal const string STATE_MUTABILITY = "state-mutability";
    internal const string STATE_MUTABILITY_EXPL =
        "Use for field mutability analysis. " +
        "Identifies: readonly fields, mutable state, immutability patterns. " +
        "Relevant for queries about immutable design, state management, or readonly enforcement.";

    internal const string DIFF_SINCE_BASELINE = "diff-since-baseline";
    internal const string DIFF_SINCE_BASELINE_EXPL =
        "Use for comparing current code against a baseline snapshot. " +
        "Identifies: new/removed/modified code elements, dependency changes, metric changes over time. " +
        "Relevant for queries about recent changes, code evolution, or regression detection.";

    internal const string NAMING = "naming";
    internal const string NAMING_EXPL =
        "Use for naming conventions and identifier patterns. " +
        "Analyzes: naming rules, prefixes/suffixes, casing conventions (PascalCase, camelCase). " +
        "Relevant for queries about naming standards, convention violations, or identifier patterns.";

    internal const string ATTRIBUTE = "attribute";
    internal const string ATTRIBUTE_EXPL =
        "Use when filtering by attributes/annotations applied to code elements and when examining specific attribute parameter values (not just attribute presence). " +
        "Examples: [Obsolete], [Serializable], custom attributes, hecking [MaxLength(50)] value, [Category(\"Important\")] category name. " +
        "Relevant for queries about decorated members or attribute presence and for inspecting attribute arguments or named parameters.";

    internal const string SOURCE_FILE_DECLARATION = "source-file-declaration";
    internal const string SOURCE_FILE_DECLARATION_EXPL =
        "Use for source file location and path analysis. " +
        "Covers: file paths, file names, code elements per file, file organization. " +
        "Relevant for queries about file structure, file-based filtering, or locating declarations.";

    internal const string EVENT_PATTERN = "event-pattern";
    internal const string EVENT_PATTERN_EXPL =
        "Represents the use of the event-based design pattern, where objects communicate through events and subscribers. " +
        "This pattern promotes loose coupling between event publishers and event handlers. " +
        "Useful to analyze notification flows and reactive behavior in the codebase.";

    internal const string CONSTRUCTOR_INSTANTIATION = "constructor-instantiation";
    internal const string CONSTRUCTOR_INSTANTIATION_EXPL =
        "Represents the pattern where objects are instantiated directly using constructors. " +
        "This highlights creation dependencies and tight coupling between types. " +
        "Useful to analyze object creation responsibilities and adherence to dependency inversion.";

}