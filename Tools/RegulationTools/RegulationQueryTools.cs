using System;
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
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var regulations = await RegulationService().QueryAsync<Regulation>(context);
            return JsonSerializer.Serialize(regulations);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>Get a regulation by name within a tenant</summary>
    [McpServerTool(Name = "get_regulation"), Description("Get a regulation by name within a tenant")]
    public async Task<string> GetRegulationAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var regulation = await RegulationService().GetAsync<Regulation>(context, regulationName);
            return JsonSerializer.Serialize(regulation);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>List all wage types of a regulation within a tenant</summary>
    [McpServerTool(Name = "list_wage_types"), Description(
        "List all wage types of a regulation within a tenant")]
    public async Task<string> ListWageTypesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName)
    {
        try
        {
            var context = await ResolveRegulationContextAsync(tenantIdentifier, regulationName);
            var wageTypes = await WageTypeService().QueryAsync<WageType>(context);
            return JsonSerializer.Serialize(wageTypes);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>List all lookups of a regulation within a tenant</summary>
    [McpServerTool(Name = "list_lookups"), Description(
        "List all lookups of a regulation within a tenant")]
    public async Task<string> ListLookupsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName)
    {
        try
        {
            var context = await ResolveRegulationContextAsync(tenantIdentifier, regulationName);
            var lookups = await LookupService().QueryAsync<Lookup>(context);
            return JsonSerializer.Serialize(lookups);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>List all values of a lookup within a regulation</summary>
    [McpServerTool(Name = "list_lookup_values"), Description(
        "List all values of a lookup within a regulation. " +
        "Returns key-value pairs with optional range and culture support.")]
    public async Task<string> ListLookupValuesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The regulation name")] string regulationName,
        [Description("The lookup name")] string lookupName)
    {
        try
        {
            var context = await ResolveLookupContextAsync(tenantIdentifier, regulationName, lookupName);
            var values = await LookupValueService().QueryAsync<LookupValue>(context);
            return JsonSerializer.Serialize(values);
        }
        catch (Exception ex) { return Error(ex); }
    }
}
