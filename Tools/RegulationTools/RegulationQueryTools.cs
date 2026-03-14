using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;

namespace PayrollEngine.McpServer.Tools.RegulationTools;

/// <summary>MCP tools for regulation queries</summary>
[McpServerToolType]
public sealed class RegulationQueryTools(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    /// <summary>List all regulations of a tenant</summary>
    [McpServerTool(Name = "list_regulations"), Description("List all regulations of a tenant")]
    public async Task<string> ListRegulationsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var regulations = await RegulationService().QueryAsync<Regulation>(context);
        return JsonSerializer.Serialize(regulations);
    }

    /// <summary>Get a regulation by name within a tenant</summary>
    [McpServerTool(Name = "get_regulation"), Description("Get a regulation by name within a tenant")]
    public async Task<string> GetRegulationAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var regulation = await RegulationService().GetAsync<Regulation>(context, regulationName);
        return JsonSerializer.Serialize(regulation);
    }

    /// <summary>List all wage types of a regulation within a tenant</summary>
    [McpServerTool(Name = "list_wage_types"), Description(
        "List all wage types of a regulation within a tenant")]
    public async Task<string> ListWageTypesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName)
    {
        var context = await ResolveRegulationContextAsync(tenantIdentifier, regulationName);
        var wageTypes = await WageTypeService().QueryAsync<WageType>(context);
        return JsonSerializer.Serialize(wageTypes);
    }

    /// <summary>List all lookups of a regulation within a tenant</summary>
    [McpServerTool(Name = "list_lookups"), Description(
        "List all lookups of a regulation within a tenant")]
    public async Task<string> ListLookupsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName)
    {
        var context = await ResolveRegulationContextAsync(tenantIdentifier, regulationName);
        var lookups = await LookupService().QueryAsync<Lookup>(context);
        return JsonSerializer.Serialize(lookups);
    }
}
