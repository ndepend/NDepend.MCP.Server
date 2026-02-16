
namespace NDepend.Mcp.Tools.CodeQuery;

[method: Description("Create a new instance of the CodeQueryFeaturePromptInfo class with the specified data.")]
[Description("Represents a prompt for a specific code query feature, including the feature name and its prompt text.")]
public sealed class CodeQueryFeaturePromptInfo(string feature, string prompt) {

    [Description("The standardized code query feature name.")]
    public string Feature { get; set; } = feature;

    [Description("The prompt that explains how to generate the query to use this feature.")]
    public string Prompt { get; set; } = prompt;
}
