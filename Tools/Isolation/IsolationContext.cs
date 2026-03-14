using System;

namespace PayrollEngine.McpServer.Tools.Isolation;

/// <summary>Holds the active data isolation and permission configuration for the MCP server session.
/// Registered as a singleton; populated from McpServer appsettings on startup.</summary>
public sealed class IsolationContext
{
    /// <summary>Active isolation level. Defaults to MultiTenant.</summary>
    public IsolationLevel Level { get; init; } = IsolationLevel.MultiTenant;

    /// <summary>Tenant identifier used when Level is Tenant.
    /// All tool calls are automatically scoped to this tenant.</summary>
    public string TenantIdentifier { get; init; }

    /// <summary>Role permission levels for this deployment.
    /// Defaults to Full for all roles when absent from configuration.</summary>
    public McpPermissions Permissions { get; init; } = new();

    /// <summary>Validates the isolation context at startup.
    /// Throws if configuration is incomplete or uses an invalid combination.</summary>
    public void Validate()
    {
        if (Level == IsolationLevel.Tenant && string.IsNullOrWhiteSpace(TenantIdentifier))
        {
            throw new InvalidOperationException(
                "McpServer:TenantIdentifier is required when IsolationLevel is Tenant.");
        }

        if (Level is IsolationLevel.Division or IsolationLevel.Employee)
        {
            throw new NotSupportedException(
                $"Isolation level '{Level}' is architecturally reserved and will be available in a future release.");
        }
    }
}
