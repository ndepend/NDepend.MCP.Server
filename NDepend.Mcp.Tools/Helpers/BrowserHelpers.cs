using System.Diagnostics;
using System.Runtime.InteropServices;
using NDepend.Path;

namespace NDepend.Mcp.Helpers {

    internal static class BrowserHelpers {

        internal static void OpenHtmlDiagram(ILogger logger, IAbsoluteFilePath tempHtmlPath) {
            logger.LogInformation($"Html Svg exported to {tempHtmlPath}.");
            OpenLocalHtml(tempHtmlPath, logger);
        }
  
        internal static void OpenLocalHtml(IAbsoluteFilePath filePath, ILogger logger) {

            logger.LogInformation($"Opening file {filePath.ToString()}");
            // Normalize path to absolute file:// URL
            string url = new Uri(filePath.ToString()!).AbsoluteUri;

            try {
                // .NET Core and .NET 5+, also works on Linux/macOS when UseShellExecute=true
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            } catch {
                // Fallback for older runtimes or edge cases
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    Process.Start("xdg-open", url);
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    Process.Start("open", url);
                } else {
                    logger.LogInformation($"Fail opening file {filePath.ToString()}");
                }
            }
        }
    }
}
