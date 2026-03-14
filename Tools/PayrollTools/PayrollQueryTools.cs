using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;

namespace PayrollEngine.McpServer.Tools.PayrollTools;

/// <summary>MCP tools for payroll, payrun and job queries</summary>
[McpServerToolType]
public sealed class PayrollQueryTools(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    /// <summary>List all payrolls of a tenant</summary>
    [McpServerTool(Name = "list_payrolls"), Description("List all payrolls of a tenant")]
    public async Task<string> ListPayrollsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var payrolls = await PayrollService().QueryAsync<Payroll>(context);
        return JsonSerializer.Serialize(payrolls);
    }

    /// <summary>Get a payroll by name within a tenant</summary>
    [McpServerTool(Name = "get_payroll"), Description("Get a payroll by name within a tenant")]
    public async Task<string> GetPayrollAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The payroll name")] string payrollName)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var payroll = await PayrollService().GetAsync<Payroll>(context, payrollName);
        return JsonSerializer.Serialize(payroll);
    }

    /// <summary>List all payruns of a tenant</summary>
    [McpServerTool(Name = "list_payruns"), Description("List all payruns of a tenant")]
    public async Task<string> ListPayrunsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var payruns = await PayrunService().QueryAsync<Payrun>(context);
        return JsonSerializer.Serialize(payruns);
    }

    /// <summary>List all payrun jobs of a tenant, ordered by creation date descending</summary>
    [McpServerTool(Name = "list_payrun_jobs"), Description(
        "List all payrun jobs of a tenant, ordered by creation date descending")]
    public async Task<string> ListPayrunJobsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var query = new Query { OrderBy = "created desc" };
        var jobs = await PayrunJobService().QueryAsync<PayrunJob>(context, query);
        return JsonSerializer.Serialize(jobs);
    }

    /// <summary>List all effective wage types of a payroll, merged across all regulation layers</summary>
    [McpServerTool(Name = "list_payroll_wage_types"), Description(
        "List all effective wage types of a payroll (merged across all regulation layers)")]
    public async Task<string> ListPayrollWageTypesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The payroll name")] string payrollName)
    {
        var context = await ResolvePayrollContextAsync(tenantIdentifier, payrollName);
        var wageTypes = await PayrollService().GetWageTypesAsync<WageType>(context);
        return JsonSerializer.Serialize(wageTypes);
    }
}
