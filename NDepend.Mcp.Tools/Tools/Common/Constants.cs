
namespace NDepend.Mcp.Tools.Common {
    public static class Constants {
        internal const string NDEPEND_MCP_SERVER = "NDepend.Mcp.Server";
        internal const string TOOL_NAME_PREFIX = "ndepend-";
        public const string INITIALIZE_FROM_SOLUTION_TOOL_NAME = TOOL_NAME_PREFIX + "initialize-from-solution";
        internal const string PROMPT_CALL_INITIALIZE = 
$"**YOU MUST** initialize the `{NDEPEND_MCP_SERVER}` by calling the tool `{INITIALIZE_FROM_SOLUTION_TOOL_NAME}` before using this tool.";

        
        internal const string NOT_AVAILABLE = "not available";

        public const string MCP_ARG_LOAD_NDEPEND_PROJECT_OR_SOLUTION = "--load-ndproj-or-slnx";
        public const string MCP_ARG_LOG_DIRECTORY = "--log-directory";


    }
}
