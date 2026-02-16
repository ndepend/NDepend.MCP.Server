
namespace NDepend.Mcp.Tools.Rule {
    [Description("Represents a rule category, including its name, rule summaries, and any child categories.")]
    public record RuleCategoryInfo(string Name) {

        [Description("The name of the rule category.")]
        public string Name { get; set; } = Name;
        [Description("Gets or sets the provider of the child rules.")]
        public string Provider { get; set; } = "";

        [Description("The array of rule summaries belonging to this category.")]
        public RuleSummaryInfo[] RuleSummary { get; set; } = [];

        [Description("The array of child rule categories under this category.")]
        public RuleCategoryInfo[] ChildCategories { get; set; } = [];
    }
}
