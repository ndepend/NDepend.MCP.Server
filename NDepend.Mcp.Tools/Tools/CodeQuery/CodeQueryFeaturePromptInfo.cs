
namespace NDepend.Mcp.Tools.CodeQuery;

[Description("Prompt for a code query feature with name and generation instructions.")]
public sealed class CodeQueryFeaturePromptInfo(string feature, string prompt) {
    [Description("Feature name")]
    public string Feature { get; set; } = feature;
    [Description("Prompt explaining how to use this feature in queries")]
    public string Prompt { get; set; } = prompt;
}