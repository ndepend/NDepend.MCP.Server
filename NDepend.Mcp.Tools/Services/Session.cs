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

        internal IProject Project { get; }
        internal IAnalysisResult AnalysisResult { get; }
        internal IAnalysisResult BaselineResult { get; }

        private readonly ILogger m_Logger;
        private readonly IDisposable m_NdarWatcher;

        public void Dispose() {
            m_NdarWatcher.Dispose();
            m_InitCompareContextTask.Dispose();
            m_InitIssuesTask.Dispose();
        }


        internal void StopWatchingForNewAnalysisResult() {
            m_NdarWatcher.Dispose();
        }

        // Eventually wait for an async computation to end with GetAwaiter()
        internal ICompareContext CompareContext => m_InitCompareContextTask.GetAwaiter().GetResult();
        internal ICodeBaseView JustMyCode => m_InitIssuesTask.GetAwaiter().GetResult().JustMyCode;
        internal IIssuesSet IssuesSet => m_InitIssuesTask.GetAwaiter().GetResult().IssuesSet;
        internal IIssuesSetDiff IssuesSetDiff => m_InitIssuesTask.GetAwaiter().GetResult().IssuesSetDiff;

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
