using System.Diagnostics;
using NDepend.Path;
using NDepend.Project;

namespace NDepend.Mcp.Helpers {

    public record NewAnalysisResultParams(
        IProject Project,
        ILogger Logger,
        IAbsoluteFilePath NewNdarFilePath
    );

    // Watch for when .ndar gets created in the ndepend-out directory, to reload it
    // .ndar is the proprietary NDepend analysis result file format
    internal sealed class NdarWatcher : IDisposable {

        private readonly NewAnalysisResultParams m_Params;
        private readonly FileSystemWatcher m_Watcher;
        private readonly Action<NewAnalysisResultParams> m_OnCreatedHandler;
        private bool m_IsDisposed;

        public NdarWatcher(NewAnalysisResultParams @params, Action<NewAnalysisResultParams> onCreatedHandler) {
            m_Params = @params;

            m_Watcher = new FileSystemWatcher {
                Path = m_Params.NewNdarFilePath.ParentDirectoryPath.ToString()!,
                Filter = "*.ndar",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            m_Watcher.Created += OnCreatedHandler;
            m_Watcher.Renamed += OnCreatedHandler;
            m_Watcher.Changed += OnCreatedHandler;
            m_OnCreatedHandler = onCreatedHandler;
        }

        private void OnCreatedHandler(object sender, FileSystemEventArgs e) {
            string fullPath = e.FullPath;

            if (fullPath.TryGetAbsoluteFilePath(out var newNdarFilePath)) {
                var logger = m_Params.Logger;
                logger.LogInformation($"Detected new .ndar file created: {newNdarFilePath}");
                WaitForFileAvailable(newNdarFilePath, logger); // Might be used by another process right after creation
                m_OnCreatedHandler(new NewAnalysisResultParams(
                    m_Params.Project,
                    logger,
                    newNdarFilePath));
            }
        }

        public void Dispose() {
            // Dispose() can be called multiple times safely
            if (m_IsDisposed) return;
            m_IsDisposed = true;
            m_Watcher.EnableRaisingEvents = false;
            m_Watcher.Created -= OnCreatedHandler;
            m_Watcher.Renamed -= OnCreatedHandler;
            m_Watcher.Changed -= OnCreatedHandler;
            m_Watcher.Dispose();
        }


        private static void WaitForFileAvailable(IAbsoluteFilePath filePath, ILogger logger, int retryDelayMs = 200, int maxWaitMs = 10000) {
            var sw = Stopwatch.StartNew();

            // 29Sep2026 (Linux/macOS): the FileShare.None probe below is a no-op there (Unix does not
            // enforce sharing modes), so a file still being written opens at once — wait until its
            // size stops changing instead. Windows keeps the exclusive-open probe only.
            long lastLength = -1;
			bool isWindows = OperatingSystem.IsWindows();
            while (true) {
                try {
                    using var stream = new FileStream(
                        filePath.ToString()!,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    if (!isWindows) {
                        long length = stream.Length;
                        if (length != lastLength || length == 0) {
                            lastLength = length;
                            if (sw.ElapsedMilliseconds > maxWaitMs) {
                                logger.LogErrorAndThrow($"Timeout waiting for file `{filePath.ToString()}` to become available.");
                            }
                            Task.Delay(retryDelayMs).Wait();
                            continue;   // stable for one full retry delay = written
                        }
                    }
                    // File is available
                    sw.Stop();
                    return;
                } catch (IOException) {
                    if (sw.ElapsedMilliseconds > maxWaitMs) {
                        logger.LogErrorAndThrow($"Timeout waiting for file `{filePath.ToString()}` to become available.");
                    }
                    Task.Delay(retryDelayMs).Wait();
                }
            }
        }
    }
}
