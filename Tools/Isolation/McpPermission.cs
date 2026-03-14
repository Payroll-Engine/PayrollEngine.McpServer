namespace PayrollEngine.McpServer.Tools.Isolation;

/// <summary>Access level granted for a role in this MCP Server deployment.
/// Ordered: None &lt; Read &lt; Full — use &gt;= comparisons for minimum permission checks.</summary>
public enum McpPermission
{
    /// <summary>Role tools are not registered — invisible to the AI agent.</summary>
    None,

    /// <summary>Read and query tools only. Write tools are excluded even if available.</summary>
    Read,

    /// <summary>Read and write tools. Required for mutation operations such as start_tenant.</summary>
    Full
}
