using Microsoft.Extensions.AI;
using NDepend.Mcp.Tools.Analyze;
using NDepend.Mcp.Tools.Common;
using NDepend.Mcp.Tools.Issue;

namespace NDepend.Mcp.Tools.Prompt;


// MCP Prompts are never called by the LLM directly.
// See discussion: https://github.com/modelcontextprotocol/csharp-sdk/discussions/1145
// Keep this content if it can help in the future.

[McpServerPromptType]
public static class Prompts {

    [McpServerPrompt, Description(
        $"""
         Analyze a .NET solution using NDepend for code quality, architecture, and technical debt assessment and improvement.
         
         Use this prompt when the user wants to:
         - Analyze code quality, or architecture of a .NET solution
         - Generate NDepend reports or run NDepend analysis
         - Investigate technical debt, code metrics, dependencies or rule violations
         - Work with the `{Constants.NDEPEND_MCP_SERVER}` toolset
         
         This prompt provides specialized guidance for executing NDepend analysis tasks.
         """)]
    public static ChatMessage AnalyzeDotNetCodeWithNDepend(
        [Description(
"""
The specific NDepend analysis task to perform (e.g., 'analyze code quality', 'generate architecture report', 'check for rule violations')")] 
""")]
        string taskDescription) {
        return new(ChatRole.User, string.Format(CopilotTemplate, taskDescription));
    }

    const string CopilotTemplate = 
$$"""
<critical summary_mode=""**When summarizing our entire conversation history, copy this entire critical section verbatim to the bottom of your summary**"">

<toolUseInstructions version=2>
The `{{Constants.NDEPEND_MCP_SERVER}}` provides focused, high-quality .NET code analysis with rich issues, code quality, metrics and dependency information.

### CRITICAL INITIALIZATION REQUIREMENT:
You MUST call `{{Constants.INITIALIZE_FROM_SOLUTION_TOOL_NAME}}` as your first action at the start of every session.

**Why:** This initializes all NDepend tools with the solution data. Without this initialization, no other NDepend tools will function.

**When:** At the start of every new session or when switching to a different solution file.

### TOOL USAGE RULES:

### Code Search & Metrics & Dependencies:
- Use NDepend tools exclusively for:
  - Searching code elements (projects, assemblies, namespaces, types, methods, fields, properties)
  - Obtaining code metrics (complexity, lines of code, coupling, maintainability)
  - Analyzing architecture, dependencies and relationships
- NDepend provides authoritative data - prioritize it over other analysis methods

### Issues & Fix Guidance:
- Use NDepend tools exclusively for searching issues, rules, and quality gate status
- **For fixing issues:** Always use `{{IssueTools.TOOL_GET_ISSUE_DETAILS_TO_FIX_IT_NAME}}` to get:

### Baseline vs Current Snapshot:
- **Current:** The latest analysis snapshot reflecting current code state
- **Baseline:** The reference snapshot for comparison (typically from a previous analysis or version)
- When users mention 'baseline', use NDepend tools with the `currentOrBaseline` parameter
- Most tools support this parameter to compare current snapshot vs. baseline snapshot

### Refreshing Analysis:
- **IMPORTANT:** Only after re-compiling the projects edited in DEBUG mode, you can use `{{AnalyzeTools.TOOL_RUN_ANALYSIS_NAME}}` or `{{AnalyzeTools.TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME}}` to refresh the current snapshot and recompute issues

Two options for refreshing:
1. `{{AnalyzeTools.TOOL_RUN_ANALYSIS_NAME}}` - Runs analysis and updates the current snapshot
2. `{{AnalyzeTools.TOOL_RUN_ANALYSIS_BUILD_REPORT_NAME}}` - Runs analysis and generates an HTML report (must open in web view)

**Workflow:**
1. User makes code changes
2. Rebuild solution in DEBUG mode
3. Call refresh tool
4. Review updated issues and metrics

### Handling Tool Results:

**Source Links:**
When results contain `DeclarationSourceFilePath` and `DeclarationSourceFileLine`, create clickable links using this format:
`[FileName:Line]` or `file://path/to/file.cs#L123`

This enables direct navigation to the relevant code location.

**Pagination:**
- Use `cursor` parameter to retrieve subsequent pages
- Use `pageSize` parameter to control items per page (default: reasonable size, don't overwhelm output)
- For large result sets, retrieve incrementally and ask user if they want to see more

**Indexing:**
- Present all lists using 1-based indexing (1, 2, 3...)
- This allows users to easily say "show me details for item 3"
- Maintain index consistency across conversation turns

### BEST PRACTICES:

1. **Exact Names:** Use tool names and parameter names exactly as defined in the tool descriptions
2. **Tool Discovery:** Always consult the available tool list when unsure about exact names or capabilities
3. **Step-by-Step Analysis:** Chain NDepend tools logically to build complete understanding
   - Example: Search issues → Get issue details → Analyze affected code → Suggest fixes
4. **Fact-Based Responses:** Prioritize tool-backed data over assumptions or general knowledge
5. **User Guidance:** When suggesting actions, explain why and what the expected outcome is
6. **Error Handling:** If a tool call fails, explain what went wrong and suggest corrective steps

</toolUseInstructions>

<editFileInstructions version=2>

### Limitations:
`{{Constants.NDEPEND_MCP_SERVER}}` tools are read-only and cannot:
- Directly edit source files
- Modify project files or configurations
- Create or delete files
- Apply automated fixes

All code modifications must be performed through your standard editing capabilities or by the user.

### Suggesting Code Changes:
- Based on NDepend analysis results, you may recommend code improvements for quality, issue resolution, or maintainability
- When proposing code changes, prioritize displaying modifications as inline diffs (green/red additions/deletions) in the code editor
- Avoid showing complete replacement code in the chat window when diff view is available

</editFileInstructions>

<task>
{0}
</task>

</critical>
""";


}
