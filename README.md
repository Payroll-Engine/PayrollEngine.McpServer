# Payroll Engine MCP Server

MCP (Model Context Protocol) server for the [Payroll Engine](https://payrollengine.org) — enables AI agents to query and analyse payroll data using natural language.

The MCP Server is **read-only by design**. It is an information and analysis tool; no mutation operations are exposed. This ensures that payroll data can never be modified through an AI agent, regardless of configuration.

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
| `Division` | Scoped to a single division within a tenant *(planned)* |
| `Employee` | Self-service — single employee access *(planned)* |

## Roles

Controls **which tools** are registered at startup. Each tool belongs to exactly one role. A tool whose role is not granted is invisible to the AI agent.

| Value | Domain |
|:------|:-------|
| `HR` | Employee master data, case values, and audit trail |
| `Payroll` | Payroll execution, results, and temporal case value queries |
| `Regulation` | Regulation definitions: wage types and lookups |
| `System` | Tenant and user management |

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

Payroll execution and result analysis: payroll structure, payruns, payrun jobs, result values, and temporal case value queries. A Payroll Specialist who needs to look up employees requires `HR: Read` in addition.

#### Structure

| Tool | Description |
|:-----|:------------|
| `list_payrolls` | List all payrolls of a tenant |
| `get_payroll` | Get a payroll by name |
| `list_payruns` | List all payruns of a tenant |
| `list_payrun_jobs` | List all payrun jobs, ordered by creation date descending. Includes period, status, employee count, and job timing. Result contains a `divisions` lookup (id → name) alongside the `payrunJobs` array. |
| `list_payroll_wage_types` | Effective wage types of a payroll, merged across all regulation layers. Distinct from `list_wage_types` (Regulation), which returns raw definitions within a single regulation. |

#### Results

Payroll results reflect what was calculated during payrun execution. Two complementary views are available:

| Tool | Description |
|:-----|:------------|
| `list_payroll_result_values` | Flat list of all result values (wage types and collectors). Fully denormalized — each row includes `employeeIdentifier`, `payrollName`, `periodName`, `payrunName`, and `jobName`. Supports optional filter by employee or payroll, plus OData filter. Use for cross-employee or cross-period analysis. |
| `get_consolidated_payroll_result` | All wage type results, collector results, and payrun results for one employee and one period in a single response. Use for a complete per-employee per-period overview. Result includes employee context and period boundaries. |

#### Temporal Case Values

`get_case_time_values` queries case values as they were valid at a specific point in time, with three temporal perspectives:

- **Historical** — set `valueDate` and `evaluationDate` to the same date to see data exactly as it was on that date, excluding later corrections.
- **Current knowledge** — set only `valueDate`; `evaluationDate` defaults to today, so retroactive corrections are visible.
- **Forecast** — set a `forecast` name and `evaluationDate = valueDate` to include planned future values.

| Tool | Description |
|:-----|:------------|
| `get_case_time_values` | Case values valid at a specific point in time. Supports `Employee`, `Company`, and `Global` case types. When scoped to a single employee via `employeeIdentifier`, result includes employee context. |

### Regulation — Regulation Design and Verification

Payroll rule definitions: regulations, wage type definitions, and lookup tables.

| Tool | Description |
|:-----|:------------|
| `list_regulations` | List all regulations of a tenant |
| `get_regulation` | Get a regulation by name |
| `list_wage_types` | Wage type definitions within a single regulation (raw, not merged). Distinct from `list_payroll_wage_types` (Payroll), which returns the effective merged result. |
| `list_lookups` | All lookups of a regulation |
| `list_lookup_values` | All values of a specific lookup, with key-value pairs and optional range and culture support. |

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

| Role | MultiTenant | Tenant | Division *(planned)* | Employee *(planned)* |
|:-----|:-----------:|:------:|:--------------------:|:--------------------:|
| **HR** | ✓ | ✓ | ✓ | ✓ |
| **Payroll** | ✓ | ✓ | ✓ | ✗ |
| **Regulation** | ✓ | ✓ | ✗ | ✗ |
| **System** | ✓ | ✓ | ✗ | ✗ |

### Persona Examples

| Persona | HR | Payroll | Regulation | System |
|:--------|:--:|:-------:|:----------:|:------:|
| HR Manager | Read | None | None | None |
| Payroll Specialist | Read | Read | None | None |
| HR Business Partner | Read | Read | None | None |
| Regulation Developer | Read | Read | Read | None |
| Controller / Analyst | Read | Read | None | None |
| System Administrator | None | None | None | Read |
| Developer | Read | Read | Read | Read |

---

## Prerequisites

- [Payroll Engine Backend](https://github.com/Payroll-Engine/PayrollEngine.Backend) running
- .NET 10 SDK
- An MCP-compatible AI client

## Configuration

Backend connection settings in `McpServer/appsettings.json`:

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
      "HR":         "Read",
      "Payroll":    "Read",
      "Regulation": "None",
      "System":     "None"
    }
  }
}
```

All settings can also be provided as environment variables using the `__` separator:

```
McpServer__IsolationLevel=Tenant
McpServer__TenantIdentifier=acme-corp
McpServer__Permissions__HR=Read
McpServer__Permissions__Payroll=Read
McpServer__Permissions__Regulation=None
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
        "path/to/McpServer/PayrollEngine.McpServer.csproj",
        "--no-launch-profile"
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
  ghcr.io/payroll-engine/payrollengine.mcpserver
```

## Example Prompts

```
List all tenants
Show me the employees of StartTenant
What case values does mario.nunez@foo.com have in StartTenant?
What changed in the employee data of mario.nunez@foo.com in January 2026?
List the lookup values of VatRates in SwissRegulation of CH.Swissdec
What wage types are effective in the CH-Monthly payroll of CH.Swissdec?
What was the salary of all employees as of Dec 31, 2024?
Show me all payroll results for Müller in March 2026
```

## License

[MIT License](LICENSE) — free for personal and commercial use.
