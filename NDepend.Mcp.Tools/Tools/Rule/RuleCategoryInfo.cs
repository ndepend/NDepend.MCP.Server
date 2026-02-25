
namespace NDepend.Mcp.Tools.Rule;

[Description("Represents a rule category, including its name, rule summaries, and any child categories.")]
public record RuleCategoryInfo(string Name) {
    [Description("Category name")]
    public string Name { get; set; } = Name;
    [Description("Provider of child rules")]
    public string Provider { get; set; } = "";
    [Description("Rules in this category")]
    public RuleSummaryInfo[] RuleSummary { get; set; } = [];
    [Description("Subcategories")]
    public RuleCategoryInfo[] ChildCategories { get; set; } = [];
}
