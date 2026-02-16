using NDepend.Analysis;

namespace NDepend.Mcp.Services {
    public interface INDependService {
        bool InitializeFromProject(string projectFilePathStr, ILogger logger, Action<int>? reportProgressProc = null);
        bool InitializeFromAnalysisResult(IAnalysisResult analysisResult, ILogger logger, Action<int>? reportProgressProc = null);
        bool IsInitialized(out Session session);
        void StopWatchingForNewAnalysisResult();
    }
}
