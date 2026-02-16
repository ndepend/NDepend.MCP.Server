
namespace NDepend.Mcp.Tools.Common {
    internal static class Constants {
        internal const string NDEPEND_MCP_SERVER = "NDepend.Mcp.Server";
        internal const string TOOL_NAME_PREFIX = "ndepend-";
        internal const string INITIALIZE_FROM_SOLUTION_TOOL_NAME = TOOL_NAME_PREFIX + "initialize-from-solution";
        internal const string PROMPT_CALL_INITIALIZE = 
$"You MUST initialize the `{NDEPEND_MCP_SERVER}` by calling the tool `{INITIALIZE_FROM_SOLUTION_TOOL_NAME}` before using this tool.";

        
        internal const string NOT_AVAILABLE = "not available";


    }
}
