---
_disableContribution: true
---

# Payroll Engine MCP Server

MCP (Model Context Protocol) server for the [Payroll Engine](https://payrollengine.org) — enables AI agents to query and analyse payroll data using natural language.

The MCP Server is **read-only by design**. It is an information and analysis tool; write operations are not exposed — with two exceptions: `get_employee_pay_preview` and `execute_payroll_report` execute synchronous calculations that return results without persisting anything to the database.

## Overview

The MCP server exposes Payroll Engine functionality as typed tools that AI clients (Claude Desktop, GitHub Copilot, Cursor, etc.) can invoke directly. It uses the [PayrollEngine.Client.Core](https://www.nuget.org/packages/PayrollEngine.Client.Core) NuGet package and communicates via stdio transport.

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

| Tool | Description |
|:-----|:------------|
| `get_server_info` | Returns server version, active isolation level, configured scope, and role permissions. |

### HR — Human Resources

#### Organisation

| Tool | Description |
|:-----|:------------|
| `list_divisions` | List all divisions of a tenant |
| `get_division` | Get a division by name |
| `get_division_attribute` | Get a single custom attribute of a division |
| `list_employees` | List employees with optional OData filter |
| `get_employee` | Get an employee by identifier |
| `get_employee_attribute` | Get a single custom attribute of an employee |

#### Case Values

| Tool | Description |
|:-----|:------------|
| `list_employee_case_values` | Full case value history of an employee |
| `list_company_case_values` | Company-level case values of a tenant |

#### Case Changes

| Tool | Description |
|:-----|:------------|
| `list_employee_case_changes` | Audit trail of employee data mutations |
| `list_company_case_changes` | Audit trail of company data mutations |

### Payroll — Payroll Processing

#### Structure

| Tool | Description |
|:-----|:------------|
| `list_payrolls` | List all payrolls of a tenant |
| `get_payroll` | Get a payroll by name |
| `list_payruns` | List all payruns of a tenant |
| `list_payrun_jobs` | List all payrun jobs, ordered by creation date descending |
| `list_payroll_wage_types` | Effective wage types of a payroll, merged across all regulation layers |
| `get_payroll_lookup_value` | Resolved lookup value for a given key or range value |

#### Results

| Tool | Description |
|:-----|:------------|
| `list_payroll_result_values` | Flat list of all result values (wage types and collectors) |
| `get_consolidated_payroll_result` | All results for one employee and one period in a single response |

#### Preview

| Tool | Description |
|:-----|:------------|
| `get_employee_pay_preview` | Preview the payroll calculation for a single employee without persisting results |

#### Temporal Case Values

| Tool | Description |
|:-----|:------------|
| `get_case_time_values` | Case values valid at a specific point in time. Supports Historical, Current knowledge, and Forecast perspectives. |

### Report — Report Execution

| Tool | Description |
|:-----|:------------|
| `execute_payroll_report` | Execute a payroll report and return its result data set |

### System — Administration

| Tool | Description |
|:-----|:------------|
| `list_tenants` | List all tenants |
| `get_tenant` | Get a tenant by identifier |
| `get_tenant_attribute` | Get a single custom attribute of a tenant |
| `list_users` | List all users of a tenant |
| `get_user` | Get a user by identifier |
| `get_user_attribute` | Get a single custom attribute of a user |

## Prerequisites

- [Payroll Engine Backend](https://github.com/Payroll-Engine/PayrollEngine.Backend) running
- .NET 10 SDK
- An MCP-compatible AI client

## Configuration

Backend connection in `McpServer/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost",
    "Port": 443
  }
}
```

IsolationLevel and role permissions:

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

## MCP Client Setup

### Claude Desktop

```json
{
  "mcpServers": {
    "payroll-engine": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/McpServer/PayrollEngine.McpServer.csproj",
        "--no-launch-profile",
        "--no-build"
      ]
    }
  }
}
```

### Docker

```bash
docker run --rm -i \
  -e ApiSettings__BaseUrl=https://your-backend \
  -e ApiSettings__Port=443 \
  ghcr.io/payroll-engine/payrollengine.mcpserver
```

## Example Prompts

```
List all tenants
Show me the employees of StartTenant
What case values does mario.nunez@foo.com have in StartTenant?
What changed in the employee data of mario.nunez@foo.com in January 2026?
What wage types are effective in the CH-Monthly payroll?
Show me all payroll results for Müller in March 2026
What would the payroll look like for Müller in April 2026?
Run the MonthlyPayslip report for the CH-Monthly payroll
```

## License

[MIT License](LICENSE) — free for personal and commercial use.
