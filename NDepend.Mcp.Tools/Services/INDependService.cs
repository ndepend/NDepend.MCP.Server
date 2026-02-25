using NDepend.Analysis;

namespace NDepend.Mcp.Services {
    public interface INDependService {
        bool InitializeFromProject(string projectFilePathStr, ILogger logger, Action<int>? reportProgressProc, out Session session);
        bool InitializeFromAnalysisResult(IAnalysisResult analysisResult, ILogger logger, Action<int>? reportProgressProc, out Session session);
        bool IsInitialized(out Session session);
        void StopWatchingForNewAnalysisResult();
    }
}
