using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service;

namespace PayrollEngine.McpServer.Tools.TenantTools;

/// <summary>MCP tools for tenant queries</summary>
[McpServerToolType]
public sealed class TenantQueryTools(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    private static readonly RootServiceContext Context = new();

    /// <summary>List all tenants</summary>
    [McpServerTool(Name = "list_tenants"), Description("List all tenants in the Payroll Engine")]
    public async Task<string> ListTenantsAsync()
    {
        var tenants = await TenantService().QueryAsync<Tenant>(Context);
        return JsonSerializer.Serialize(tenants);
    }

    /// <summary>Get a tenant by identifier</summary>
    [McpServerTool(Name = "get_tenant"), Description("Get a tenant by its identifier")]
    public async Task<string> GetTenantAsync(
        [Description("The unique tenant identifier")] string identifier)
    {
        var tenant = await TenantService().GetAsync<Tenant>(Context, identifier);
        return JsonSerializer.Serialize(tenant);
    }

    /// <summary>Get a single tenant attribute by name</summary>
    [McpServerTool(Name = "get_tenant_attribute"), Description(
        "Get a single attribute value of a tenant by attribute name")]
    public async Task<string> GetTenantAttributeAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The attribute name")] string attributeName)
    {
        var tenant = await ResolveTenantAsync(tenantIdentifier);
        return await TenantService().GetAttributeAsync(Context, tenant.Id, attributeName);
    }
}
