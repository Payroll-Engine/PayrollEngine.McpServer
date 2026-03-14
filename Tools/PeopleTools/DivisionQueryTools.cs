using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;

namespace PayrollEngine.McpServer.Tools.PeopleTools;

/// <summary>MCP tools for division queries</summary>
[McpServerToolType]
public sealed class DivisionQueryTools(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    /// <summary>List all divisions of a tenant</summary>
    [McpServerTool(Name = "list_divisions"), Description("List all divisions of a tenant")]
    public async Task<string> ListDivisionsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var divisions = await DivisionService().QueryAsync<Division>(context);
        return JsonSerializer.Serialize(divisions);
    }

    /// <summary>Get a division by name within a tenant</summary>
    [McpServerTool(Name = "get_division"), Description("Get a division by name within a tenant")]
    public async Task<string> GetDivisionAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The division name")] string divisionName)
    {
        var (_, division) = await ResolveDivisionAsync(tenantIdentifier, divisionName);
        return JsonSerializer.Serialize(division);
    }

    /// <summary>Get a single attribute value of a division</summary>
    [McpServerTool(Name = "get_division_attribute"), Description("Get a single attribute value of a division")]
    public async Task<string> GetDivisionAttributeAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The division name")] string divisionName,
        [Description("The attribute name")] string attributeName)
    {
        var (context, division) = await ResolveDivisionAsync(tenantIdentifier, divisionName);
        return await DivisionService().GetAttributeAsync(context, division.Id, attributeName);
    }
}
