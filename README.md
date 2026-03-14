# Payroll Engine MCP Server

MCP (Model Context Protocol) server for the [Payroll Engine](https://payrollengine.org) — enables AI agents to interact with the Payroll Engine REST API using natural language.

## Overview

The MCP server exposes Payroll Engine functionality as typed tools that AI clients (Claude, GitHub Copilot, Cursor, etc.) can invoke directly. It uses the [PayrollEngine.Client.Core](https://www.nuget.org/packages/PayrollEngine.Client.Core) NuGet package and communicates via stdio transport.

## Prerequisites

- [Payroll Engine Backend](https://github.com/Payroll-Engine/PayrollEngine.Backend) running
- .NET 10 SDK
- An MCP-compatible AI client

## Configuration

Copy `McpServer/appsettings.json` and configure the backend connection:

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

## MCP Client Setup

### Claude Desktop (`claude_desktop_config.json`)

```json
{
  "mcpServers": {
    "payroll-engine": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/McpServer/PayrollEngine.McpServer.csproj"]
    }
  }
}
```

### Docker

```bash
docker run --rm -i \
  -e ApiSettings__BaseUrl=https://your-backend \
  ghcr.io/payroll-engine/payrollengine.mcpserver
```

## Tool Groups

| Group        | Tools                                      |
|:-------------|:-------------------------------------------|
| Tenant       | `start_tenant`, `get_tenant`, `list_tenants` |
| People       | `list_users`, `list_divisions`, `list_employees`, `get_employee` |
| Regulation   | `list_regulations`, `list_wage_types`, `list_lookups` |
| Payroll      | `list_payrolls`, `list_payruns`, `list_payrun_jobs`, `get_payrun_results` |

## License

[MIT License](LICENSE) — free for personal and commercial use.
