using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.McpServer.Tools.Isolation;

namespace PayrollEngine.McpServer.Tools.PayrollTools;

/// <summary>MCP tools for payroll, payrun and job queries</summary>
[McpServerToolType]
[ToolRole(McpRole.Payroll)]
// ReSharper disable once UnusedType.Global
public sealed class PayrollQueryTools(PayrollHttpClient httpClient, IsolationContext isolation) : ToolBase(httpClient, isolation)
{
    /// <summary>List all payrolls of a tenant</summary>
    [McpServerTool(Name = "list_payrolls"), Description("List all payrolls of a tenant")]
    public async Task<string> ListPayrollsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var payrolls = await PayrollService().QueryAsync<Payroll>(context, ActiveQuery());
            return JsonSerializer.Serialize(payrolls);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>Get a payroll by name within a tenant</summary>
    [McpServerTool(Name = "get_payroll"), Description("Get a payroll by name within a tenant")]
    public async Task<string> GetPayrollAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The payroll name")] string payrollName)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var payroll = await PayrollService().GetAsync<Payroll>(context, payrollName);
            return JsonSerializer.Serialize(payroll);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>List all payruns of a tenant</summary>
    [McpServerTool(Name = "list_payruns"), Description("List all payruns of a tenant")]
    public async Task<string> ListPayrunsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var payruns = await PayrunService().QueryAsync<Payrun>(context, ActiveQuery());
            return JsonSerializer.Serialize(payruns);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>List all payrun jobs of a tenant, ordered by creation date descending</summary>
    [McpServerTool(Name = "list_payrun_jobs"), Description(
        "List all payrun jobs of a tenant, ordered by creation date descending")]
    public async Task<string> ListPayrunJobsAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var query = ActiveQuery(orderBy: "created desc");
            var jobs = await PayrunJobService().QueryAsync<PayrunJob>(context, query);
            return JsonSerializer.Serialize(jobs);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>List all effective wage types of a payroll merged across all regulation layers</summary>
    [McpServerTool(Name = "list_payroll_wage_types"), Description(
        "List all effective wage types of a payroll (merged across all regulation layers)")]
    public async Task<string> ListPayrollWageTypesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The payroll name")] string payrollName)
    {
        try
        {
            var context = await ResolvePayrollContextAsync(tenantIdentifier, payrollName);
            var wageTypes = await PayrollService().GetWageTypesAsync<WageType>(context);
            return JsonSerializer.Serialize(wageTypes);
        }
        catch (Exception ex) { return Error(ex); }
    }
}
