
namespace NDepend.Mcp.Helpers {
    internal static class StringHelpers {

        // Sometimes the agent fill with "null" string value instead of null reference
        internal static bool IsValid([NotNullWhen(true)] this string? str) {
            return !string.IsNullOrEmpty(str) && str != "null"; 
        }

    }
}
