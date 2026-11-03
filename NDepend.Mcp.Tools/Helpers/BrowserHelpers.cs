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

            // 29Sep2026: a headless Linux box (no display system like DISPLAY / WAYLAND_DISPLAY: 
			// on a server, a CI agent, WSL1) has nothing to open the page with — 
			// say where it is instead of failing slowly.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) &&
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"))) {
                logger.LogInformation($"No graphical session: open the file yourself: {filePath.ToString()}");
                return;
            }

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
