# Payroll Engine MCP Server

MCP (Model Context Protocol) server for the [Payroll Engine](https://payrollengine.org) — enables AI agents to interact with the Payroll Engine REST API using natural language.

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
| `HR` | Employee master data and organisational structure |
| `Payroll` | Payroll execution, payruns, jobs, and temporal CaseValue queries |
| `Regulation` | Regulation definitions: wage types and lookups |
| `System` | Tenant and user management |

### HR — Human Resources

Employee master data and organisational structure: who is employed, in which division, under what conditions, and how that data has changed over time. Does not include payrun execution or regulation internals.

| Tool | Description | Min. Permission |
|:-----|:------------|:---------------:|
| `list_divisions` | List all divisions of a tenant | Read |
| `get_division` | Get a division by name | Read |
| `get_division_attribute` | Get a single attribute of a division | Read |
| `list_employees` | List employees, with optional OData filter | Read |
| `get_employee` | Get an employee by identifier | Read |
| `get_employee_attribute` | Get a single attribute of an employee | Read |
| `list_employee_case_values` | Full CaseValue history of an employee | Read |
| `list_company_case_values` | Company-level CaseValues of a tenant | Read |

### Payroll — Payroll Processing

Payroll execution and result verification: payroll structure, payruns, jobs, and temporal CaseValue queries. `get_case_time_values` drives payroll verification, retroactive correction checks, and forecast planning. A Payroll Specialist who needs to look up employees requires `HR: Read` in addition.

| Tool | Description | Min. Permission |
|:-----|:------------|:---------------:|
| `list_payrolls` | List all payrolls of a tenant | Read |
| `get_payroll` | Get a payroll by name | Read |
| `list_payruns` | List all payruns of a tenant | Read |
| `list_payrun_jobs` | List all payrun jobs, ordered by creation date | Read |
| `list_payroll_wage_types` | Effective wage types of a payroll (merged across all regulation layers) | Read |
| `get_case_time_values` | CaseValues at a specific point in time — historical, current knowledge, or forecast | Read |

### Regulation — Regulation Design and Verification

Payroll rule definitions: regulations, wage type definitions, and lookup tables. `list_wage_types` returns raw definitions within a single regulation — distinct from `list_payroll_wage_types` (Payroll), which returns the effective merged result across regulation layers.

| Tool | Description | Min. Permission |
|:-----|:------------|:---------------:|
| `list_regulations` | List all regulations of a tenant | Read |
| `get_regulation` | Get a regulation by name | Read |
| `list_wage_types` | Wage type definitions within a regulation | Read |
| `list_lookups` | All lookups of a regulation | Read |
| `list_lookup_values` | Values of a specific lookup | Read |

### System — Administration

Tenant and user management. `start_tenant` is the only current write tool and requires `Full`. In Tenant-isolated deployments the tenant context is fixed at startup — System tools are typically only needed for cross-tenant administration.

| Tool | Description | Min. Permission |
|:-----|:------------|:---------------:|
| `list_tenants` | List all tenants | Read |
| `get_tenant` | Get a tenant by identifier | Read |
| `get_tenant_attribute` | Get a single attribute of a tenant | Read |
| `list_users` | List all users of a tenant | Read |
| `get_user` | Get a user by identifier | Read |
| `get_user_attribute` | Get a single attribute of a user | Read |
| `start_tenant` | Create tenant, admin user, and divisions (idempotent) | **Full** |

## Permissions

Each role is granted a permission level per deployment. The default when the `Permissions` section is absent is `Full` for all roles.

| Value | Description |
|:------|:------------|
| `None` | Role tools are not registered — invisible to the AI agent |
| `Read` | Query tools only. Write tools are excluded even if available |
| `Full` | Read and write tools (default) |

### Role × Isolation Level

`✓` = permission can be assigned (`None` / `Read` / `Full`)  
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
| HR Manager | Full | None | None | None |
| Payroll Specialist | Read | Full | None | None |
| HR Business Partner | Full | Read | None | None |
| Regulation Developer | Read | Read | Full | None |
| Controller / Analyst | Read | Read | None | None |
| System Administrator | None | None | None | Full |
| Developer | Full | Full | Full | Full |

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
      "Payroll":    "Full",
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
McpServer__Permissions__Payroll=Full
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
List the lookup values of VatRates in SwissRegulation of CH.Swissdec
What wage types are effective in the CH-Monthly payroll of CH.Swissdec?
What was the salary of all employees as of Dec 31, 2024?
Set up a new tenant called AcmeCorp with culture de-CH, admin user admin@acme.com and divisions HQ,Finance
```

## License

[MIT License](LICENSE) — free for personal and commercial use.
