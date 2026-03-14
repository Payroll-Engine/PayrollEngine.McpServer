using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.McpServer.Tools.Isolation;

namespace PayrollEngine.McpServer.Tools.PayrollTools;

/// <summary>MCP tools for time-based case value queries</summary>
[McpServerToolType]
[ToolRole(McpRole.Payroll)]
// ReSharper disable once UnusedType.Global
public sealed class CaseValueTimeTools(PayrollHttpClient httpClient, IsolationContext isolation) : ToolBase(httpClient, isolation)
{
    /// <summary>Get case values at a specific point in time</summary>
    [McpServerTool(Name = "get_case_time_values"), Description(
        "Get case values (salary, address, bank account, etc.) valid at a specific point in time. " +
        "Supports three temporal perspectives: " +
        "(1) Historical — set valueDate and evaluationDate to the same date to see data 'as of that date', excluding later corrections. " +
        "(2) Current knowledge — set only valueDate; evaluationDate defaults to today, so retroactive corrections are visible. " +
        "(3) Forecast — set a forecast name and evaluationDate = valueDate to include planned future changes. " +
        "Omit employeeIdentifier to query all employees (Employee caseType only).")]
    public async Task<string> GetCaseTimeValuesAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The payroll name")] string payrollName,
        [Description("Case type: Employee, Company, or Global (default: Employee)")] string caseType = "Employee",
        [Description("Comma-separated list of case field names, e.g. 'Salary,City,IBAN' (omit for all fields)")] string caseFieldNames = null,
        [Description("The value date (ISO 8601): which value is valid on this date, e.g. '2026-01-01' (default: today)")] string valueDate = null,
        [Description("The evaluation date (ISO 8601): knowledge perspective. Equal to valueDate = historical view; today = current knowledge (default: today)")] string evaluationDate = null,
        [Description("Employee identifier — required for a single employee; omit for all employees (Employee caseType only)")] string employeeIdentifier = null,
        [Description("Forecast name to include planned values (set evaluationDate = valueDate when using forecast)")] string forecast = null)
    {
        try
        {
            // resolve payroll context
            var payrollContext = await ResolvePayrollContextAsync(tenantIdentifier, payrollName);

            // parse caseType
            if (!Enum.TryParse<CaseType>(caseType, ignoreCase: true, out var parsedCaseType))
            {
                return JsonSerializer.Serialize(new
                {
                    error = $"Unknown caseType '{caseType}'. Valid values: Employee, Company, Global.",
                    type = nameof(ArgumentException)
                });
            }

            // parse optional dates
            DateTime? parsedValueDate = null;
            if (!string.IsNullOrWhiteSpace(valueDate))
            {
                if (!DateTime.TryParse(valueDate, out var vd))
                    return JsonSerializer.Serialize(new { error = $"Invalid valueDate '{valueDate}'. Use ISO 8601 format, e.g. '2026-01-01'.", type = nameof(FormatException) });
                parsedValueDate = vd;
            }

            DateTime? parsedEvaluationDate = null;
            if (!string.IsNullOrWhiteSpace(evaluationDate))
            {
                if (!DateTime.TryParse(evaluationDate, out var ed))
                    return JsonSerializer.Serialize(new { error = $"Invalid evaluationDate '{evaluationDate}'. Use ISO 8601 format, e.g. '2026-01-01'.", type = nameof(FormatException) });
                parsedEvaluationDate = ed;
            }

            // parse optional field names
            IEnumerable<string> fieldNames = null;
            if (!string.IsNullOrWhiteSpace(caseFieldNames))
            {
                fieldNames = caseFieldNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            // resolve optional employeeId
            int? employeeId = null;
            if (!string.IsNullOrWhiteSpace(employeeIdentifier))
            {
                var (_, employee) = await ResolveEmployeeAsync(tenantIdentifier, employeeIdentifier);
                employeeId = employee.Id;
            }

            var values = await PayrollService().GetCaseTimeValuesAsync(
                context: payrollContext,
                caseType: parsedCaseType,
                employeeId: employeeId,
                caseFieldNames: fieldNames,
                valueDate: parsedValueDate,
                evaluationDate: parsedEvaluationDate,
                forecast: string.IsNullOrWhiteSpace(forecast) ? null : forecast);

            return JsonSerializer.Serialize(values);
        }
        catch (Exception ex) { return Error(ex); }
    }
}
