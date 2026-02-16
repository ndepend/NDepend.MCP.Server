using System.Globalization;


namespace NDepend.Mcp.Helpers {
    internal static class TimeHelpers {

        static readonly CultureInfo s_EnUS = new CultureInfo("en-US");

        // "09 Dec 2025 15:01:23"
        internal static string GetString(this DateTime dateTime) {
            return dateTime.ToString("dd MMM yyyy HH:mm:ss", s_EnUS);
        }

    }
}
