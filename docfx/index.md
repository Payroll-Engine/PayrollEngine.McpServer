# Payroll Engine MCP Server

The **PayrollEngine.McpServer** enables AI agents to interact with the
[Payroll Engine](https://payrollengine.org) REST API using natural language via the
[Model Context Protocol](https://modelcontextprotocol.io) (MCP).

---

## Tool Groups

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
| `list_employee_case_values` | List all case values of an employee |
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

---

## Links

- [Repository](https://github.com/Payroll-Engine/PayrollEngine.McpServer)
- [Payroll Engine](https://payrollengine.org)
