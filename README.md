# Payroll Engine MCP Server

MCP (Model Context Protocol) server for the [Payroll Engine](https://payrollengine.org) — enables AI agents to interact with the Payroll Engine REST API using natural language.

## Overview

The MCP server exposes Payroll Engine functionality as typed tools that AI clients (Claude Desktop, GitHub Copilot, Cursor, etc.) can invoke directly. It uses the [PayrollEngine.Client.Core](https://www.nuget.org/packages/PayrollEngine.Client.Core) NuGet package and communicates via stdio transport.

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

Backend settings can also be provided as environment variables using the `__` separator convention:

```
ApiSettings__BaseUrl=https://your-backend
ApiSettings__Port=443
AllowInsecureSsl=true
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

## Tools

### Tenant

| Tool | Description |
|:-----|:------------|
| `list_tenants` | List all tenants |
| `get_tenant` | Get a tenant by identifier |
| `get_tenant_attribute` | Get a single attribute value of a tenant |
| `start_tenant` | Set up a new tenant with admin user and divisions (idempotent) |

### People

| Tool | Description |
|:-----|:------------|
| `list_users` | List all users of a tenant |
| `get_user` | Get a user by identifier |
| `get_user_attribute` | Get a single attribute value of a user |
| `list_divisions` | List all divisions of a tenant |
| `get_division` | Get a division by name |
| `get_division_attribute` | Get a single attribute value of a division |
| `list_employees` | List all employees of a tenant |
| `get_employee` | Get an employee by identifier |
| `get_employee_attribute` | Get a single attribute value of an employee |
| `list_employee_case_values` | List all case values of an employee (salary, address, etc.) |
| `list_company_case_values` | List all company case values of a tenant |

### Regulation

| Tool | Description |
|:-----|:------------|
| `list_regulations` | List all regulations of a tenant |
| `get_regulation` | Get a regulation by name |
| `list_wage_types` | List all wage types of a regulation |
| `list_lookups` | List all lookups of a regulation |
| `list_lookup_values` | List all values of a lookup |

### Payroll

| Tool | Description |
|:-----|:------------|
| `list_payrolls` | List all payrolls of a tenant |
| `get_payroll` | Get a payroll by name |
| `list_payruns` | List all payruns of a tenant |
| `list_payrun_jobs` | List all payrun jobs, ordered by creation date |
| `list_payroll_wage_types` | List effective wage types of a payroll (merged across all regulation layers) |

## Example Prompts

```
List all tenants
Show me the employees of StartTenant
What case values does mario.nunez@foo.com have in StartTenant?
List the lookup values of VatRates in SwissRegulation of CH.Swissdec
What wage types are effective in the CH-Monthly payroll of CH.Swissdec?
Set up a new tenant called AcmeCorp with culture de-CH, admin user admin@acme.com and divisions HQ,Finance
```

## License

[MIT License](LICENSE) — free for personal and commercial use.
