![ndepend-logo](artifacts/images/ndepend-logo.png)  ![MCP-logo](artifacts/images/mcp.png) 
# NDepend.MCP.Server   

> **NDepend.MCP.Server is now part of NDepend.AI**, the NDepend initiative to fix .NET issues at scale with AI. NDepend drives your coding agent (Claude Code, Codex, GitHub Copilot, Junie) to fix hundreds of issues in a single run, then rebuilds, runs the tests, reviews each changed method and re-analyzes the code, so the progress is measured.

Watch the NDepend.AI video:

[![NDepend.AI video](https://img.youtube.com/vi/y402pp2iuQw/maxresdefault.jpg)](https://www.youtube.com/watch?v=y402pp2iuQw)

The **NDepend.MCP.Server** is a Model Context Protocol (MCP) server that delivers deep workspace analysis and advanced code inspection capabilities for .NET projects, powered by the NDepend API.

It turns NDepend's deep code insights into AI-powered actions:

- **Fix** one or multiple complex issues automatically
- **Generate** code queries or rules on demand
- **Interact** with NDepend's rich data using natural language
- **Refactor** several files at once to improve code quality and maintainability

Its key strengths are:

- **Privacy:** NDepend scan runs entirely on-premises - Your code stays private on your machine
- **Reliability:** AI gets reliable, structured data instead of raw file dumps
- **Performance:** Faster responses and lower token costs
- **Objectivity:** No LLM bias—just NDepend's proven analysis

Since the project is open source, you’re free to adapt and extend the server with MCP tools that fit your workflow.

## Features

- **Workspace Overview** - Provide the LLM with **comprehensive**, **detailed** and **unbiased** information about your .NET workspace—obtained quickly- without sending source code to the LLM and without consuming tokens.
- **Source Code Analysis** - Analyze C# source files, quality, and project structure.
- **Dependency Tracking** - Explore project dependencies and generate SVG diagrams of your code.
- **Issues List** - Identify code issues from NDepend rules, Roslyn Analyzers, and ReSharper Code Inspections.
- **Issues Fix** - Provide rich issue data to guide Copilot and the LLM in suggesting proper fixes (suport complex issue fix involving  multiple source files).
- **Generate Web Report** - Generate a full NDepend web report and open it in the browser. See sample reports here: https://www.ndepend.com/sample-reports/
- **Generate CQLinq Query and Rule** - Guide the LLM in creating custom code queries, rules or quality gates based on your requirements.
- **Answer Complex Request About Your Code** - Based on the user request, generate and execute complex queries on-the-fly to inspect: 
  - Source code files
  - Code quality (lines of code, comment, complexity, maintainability index, code coverage)
  - Code dependencies (usage, coupling, cohesion, Clean Architecture)
  - Object Oriented usage (inheritance, interface, encapsulation, instantiation, SOLID principles enforcement)
  - Naming of code elements
  - State mutability (fields, properties, events)
  - Changes since a baseline
  - Issues, rules or quality gates status

Watch this video featuring the NDepend MCP Server, including five complete demos and an in-depth guide to creating your own C# MCP Server: 


[![MCP-logo](artifacts/images/video-cover.jpg)](https://www.youtube.com/watch?v=-MDFXj1rUjU)
  
## Build

- Clone or download this repository into a local folder on your machine (referred to as **%ndp-mcp-svr-dir%**).

- First get the NDepend zipped redistributable from: https://www.ndepend.com/download

- Unzip it in **%ndp-mcp-svr-dir%\artifacts\ndepend**

  ![artifacts-ndepend-install](artifacts/images/artifacts-ndepend.png) 

- **REQUIRED:** Run **VisualNDepend.exe** once to start your evaluation or register a license.
  On Linux/macOS, run `dotnet NDepend.Console.MultiOS.dll /regeval` (or `/reglic <key>`) from the redistributable's `net10.0` directory instead.

- Then rebuild the solution **%ndp-mcp-svr-dir%\NDepend.Mcp.Server.slnx** (.NET 10.0 required)

- DLLs and executables built can be found here:
```
%ndp-mcp-svr-dir%\artifacts\bin\NDepend.Mcp.StdioServer\Debug\net10.0\NDepend.Mcp.StdioServer.exe
```
```
%ndp-mcp-svr-dir%\artifacts\bin\NDepend.Mcp.HttpServer\Debug\net10.0\NDepend.Mcp.HttpServer.exe
```

## MCP Server Registration

The NDepend MCP Server is a **stdio** server: the coding agent launches it and talks to it over standard input/output. Every agent below needs the same two things: a server name and the command that starts the server.

**Server name:** use a name without dots, such as `ndepend`. Codex reads the name as a TOML key (a dot would split it), and the other agents derive their tool ids from it.

**Command:**

| OS | Command |
|---|---|
| Windows | `%ndp-mcp-svr-dir%\artifacts\bin\NDepend.Mcp.StdioServer\Debug\net10.0\NDepend.Mcp.StdioServer.exe` |
| Linux / macOS | `dotnet %ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll` |

No `.exe` apphost is produced on Linux/macOS, so the server is launched through the dotnet host there. The same commands work with the pre-built server shipped in the NDepend redistributable's `net10.0` directory (`<ndepend-install>\net10.0\NDepend.Mcp.StdioServer.exe`, or `dotnet <ndepend-install>/net10.0/NDepend.Mcp.StdioServer.dll`).

**Optional arguments**, appended after the command:
- `--load-ndproj-or-slnx <path>` loads an NDepend project (`.ndproj`) or a solution (`.slnx` / `.sln`) at startup. Without it, the agent calls `ndepend-initialize-from-solution` first.
- `--log-directory <dir>` sets where the logs go (see [Logs](#logs)). Without it, logs go to a `logs` directory next to the server. When the directory cannot be created, the server falls back to the temp directory.

Loading a project and running an analysis can take minutes on a large code base: raise the agent's MCP startup and tool-call timeouts where it has them (see Codex below).

### Claude Code

From the command line (`--scope user` for every project, `--scope project` to share it through a `.mcp.json` at the repository root):

```
claude mcp add ndepend --scope user -- "%ndp-mcp-svr-dir%\artifacts\bin\NDepend.Mcp.StdioServer\Debug\net10.0\NDepend.Mcp.StdioServer.exe"
claude mcp add ndepend --scope user -- dotnet "%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"
```

or in the repository's `.mcp.json`:

```json
{
  "mcpServers": {
    "ndepend": {
      "command": "dotnet",
      "args": ["%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"]
    }
  }
}
```

Check it with `claude mcp list`, or `/mcp` inside a session.

### Codex

From the command line:

```
codex mcp add ndepend -- dotnet "%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"
```

or in `~/.codex/config.toml`. The timeouts matter here: Codex's defaults are shorter than a project load or an analysis.

```toml
[mcp_servers.ndepend]
command = "dotnet"
args = ["%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"]
startup_timeout_sec = 300
tool_timeout_sec = 300
```

On Windows, `command` is the `.exe` path and `args` is empty (or holds the optional arguments). In TOML, write Windows paths with forward slashes or doubled backslashes.

### GitHub Copilot

**Visual Studio 2026 and VS Code.** Both IDEs can register the server from their menus (Visual Studio on the left, VS Code on the right):

![ndepend-logo](artifacts/images/vs-reg-mcp.png)

This adds to the MCP JSON configuration file:

```json
{
  "inputs": [],
  "servers": {
    "ndepend": {
      "type": "stdio",
      "command": "%ndp-mcp-svr-dir%\\artifacts\\bin\\NDepend.Mcp.StdioServer\\Debug\\net10.0\\NDepend.Mcp.StdioServer.exe",
      "env": {}
    }
  }
}
```

On Linux/macOS, use `"command": "dotnet"` and `"args": ["<path to NDepend.Mcp.StdioServer.dll>"]`.

The configuration file can live here:
```
C:\Users\%usr%\AppData\Roaming\Code\User\mcp.json - Global configuration for all solutions
[SOLUTIONDIR]\.vs\mcp.json - Visual Studio only, solution-specific, user-specific
[SOLUTIONDIR]\.mcp.json - Solution-level, can be tracked in source control
[SOLUTIONDIR]\.vscode\mcp.json - Supports both VS Code and Visual Studio
```

If the same MCP server is listed in both a global and a solution-specific configuration file, Copilot treats them as separate servers, which can cause conflicts.

**Copilot CLI.** From the command line:

```
copilot mcp add ndepend -- dotnet "%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"
```

This writes the user configuration `~/.copilot/mcp-config.json`. A workspace can also declare it in `.mcp.json` or `.github/mcp.json`:

```json
{
  "mcpServers": {
    "ndepend": {
      "type": "local",
      "command": "dotnet",
      "args": ["%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"],
      "tools": ["*"]
    }
  }
}
```

Check it with `copilot mcp list`.

### Junie

Junie reads `mcp.json` from `~/.junie/mcp/` (every project) and from `.junie/mcp/` in the project (this project only):

```json
{
  "mcpServers": {
    "ndepend": {
      "command": "dotnet",
      "args": ["%ndp-mcp-svr-dir%/artifacts/bin/NDepend.Mcp.StdioServer/Debug/net10.0/NDepend.Mcp.StdioServer.dll"]
    }
  }
}
```

The Junie CLI can also read additional folders with `--mcp-location=<folder>`.

### Streamable HTTP

`NDepend.Mcp.HttpServer` serves the same tools over Streamable HTTP, at `http://localhost:3001/mcp` by default (`--port <n>` to change it). Start it yourself, then register the URL:

```
claude mcp add --transport http ndepend http://localhost:3001/mcp
codex mcp add ndepend --url http://localhost:3001/mcp
copilot mcp add --transport http ndepend http://localhost:3001/mcp
```

or, in a Visual Studio / VS Code MCP JSON file:

```json
{
  "inputs": [],
  "servers": {
    "ndepend": {
      "type": "http",
      "url": "http://localhost:3001/mcp"
    }
  }
}
```

## Logs

- If an error occurs while using an NDepend MCP tool, check the log files located at:

```
%ndp-mcp-svr-dir%\artifacts\logs
```

## Usage

Once configured, you can ask questions like:

- Initialize + Run Analysis + Web Report:
```
run ndepend analysis
give me analysis details
build an ndepend web report
```

- Code query or rule generation to answer a complex request:
```
list pairs of class that uses each other
which ctor that register to some events in a non disposable class
check for the SOLID open close principle
list methods that are both poorly maintainable and insufficiently tested
find Clean Architecture violations
```

![query-gen](https://www.ndepend.com/Res/mcp-svr/ndepend-mcp-query-gen.gif) 

- Dependencies:
```
which callers and callees for this class
show a call graph of this method
```
![query-gen](https://www.ndepend.com/Res/mcp-svr/ndepend-mcp-dep.gif) 


- Issue list & fix:
```
which new issue in files named like Period
fix the first one
which code sprawl related issues
```

![fix-issue](https://www.ndepend.com/Res/mcp-svr/ndepend-mcp-fix.gif) 

- Metrics:
```
which complex method poorly covered by tests
list less maintainable methods
```

- Quality Gates:
```
which quality gate fail
which quality gate got fixed since the baseline
```

- Rules:
```
which Roslyn Analyzer fail?
which ndepend rule fail with more than 5 issues
```

- Code Search:
```
which class is related to Authentification
```

- Source Files:
```
which source file changed since the baseline
describe changes in the third one
show diff for the 5 last
```

## 14 Exposed MCP Tools

### Initialization Tools

- `ndepend-initialize-from-solution`: Initialize the NDepend analysis session with a .NET solution file. If an NDepend project (.ndproj file) is attached to the solution, use it. Else create an NDepend project side-by-side with the solution and analyze it.
  
  Use when:
  - **REQUIRED FIRST STEP** before using any other NDepend MCP tool
  - Switching to a different solution

### Analysis Tools

- `ndepend-run-analysis`: Run the NDepend analysis on the initialized workspace and update all session data (dependencies, issues, metrics, etc...). Returns analysis data loaded (snapshot date, baseline snapshot date, .ndar analysis result file...).

  Use when:
  - User explicitly requests: "run analysis", "analyze the code", "refresh results"
  - Post-fix verification: "I fixed the issues, check again"
  - After code changes or recompilation

- `ndepend-run-analysis-build-report`: Run the NDepend analysis, update all session data, and build a web report, open it in the browser.

  Use when:
  - User requests: "show me a web report", "generate HTML report", "visualize the analysis"
  - User wants interactive exploration
  - User asks for visual/graphical outputs

### Code Metrics Tools

- `ndepend-search-code-metrics`: Collect and analyze code metrics for quality assessment.

  Use when:
  - Refactoring Candidates: "Which code is poorly maintainable?"
  - Quality Assessment: "How complex is the UserService class?"
  - Test Coverage Analysis: "Which methods need more tests?"
  - Size & Complexity: "How many lines of code in this class?"
  - Comparison & Prioritization: "Compare metrics across services"

  Also use for code search when:
  - Discovery & Navigation: "Which classes are related to authentication?"
  - Pattern & Convention: "Which classes follow the *Manager pattern?"
  - File-Based: "What's in the UserService.cs file?"
  - Change Tracking: "Which methods were added since the baseline?"
  
### Dependency Analysis Tools

- `ndepend-list-dependencies`: Collect and present dependencies for code elements with comprehensive context. Also generate a HTML SVG diagram of the required dependency.

  Use when:
  - Understanding Usage (Callers): "What calls this method?"
  - Understanding Dependencies (Callees): "What does this method call?"
  - Impact Analysis: "If I change this method, what breaks?"
  - Call Chain Exploration: "Show me the full call chain"
  - Refactoring Planning: "Can I safely delete this method?"
  - Architectural Analysis: "Does the UI layer call the database directly?"
  - Debugging & Troubleshooting: "How is this method being invoked?"
  - Code Review: "What's affected by this new method?"

### Issue Management Tools

- `ndepend-list-issues`: List issues scoped by user's request with comprehensive details.

  Use when:
  - Code Quality & Review: "List issues in this source file"
  - Technical Debt Management: "Show me all technical debt items"
  - Compliance & Standards: "List all coding standard violations"
  - Reporting & Analytics: "Generate a report of all issues by severity"

- `ndepend-get-issue-details-to-fix-it`: Retrieve comprehensive diagnostic information for a specific issue.

  Use when:
  - User asks: "How do I fix issue #X?"
  - User requests detailed information about an issue
  - User wants want the Copilot to fix the issue

### Rule Management Tools

- `ndepend-list-all-rules-summary`: Return hierarchical categorized summary of all rules in the current project, with each the number of issues found.

  Use when:
  - Discover available rules before calling `ndepend-list-rules-detailed` or `ndepend-list-issues`
  - Identify valid values for `filterRuleCategories` and `filterRuleIds` before calling `ndepend-list-rules-detailed` or `ndepend-list-issues` 
  - Audit and governance

- `ndepend-list-rules-detailed`: Return paginated list of rules with descriptions and fix guidance.

  Use when:
  - Understanding what a specific rule checks
  - Learning how to fix violations
  - Exploring rule documentation
  - After calling `ndepend-list-all-rules-summary` to get detailed info

### Quality Gate Tools

- `ndepend-list-quality-gates-status`: Return status of each quality gate to assess code quality standards.

  Use when:
  - Status Checks: "Did the code pass quality gates?"
  - Failure Investigation: "Which quality gates are failing?"
  - Specific Gate Queries: "Did we pass the test coverage gate?"
  - Pre-Merge Validation: "Can I merge this PR?"
  - Threshold Details: "What are the quality gate thresholds?"
  - Comparison & Trends: "How does this compare to the baseline?"
  - Team Standards: "What quality gates do we have?"
  - Debugging Build Issues: "Why is the build blocked?"

### Source Code Tools

- `ndepend-get-sources`: Retrieve source code as raw text from NDepend.

  Use when:
  - Viewing Code: "Show me the code for UserService.Authenticate"
  - Understanding Implementation: "How is this method implemented?"
  - Code Review: "Review the code for this method"
  - Debugging & Troubleshooting: "Show me where the error occurs"
  - Learning & Examples: "Show me an example of dependency injection"
  - Refactoring Analysis: "Show me the code before I refactor it"
  - Following Up on Other Tools: "Show me the code with low maintainability"

 - `ndepend-diff-sources`: Invokes NDepend's diff action to compare a source file's current content against its baseline using the configured source compare tool.

   Use when:
   - "Show me the before and after"
   - "compare with the old version"
   - "What's changed in this method?"
   - "show differences since baseline"

### Code Query Tools

- `ndepend-gen-code-query-and-rule`: Provide prompts to the LLM to make it an expert in code queries and rule generation. The LLM first identifies the code querying features it needs based on the user’s request, then it calls this tool to obtain a prompt for each feature choosen.

  Use when:
  - **ALWAYS** when user requests an NDepend code query or code rule
  - Complex codebase analysis that can't be handled by other tools

  Available code query kind prompts:
  - `code-query-list`: Returns collection of code elements with properties
  - `code-rule`: Quality rule identifying violations
  - `quality-gate`: Quality criterion with WARN/FAIL thresholds
  - `querying-issue-rule`: Selects issues and rules
  - `trend-metric`: Scalar value stored over time to plot chart
  - `code-query-scalar`: Returns single numeric value

  Available features prompts:
  - `code-query-essential` (ALWAYS REQUIRED)
  - `line-of-code`, `maintainability`, `complexity`, `coverage`, `comment`
  - `usage-dependency`, `parent-children-relationship`, `inheritance-and-base-class`, `interface`
  - `solid-principles`, `clean-architecture`, `encapsulation-and-visibility`, `state-mutability`
  - `diff-since-baseline`, `naming`, `attribute`, `source-file-declaration`
  - `event-pattern`, `constructor-instantiation`

- `ndepend-run-code-query-or-rule`: Compile and execute code query or rule against the session's analysis result and baseline.

  Use when:
  - After generating a query
  - User requests execution
  - Complex codebase analysis requiring custom queries

  Also use to check whether a generated query compiles (parameters compileOnly set to `True`). If the query fails to compile, an `McpException` is thrown containing the compilation errors in its message, allowing the LLM to correct them.

## Requirements

- Windows, Linux or MacOS
- .NET 10.0 or higher
- ModelContextProtocol NuGet package (loaded at build time)
- NDepend version 2026.1.3 or upper redistributable (evaluation or license)

   ### Note

   This project is open source, but it relies on the NDepend.API, which requires a separate license (either a free evaluation or a paid license) to use.
   - Evaluation: https://www.ndepend.com/download
   - License: https://www.ndepend.com/purchase

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## License

This project is open source under the MIT License. 
It depends on the proprietary NDepend.API (https://www.ndepend.com/) which must be obtained separately (evaluation or license).
No rights to the NDepend API are granted by this project.