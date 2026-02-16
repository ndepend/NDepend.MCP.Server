using NDepend.Issue;
using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Common {
    internal static class RuleProviderHelpers {

        
        internal const string RULE_PROVIDER_NDEPEND = "NDepend";
        internal const string RULE_PROVIDER_ROSLYN_ANALYZERS = "RoslynAnalyzers";
        internal const string RULE_PROVIDER_RESHARPER = "Resharper";

        internal static RuleProvider GetRuleProviderVal<C>(ILogger<C> logger, string str) {
            return str switch {
                RULE_PROVIDER_NDEPEND => RuleProvider.CodeQueryRule,
                RULE_PROVIDER_ROSLYN_ANALYZERS => RuleProvider.Roslyn,
                RULE_PROVIDER_RESHARPER => RuleProvider.Resharper,
                _ => throw logger.LogErrorAndGetException(
                    $"""
                     Invalid rule provider: `{str}`.
                     Valid values are `{RULE_PROVIDER_NDEPEND}`, `{RULE_PROVIDER_ROSLYN_ANALYZERS}`, `{RULE_PROVIDER_RESHARPER}` or null or empty string.
                     """)
            };
        }

        internal static string GetString(this RuleProvider ruleProvider) {
            return ruleProvider switch {
                RuleProvider.Roslyn => RULE_PROVIDER_ROSLYN_ANALYZERS,
                RuleProvider.Resharper => RULE_PROVIDER_RESHARPER,
                _ => RULE_PROVIDER_NDEPEND,
            };
        }
    }
}
