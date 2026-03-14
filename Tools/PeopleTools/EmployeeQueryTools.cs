using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;

namespace PayrollEngine.McpServer.Tools.PeopleTools;

/// <summary>MCP tools for employee queries</summary>
[McpServerToolType]
public sealed class EmployeeQueryTools(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    /// <summary>List all employees of a tenant</summary>
    [McpServerTool(Name = "list_employees"), Description("List all employees of a tenant")]
    public async Task<string> ListEmployeesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        try
        {
            var context = await ResolveTenantContextAsync(tenantIdentifier);
            var employees = await EmployeeService().QueryAsync<Employee>(context);
            return JsonSerializer.Serialize(employees);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>Get an employee by identifier within a tenant</summary>
    [McpServerTool(Name = "get_employee"), Description("Get an employee by identifier within a tenant")]
    public async Task<string> GetEmployeeAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The employee identifier")] string employeeIdentifier)
    {
        try
        {
            var (_, employee) = await ResolveEmployeeAsync(tenantIdentifier, employeeIdentifier);
            return JsonSerializer.Serialize(employee);
        }
        catch (Exception ex) { return Error(ex); }
    }

    /// <summary>Get a single attribute value of an employee</summary>
    [McpServerTool(Name = "get_employee_attribute"), Description("Get a single attribute value of an employee")]
    public async Task<string> GetEmployeeAttributeAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The employee identifier")] string employeeIdentifier,
        [Description("The attribute name")] string attributeName)
    {
        try
        {
            var (context, employee) = await ResolveEmployeeAsync(tenantIdentifier, employeeIdentifier);
            return await EmployeeService().GetAttributeAsync(context, employee.Id, attributeName);
        }
        catch (Exception ex) { return Error(ex); }
    }
}
