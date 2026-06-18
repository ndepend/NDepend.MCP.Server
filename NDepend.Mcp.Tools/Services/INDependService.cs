using NDepend.Analysis;

namespace NDepend.Mcp.Services {
    public interface INDependService {

        // Resolves the .ndproj to load from an .ndproj OR .sln/.slnx path
        // (creating an .ndproj side-by-side with the solution when needed),
        // then loads/analyzes it and returns the Session
        Task<Session> InitializeFromProjectOrSolutionAsync(string slnOrNdprojFilePath, ILogger logger, Action<int>? reportProgressProc);
        bool InitializeFromAnalysisResult(IAnalysisResult analysisResult, ILogger logger, Action<int>? reportProgressProc, out Session session);
        bool IsInitialized(out Session session);
        void StopWatchingForNewAnalysisResult();
    }
}
