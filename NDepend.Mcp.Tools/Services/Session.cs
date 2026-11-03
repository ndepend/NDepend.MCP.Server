using NDepend.Analysis;
using NDepend.CodeModel;
using NDepend.Helpers;
using NDepend.Issue;
using NDepend.Mcp.Helpers;
using NDepend.Project;

namespace NDepend.Mcp.Services {
    using IssuesResult = (IIssuesSet IssuesSet, IIssuesSetDiff IssuesSetDiff, ICodeBaseView JustMyCode);

    public sealed class Session : IDisposable {

        internal Session(
                IAnalysisResult analysisResult, 
                IAnalysisResult baselineResult, 
                ILogger logger, 
                NdarWatcher ndarWatcher) {
            Project = analysisResult.AnalysisResultRef.Project;
            AnalysisResult = analysisResult;
            BaselineResult = baselineResult;
            m_Logger = logger;
            m_NdarWatcher = ndarWatcher;

            // Expensive compare-context and issue computations run asynchronously.
            // This makes use of the few seconds the AI agent spends chaining a tool
            // after Initialize, improving overall responsiveness.
            m_InitCompareContextTask = ComputeCompareContextAsync();
            m_InitIssuesTask = m_InitCompareContextTask.ContinueWith(ComputeIssuesAsync).Unwrap();
        }

        public IProject Project { get; }
        public IAnalysisResult AnalysisResult { get; }
        public IAnalysisResult BaselineResult { get; }

        private readonly ILogger m_Logger;
        private readonly IDisposable m_NdarWatcher;

        public void Dispose() {
            m_NdarWatcher.Dispose();
            // Do NOT dispose the two init tasks: they can still be RUNNING when a new analysis result
            // triggers the NdarWatcher re-initialization that disposes the previous session — and
            // disposing a non-completed Task throws InvalidOperationException on the watcher thread,
            // killing the process. Tasks hold no resource that needs disposing here anyway.
        }


        // public (not internal): the NDepend.AI fix-issues orchestrator manages its analysis snapshots
        // explicitly and must stop this watcher (meant for the MCP server process) from re-initializing
        // sessions on every fresh analysis result it persists.
        public void StopWatchingForNewAnalysisResult() {
            m_NdarWatcher.Dispose();
        }

        // Eventually wait for an async computation to end with GetAwaiter()
        public ICompareContext CompareContext => m_InitCompareContextTask.GetAwaiter().GetResult();
        public ICodeBaseView JustMyCode => m_InitIssuesTask.GetAwaiter().GetResult().JustMyCode;
        public IIssuesSet IssuesSet => m_InitIssuesTask.GetAwaiter().GetResult().IssuesSet;
        public IIssuesSetDiff IssuesSetDiff => m_InitIssuesTask.GetAwaiter().GetResult().IssuesSetDiff;

        private readonly Task<ICompareContext> m_InitCompareContextTask;
        private readonly Task<IssuesResult> m_InitIssuesTask;
        
        private async Task<ICompareContext> ComputeCompareContextAsync() {
            return await Task.Run(() => {
                m_Logger.LogInformation("Start computing compare context asynchronously.");
                var cc = AnalysisResult.CodeBase.CreateCompareContextWithOlder(BaselineResult.CodeBase);
                m_Logger.LogInformation("Complete computing compare context asynchronously.");
                return cc;
            });
        }
        private async Task<IssuesResult> ComputeIssuesAsync(Task<ICompareContext> taskIn) {
            return await Task.Run(() => {
                m_Logger.LogInformation("Start computing issues asynchronously.");
                IIssuesSetDiff issuesSetDiff = AnalysisResult.ComputeIssuesDiff(
                    taskIn.Result,
                    AnalysisResult.IssuesImported,
                    BaselineResult.IssuesImported,
                    1.ToMinutes(),
                    IssueReferenceRecord.Yes,
                    out ICodeBaseView justMyCode,
                    out _,
                    out _);
                IIssuesSet issuesSet = issuesSetDiff.NewerIssuesSet;
                m_Logger.LogInformation("Complete computing issues asynchronously.");
                return (issuesSet, issuesSetDiff, justMyCode);
            });
        }

       
    }
   
    
    
}
