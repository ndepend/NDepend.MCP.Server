using NDepend.Mcp.Helpers;
using NDepend.TechnicalDebt;

namespace NDepend.Mcp.Tools.Common {
    internal static class SeverityHelpers {
        internal const string SEVERITY_LOW = "low";
        internal const string SEVERITY_MEDIUM = "medium";
        internal const string SEVERITY_HIGH = "high";
        internal const string SEVERITY_CRITICAL = "critical";
        internal const string SEVERITY_BLOCKER = "blocker";
        internal const string SEVERITY_ALL = "all";

        internal static Severity GetSeverity<C>(ILogger<C> logger, string str) {
            return str.ToLowerInvariant() switch {
                SEVERITY_LOW => Severity.Low,
                SEVERITY_MEDIUM => Severity.Medium,
                SEVERITY_HIGH => Severity.High,
                SEVERITY_CRITICAL => Severity.Critical,
                SEVERITY_BLOCKER => Severity.Blocker,
                _ => throw logger.LogErrorAndGetException($"Invalid severity: {str}. Valid values are '{SEVERITY_LOW}', '{SEVERITY_MEDIUM}', '{SEVERITY_HIGH}', '{SEVERITY_CRITICAL}', '{SEVERITY_BLOCKER}'.")
            };
        }


        internal static string GetString(this Severity severity) {
            return severity switch {
                Severity.Low => SEVERITY_LOW,
                Severity.Medium => SEVERITY_MEDIUM,
                Severity.High => SEVERITY_HIGH,
                Severity.Critical => SEVERITY_CRITICAL,
                _ => SEVERITY_BLOCKER,
            };
        }
    }
}
