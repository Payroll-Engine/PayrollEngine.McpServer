using System.Collections.Generic;

namespace PayrollEngine.McpServer.Tools.TenantTools;

/// <summary>Result of a StartTenant operation</summary>
public sealed class StartTenantResult
{
    /// <summary>The tenant identifier</summary>
    public string TenantIdentifier { get; init; }

    /// <summary>The internal tenant id assigned by the backend</summary>
    public int TenantId { get; init; }

    /// <summary>Identifiers of users created during this operation</summary>
    public List<string> CreatedUsers { get; init; } = [];

    /// <summary>Names of divisions created during this operation</summary>
    public List<string> CreatedDivisions { get; init; } = [];

    /// <summary>Error messages collected during this operation</summary>
    public List<string> Errors { get; init; } = [];

    /// <summary>True when no errors occurred</summary>
    public bool Success => Errors.Count == 0;
}