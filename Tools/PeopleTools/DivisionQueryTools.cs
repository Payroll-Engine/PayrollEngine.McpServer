using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.McpServer.Tools.Isolation;

namespace PayrollEngine.McpServer.Tools.PeopleTools;

/// <summary>MCP tools for division queries</summary>
[McpServerToolType]
[ToolRole(McpRole.HR)]
// ReSharper disable once UnusedType.Global
public sealed class DivisionQueryTools(PayrollHttpClient httpClient, IsolationContext isolation) : ToolBase(httpClient, isolation)
{
    /// <summary>List all divisions of a tenant</summary>
    [McpServerTool(Name = "list_divisions"), Description("List all divisions of a tenant")]
    public async Task<string> ListDivisionsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var divisions = await DivisionService().QueryAsync<Division>(context, IsolatedDivisionQuery());
            return JsonSerializer.Serialize(divisions);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>Get a division by name within a tenant</summary>
    [McpServerTool(Name = "get_division"), Description("Get a division by name within a tenant")]
    public async Task<string> GetDivisionAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The division name")] string divisionName)
    {
        try
        {
            var (_, division) = await ResolveDivisionAsync(tenantIdentifier, divisionName);
            return JsonSerializer.Serialize(division);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>Get a single attribute value of a division</summary>
    [McpServerTool(Name = "get_division_attribute"), Description("Get a single attribute value of a division")]
    public async Task<string> GetDivisionAttributeAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The division name")] string divisionName,
        [Description("The attribute name")] string attributeName)
    {
        try
        {
            var (context, division) = await ResolveDivisionAsync(tenantIdentifier, divisionName);
            return await DivisionService().GetAttributeAsync(context, division.Id, attributeName);
        }
        catch (Exception ex) { return Error(ex); }
    }
}
