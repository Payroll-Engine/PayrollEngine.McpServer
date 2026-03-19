# Payroll Engine MCP Server

MCP (Model Context Protocol) server for the [Payroll Engine](https://payrollengine.org) — enables AI agents to query and analyse payroll data using natural language.

The MCP Server is **read-only by design**. It is an information and analysis tool; write operations are not exposed — with two exceptions: `get_employee_pay_preview` and `execute_payroll_report` execute synchronous calculations that return results without persisting anything to the database.

## Overview

The MCP server exposes Payroll Engine functionality as typed tools that AI clients (Claude Desktop, GitHub Copilot, Cursor, etc.) can invoke directly. It communicates via stdio transport and is built on [PayrollEngine.Mcp.Core](https://github.com/Payroll-Engine/PayrollEngine.Mcp.Core) and [PayrollEngine.Mcp.Tools](https://github.com/Payroll-Engine/PayrollEngine.Mcp.Tools).

## Access Control

Access is controlled by two independent dimensions:

| Dimension | Controls | Applied |
|:----------|:---------|:--------|
| Isolation Level | Which records are returned | At runtime, per query |
| Permissions | Which tools are registered | At startup, once |

Isolation Level restricts *data* — a Tenant-isolated server cannot return records from another tenant regardless of which tools are active. Permissions restrict *functionality* — a tool that is not granted is invisible to the AI agent.

## Isolation Level

Controls **which records** are returned at runtime. A Tenant-isolated server physically cannot return records from another tenant, regardless of which roles are active.

| Value | Description |
|:------|:------------|
| `MultiTenant` | Full access across all tenants (default) |
| `Tenant` | All tool calls scoped to a single tenant |
| `Division` | Scoped to a single division within a tenant. Requires `TenantIdentifier` and `DivisionName`. |
| `Employee` | Self-service — single employee access. Requires `TenantIdentifier` and `EmployeeIdentifier`. |

## Roles

Controls **which tools** are registered at startup. Each tool belongs to exactly one role. A tool whose role is not granted is invisible to the AI agent.

| Value | Domain |
|:------|:-------|
| `HR` | Employee master data, case values, and audit trail |
| `Payroll` | Payroll execution, results, preview calculations, temporal case value queries, and lookup resolution |
| `Report` | Payroll report execution and result analysis |
| `System` | Tenant and user management |

### Server — Always Available

The server tool is registered unconditionally — regardless of role permissions or isolation level.

| Tool | Description |
|:-----|:------------|
| `get_server_info` | Returns server version, active isolation level, configured scope, and role permissions. Use to verify which build is running and how it is configured. |

### HR — Human Resources

Employee master data and organisational structure: who is employed, in which division, under what conditions, and how that data has changed over time. Includes the full case value history and the audit trail of all data mutations.

#### Organisation

| Tool | Description |
|:-----|:------------|
| `list_divisions` | List all divisions of a tenant |
| `get_division` | Get a division by name |
| `get_division_attribute` | Get a single custom attribute of a division |
| `list_employees` | List employees with optional OData filter (e.g. `lastName eq 'Müller'`) |
| `get_employee` | Get an employee by identifier |
| `get_employee_attribute` | Get a single custom attribute of an employee |

#### Case Values

Case values are the current and historical field values of an employee or company (e.g. salary, address, IBAN). The history contains all versions with their validity period.

| Tool | Description |
|:-----|:------------|
| `list_employee_case_values` | Full case value history of an employee — all fields with start/end dates. Result includes employee context (identifier, first name, last name). |
| `list_company_case_values` | Company-level case values of a tenant — all company fields with start/end dates. |

#### Case Changes

Case changes are the audit trail of data mutations: each change records who made it, in which payroll, with what reason, and which field values were affected.

| Tool | Description |
|:-----|:------------|
| `list_employee_case_changes` | Audit trail of employee data mutations. Use to answer "who changed what and when". Supports OData filter and `top`. Result includes employee context. |
| `list_company_case_changes` | Audit trail of company data mutations. Supports OData filter and `top`. |

### Payroll — Payroll Processing

Payroll execution and result analysis: payroll structure, payruns, payrun jobs, result values, preview calculations, temporal case value queries, and lookup resolution. A Payroll Specialist who needs to look up employees requires `HR: Read` in addition.

#### Structure

| Tool | Description |
|:-----|:------------|
| `list_payrolls` | List all payrolls of a tenant |
| `get_payroll` | Get a payroll by name |
| `list_payruns` | List all payruns of a tenant |
| `list_payrun_jobs` | List all payrun jobs, ordered by creation date descending. Includes period, status, employee count, and job timing. Result contains a `divisions` lookup (id → name) alongside the `payrunJobs` array. |
| `list_payroll_wage_types` | Effective wage types of a payroll, merged across all regulation layers. |
| `get_payroll_lookup_value` | Resolved lookup value for a given key or range value, merged across all regulation layers. Use `lookupKey` for exact-key lookups and `rangeValue` for progressive lookups (e.g. income bracket). |

#### Results

Payroll results reflect what was calculated during payrun execution. Two complementary views are available:

| Tool | Description |
|:-----|:------------|
| `list_payroll_result_values` | Flat list of all result values (wage types and collectors). Fully denormalized — each row includes `employeeIdentifier`, `payrollName`, `periodName`, `payrunName`, and `jobName`. Supports optional filter by employee or payroll, plus OData filter. Use for cross-employee or cross-period analysis. |
| `get_consolidated_payroll_result` | All wage type results, collector results, and payrun results for one employee and one period in a single response. Use for a complete per-employee per-period overview. Result includes employee context and period boundaries. |

#### Preview

| Tool | Description |
|:-----|:------------|
| `get_employee_pay_preview` | Preview the payroll calculation for a single employee without persisting results. Returns wage type results, collector results, and payrun results. Requires `McpServer:PreviewUserIdentifier` to be configured. |

#### Temporal Case Values

`get_case_time_values` queries case values as they were valid at a specific point in time, with three temporal perspectives:

- **Historical** — set `valueDate` and `evaluationDate` to the same date to see data exactly as it was on that date, excluding later corrections.
- **Current knowledge** — set only `valueDate`; `evaluationDate` defaults to today, so retroactive corrections are visible.
- **Forecast** — set a `forecast` name and `evaluationDate = valueDate` to include planned future values.

| Tool | Description |
|:-----|:------------|
| `get_case_time_values` | Case values valid at a specific point in time. Supports `Employee`, `Company`, and `Global` case types. When scoped to a single employee via `employeeIdentifier`, result includes employee context. |

### Report — Report Execution

Payroll report execution and result analysis. Reports are resolved across all regulation layers of the payroll and return one or more named result tables. Requires `McpServer:PreviewUserIdentifier` to be configured.

| Tool | Description |
|:-----|:------------|
| `execute_payroll_report` | Execute a payroll report and return its result data set. The report is resolved across all regulation layers of the payroll. Use the `parameters` dictionary to pass report-specific input values (e.g. period, employee filter). |

### System — Administration

Tenant and user queries for cross-tenant administration and user management.

| Tool | Description |
|:-----|:------------|
| `list_tenants` | List all tenants |
| `get_tenant` | Get a tenant by identifier |
| `get_tenant_attribute` | Get a single custom attribute of a tenant |
| `list_users` | List all users of a tenant |
| `get_user` | Get a user by identifier |
| `get_user_attribute` | Get a single custom attribute of a user |

## Permissions

Each role is independently enabled or disabled per deployment.

| Value | Description |
|:------|:------------|
| `None` | Role tools are not registered — invisible to the AI agent |
| `Read` | Query tools registered (default) |

### Role × Isolation Level

`✓` = permission can be assigned (`None` / `Read`)  
`✗` = not applicable at this isolation level

| Role | MultiTenant | Tenant | Division | Employee |
|:-----|:-----------:|:------:|:--------:|:--------:|
| **HR** | ✓ | ✓ | ✓ | ✓ |
| **Payroll** | ✓ | ✓ | ✓ | ✗ |
| **Report** | ✓ | ✓ | ✗ | ✗ |
| **System** | ✓ | ✓ | ✗ | ✗ |

### Tool × Role

Each tool belongs to exactly one role. Granting a role registers all its tools.  
`*` = always registered, independent of role permissions.

| Tool | HR | Payroll | Report | System |
|:-----|:--:|:-------:|:------:|:------:|
| `get_server_info` | * | * | * | * |
| `list_divisions` | ✓ | | | |
| `get_division` | ✓ | | | |
| `get_division_attribute` | ✓ | | | |
| `list_employees` | ✓ | | | |
| `get_employee` | ✓ | | | |
| `get_employee_attribute` | ✓ | | | |
| `list_employee_case_values` | ✓ | | | |
| `list_company_case_values` | ✓ | | | |
| `list_employee_case_changes` | ✓ | | | |
| `list_company_case_changes` | ✓ | | | |
| `list_payrolls` | | ✓ | | |
| `get_payroll` | | ✓ | | |
| `list_payruns` | | ✓ | | |
| `list_payrun_jobs` | | ✓ | | |
| `list_payroll_wage_types` | | ✓ | | |
| `get_payroll_lookup_value` | | ✓ | | |
| `list_payroll_result_values` | | ✓ | | |
| `get_consolidated_payroll_result` | | ✓ | | |
| `get_employee_pay_preview` | | ✓ | | |
| `get_case_time_values` | | ✓ | | |
| `execute_payroll_report` | | | ✓ | |
| `list_tenants` | | | | ✓ |
| `get_tenant` | | | | ✓ |
| `get_tenant_attribute` | | | | ✓ |
| `list_users` | | | | ✓ |
| `get_user` | | | | ✓ |
| `get_user_attribute` | | | | ✓ |

### Tool × Isolation Level

`✓` = available  
`F` = available, records automatically filtered to configured scope  
`G` = available, access denied if employee is not in configured scope  
`✗` = not registered at this isolation level

| Tool | MultiTenant | Tenant | Division | Employee |
|:-----|:-----------:|:------:|:--------:|:--------:|
| `get_server_info` | ✓ | ✓ | ✓ | ✓ |
| `list_divisions` | ✓ | ✓ | F | F |
| `get_division` | ✓ | ✓ | ✓ | G |
| `get_division_attribute` | ✓ | ✓ | ✓ | G |
| `list_employees` | ✓ | ✓ | F ¹ | F ¹ |
| `get_employee` | ✓ | ✓ | G | G |
| `get_employee_attribute` | ✓ | ✓ | G | G |
| `list_employee_case_values` | ✓ | ✓ | G | G |
| `list_company_case_values` | ✓ | ✓ | ✓ | ✓ |
| `list_employee_case_changes` | ✓ | ✓ | G | G |
| `list_company_case_changes` | ✓ | ✓ | ✓ | ✓ |
| `list_payrolls` | ✓ | ✓ | F | ✗ |
| `get_payroll` | ✓ | ✓ | G | ✗ |
| `list_payruns` | ✓ | ✓ | F | ✗ |
| `list_payrun_jobs` | ✓ | ✓ | F | ✗ |
| `list_payroll_wage_types` | ✓ | ✓ | G | ✗ |
| `get_payroll_lookup_value` | ✓ | ✓ | G | ✗ |
| `list_payroll_result_values` | ✓ | ✓ | F | ✗ |
| `get_consolidated_payroll_result` | ✓ | ✓ | G | ✗ |
| `get_employee_pay_preview` | ✓ | ✓ | ✓ | ✗ |
| `get_case_time_values` | ✓ | ✓ | G ² | ✗ |
| `execute_payroll_report` | ✓ | ✓ | ✗ | ✗ |
| `list_tenants` | ✓ | ✓ | ✗ | ✗ |
| `get_tenant` | ✓ | ✓ | ✗ | ✗ |
| `get_tenant_attribute` | ✓ | ✓ | ✗ | ✗ |
| `list_users` | ✓ | ✓ | ✗ | ✗ |
| `get_user` | ✓ | ✓ | ✗ | ✗ |
| `get_user_attribute` | ✓ | ✓ | ✗ | ✗ |

¹ Division filtering applied client-side — the backend does not support OData collection lambda expressions (`divisions/any()`). All employees are fetched and then filtered in memory by division membership.  
² Guard applies only when `employeeIdentifier` is provided. Without it, `Company` and `Global` case types return all values unfiltered; `Employee` case type returns values for all employees without division scoping.

### Persona Examples

| Persona | HR | Payroll | Report | System |
|:--------|:--:|:-------:|:------:|:------:|
| HR Manager | Read | None | None | None |
| Payroll Specialist | Read | Read | None | None |
| HR Business Partner | Read | Read | None | None |
| Controller / Analyst | Read | Read | Read | None |
| Report Analyst | None | None | Read | None |
| System Administrator | None | None | None | Read |
| Developer | Read | Read | Read | Read |

---

## Prerequisites

- [Payroll Engine Backend](https://github.com/Payroll-Engine/PayrollEngine.Backend) running
- .NET 10 SDK
- An MCP-compatible AI client

## Configuration

Backend connection settings in `Server/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost",
    "Port": 443
  }
}
```

Sensitive settings (API key) go in `apisettings.json` (excluded from source control):

```json
{
  "ApiSettings": {
    "ApiKey": "your-api-key"
  }
}
```

IsolationLevel and role permissions in `appsettings.json`:

```json
{
  "McpServer": {
    "IsolationLevel": "Tenant",
    "TenantIdentifier": "acme-corp",
    "Permissions": {
      "HR":      "Read",
      "Payroll": "Read",
      "Report":  "Read",
      "System":  "None"
    }
  }
}
```

For Division isolation:

```json
{
  "McpServer": {
    "IsolationLevel": "Division",
    "TenantIdentifier": "acme-corp",
    "DivisionName": "sales"
  }
}
```

For Employee isolation:

```json
{
  "McpServer": {
    "IsolationLevel": "Employee",
    "TenantIdentifier": "acme-corp",
    "EmployeeIdentifier": "mario.nunez@acme.com"
  }
}
```

For payrun preview and report execution (`get_employee_pay_preview`, `execute_payroll_report`), configure a service account user that exists in the target tenant:

```json
{
  "McpServer": {
    "PreviewUserIdentifier": "mcp-service@acme.com"
  }
}
```

All settings can also be provided as environment variables using the `__` separator:

```
McpServer__IsolationLevel=Tenant
McpServer__TenantIdentifier=acme-corp
McpServer__DivisionName=sales
McpServer__EmployeeIdentifier=mario.nunez@acme.com
McpServer__PreviewUserIdentifier=mcp-service@acme.com
McpServer__Permissions__HR=Read
McpServer__Permissions__Payroll=Read
McpServer__Permissions__Report=Read
McpServer__Permissions__System=None
ApiSettings__BaseUrl=https://your-backend
ApiSettings__Port=443
```

## MCP Client Setup

### Claude Desktop

Add to `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "payroll-engine": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/Server/PayrollEngine.Mcp.Server.csproj",
        "--no-launch-profile",
        "--no-build"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Development",
        "ApiSettings__BaseUrl": "https://localhost",
        "ApiSettings__Port": "443",
        "AllowInsecureSsl": "true"
      }
    }
  }
}
```

### Docker

```bash
docker run --rm -i \
  -e ApiSettings__BaseUrl=https://your-backend \
  -e ApiSettings__Port=443 \
  ghcr.io/payroll-engine/payrollengine.mcp.server
```

## Example Prompts

```
List all tenants
Show me the employees of StartTenant
What case values does mario.nunez@foo.com have in StartTenant?
What changed in the employee data of mario.nunez@foo.com in January 2026?
What wage types are effective in the CH-Monthly payroll of CH.Swissdec?
What was the salary of all employees as of Dec 31, 2024?
Show me all payroll results for Müller in March 2026
What is the tax rate for an income of 85000 in the TaxRates lookup?
What would the payroll look like for Müller in April 2026?
Run the MonthlyPayslip report for the CH-Monthly payroll
```

## License

[MIT License](LICENSE) — free for personal and commercial use.
