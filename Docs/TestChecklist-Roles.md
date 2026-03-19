# MCP Server — Test Checklist: Roles (Permissions)

Verify that each role registers exactly the correct tools and no others.  
Roles are enforced at **startup** — each section requires a server restart.

**Legend**
- `[ ]` — not yet tested
- `[✓]` — passed
- `[✗]` — failed (document actual in notes)
- Visible: tool appears in the AI agent's tool list
- Invisible: tool does **not** appear — calling it returns "unknown tool" or similar

> All tests run at `IsolationLevel: Tenant` with `TenantIdentifier: ACME.International`  
> to keep data scoping consistent and independent of role testing.

---

## Round 1 — HR only

```json
"McpServer": {
  "IsolationLevel": "Tenant",
  "TenantIdentifier": "ACME.International",
  "Permissions": {
    "HR":      "Read",
    "Payroll": "None",
    "Report":  "None",
    "System":  "None"
  }
}
```

### Must be visible (10 tools)

| # | Tool | Result | Notes |
|---|------|:------:|-------|
| 1 | `list_divisions` | [ ] | |
| 2 | `get_division` | [ ] | |
| 3 | `get_division_attribute` | [ ] | |
| 4 | `list_employees` | [ ] | |
| 5 | `get_employee` | [ ] | |
| 6 | `get_employee_attribute` | [ ] | |
| 7 | `list_employee_case_values` | [ ] | |
| 8 | `list_company_case_values` | [ ] | |
| 9 | `list_employee_case_changes` | [ ] | |
| 10 | `list_company_case_changes` | [ ] | |

### Must be invisible

| # | Tool | Role | Result | Notes |
|---|------|------|:------:|-------|
| 11 | `list_payrolls` | Payroll | [ ] | |
| 12 | `get_payroll` | Payroll | [ ] | |
| 13 | `list_payruns` | Payroll | [ ] | |
| 14 | `list_payrun_jobs` | Payroll | [ ] | |
| 15 | `list_payroll_wage_types` | Payroll | [ ] | |
| 16 | `get_payroll_lookup_value` | Payroll | [ ] | |
| 17 | `list_payroll_result_values` | Payroll | [ ] | |
| 18 | `get_consolidated_payroll_result` | Payroll | [ ] | |
| 19 | `get_employee_pay_preview` | Payroll | [ ] | |
| 20 | `get_case_time_values` | Payroll | [ ] | |
| 21 | `execute_payroll_report` | Report | [ ] | |
| 22 | `list_tenants` | System | [ ] | |
| 23 | `get_tenant` | System | [ ] | |
| 24 | `get_tenant_attribute` | System | [ ] | |
| 25 | `list_users` | System | [ ] | |
| 26 | `get_user` | System | [ ] | |
| 27 | `get_user_attribute` | System | [ ] | |

---

## Round 2 — Payroll only

```json
"McpServer": {
  "IsolationLevel": "Tenant",
  "TenantIdentifier": "ACME.International",
  "Permissions": {
    "HR":      "None",
    "Payroll": "Read",
    "Report":  "None",
    "System":  "None"
  }
}
```

### Must be visible (10 tools)

| # | Tool | Result | Notes |
|---|------|:------:|-------|
| 1 | `list_payrolls` | [ ] | |
| 2 | `get_payroll` | [ ] | |
| 3 | `list_payruns` | [ ] | |
| 4 | `list_payrun_jobs` | [ ] | |
| 5 | `list_payroll_wage_types` | [ ] | |
| 6 | `get_payroll_lookup_value` | [ ] | |
| 7 | `list_payroll_result_values` | [ ] | |
| 8 | `get_consolidated_payroll_result` | [ ] | |
| 9 | `get_employee_pay_preview` | [ ] | Requires `PreviewUserIdentifier` |
| 10 | `get_case_time_values` | [ ] | |

### Must be invisible

| # | Tool | Role | Result | Notes |
|---|------|------|:------:|-------|
| 11 | `list_divisions` | HR | [ ] | |
| 12 | `get_division` | HR | [ ] | |
| 13 | `get_division_attribute` | HR | [ ] | |
| 14 | `list_employees` | HR | [ ] | |
| 15 | `get_employee` | HR | [ ] | |
| 16 | `get_employee_attribute` | HR | [ ] | |
| 17 | `list_employee_case_values` | HR | [ ] | |
| 18 | `list_company_case_values` | HR | [ ] | |
| 19 | `list_employee_case_changes` | HR | [ ] | |
| 20 | `list_company_case_changes` | HR | [ ] | |
| 21 | `execute_payroll_report` | Report | [ ] | |
| 22 | `list_tenants` | System | [ ] | |
| 23 | `get_tenant` | System | [ ] | |
| 24 | `get_tenant_attribute` | System | [ ] | |
| 25 | `list_users` | System | [ ] | |
| 26 | `get_user` | System | [ ] | |
| 27 | `get_user_attribute` | System | [ ] | |

---

## Round 3 — Report only

```json
"McpServer": {
  "IsolationLevel": "Tenant",
  "TenantIdentifier": "ACME.International",
  "Permissions": {
    "HR":      "None",
    "Payroll": "None",
    "Report":  "Read",
    "System":  "None"
  }
}
```

> Note: `execute_payroll_report` requires `McpServer:PreviewUserIdentifier` to be set.

### Must be visible (1 tool)

| # | Tool | Result | Notes |
|---|------|:------:|-------|
| 1 | `execute_payroll_report` | [ ] | Requires `PreviewUserIdentifier` |

### Must be invisible (all others)

| # | Tool | Role | Result | Notes |
|---|------|------|:------:|-------|
| 2 | `list_divisions` | HR | [ ] | |
| 3 | `list_employees` | HR | [ ] | |
| 4 | `list_employee_case_values` | HR | [ ] | |
| 5 | `list_payrolls` | Payroll | [ ] | |
| 6 | `list_payruns` | Payroll | [ ] | |
| 7 | `get_case_time_values` | Payroll | [ ] | |
| 8 | `list_tenants` | System | [ ] | |
| 9 | `list_users` | System | [ ] | |

---

## Round 4 — System only

```json
"McpServer": {
  "IsolationLevel": "Tenant",
  "TenantIdentifier": "ACME.International",
  "Permissions": {
    "HR":      "None",
    "Payroll": "None",
    "Report":  "None",
    "System":  "Read"
  }
}
```

### Must be visible (6 tools)

| # | Tool | Result | Notes |
|---|------|:------:|-------|
| 1 | `list_tenants` | [ ] | Only `ACME.International` |
| 2 | `get_tenant` | [ ] | |
| 3 | `get_tenant_attribute` | [ ] | |
| 4 | `list_users` | [ ] | No password hash in response |
| 5 | `get_user` | [ ] | No password hash in response |
| 6 | `get_user_attribute` | [ ] | |

### Must be invisible

| # | Tool | Role | Result | Notes |
|---|------|------|:------:|-------|
| 7 | `list_divisions` | HR | [ ] | |
| 8 | `list_employees` | HR | [ ] | |
| 9 | `list_employee_case_values` | HR | [ ] | |
| 10 | `list_payrolls` | Payroll | [ ] | |
| 11 | `list_payruns` | Payroll | [ ] | |
| 12 | `execute_payroll_report` | Report | [ ] | |

---

## Round 5 — Persona: Payroll Specialist (HR + Payroll)

```json
"McpServer": {
  "IsolationLevel": "Tenant",
  "TenantIdentifier": "ACME.International",
  "Permissions": {
    "HR":      "Read",
    "Payroll": "Read",
    "Report":  "None",
    "System":  "None"
  }
}
```

### Must be visible (20 tools)

| # | Tool | Result | Notes |
|---|------|:------:|-------|
| 1 | `list_divisions` | [ ] | |
| 2 | `list_employees` | [ ] | |
| 3 | `list_employee_case_values` | [ ] | |
| 4 | `list_employee_case_changes` | [ ] | |
| 5 | `list_company_case_values` | [ ] | |
| 6 | `list_company_case_changes` | [ ] | |
| 7 | `list_payrolls` | [ ] | |
| 8 | `list_payruns` | [ ] | |
| 9 | `list_payroll_result_values` | [ ] | |
| 10 | `get_case_time_values` | [ ] | |
| 11–20 | *(remaining HR + Payroll tools)* | [ ] | |

### Must be invisible

| # | Tool | Role | Result | Notes |
|---|------|------|:------:|-------|
| 21 | `execute_payroll_report` | Report | [ ] | |
| 22 | `list_tenants` | System | [ ] | |
| 23 | `list_users` | System | [ ] | |

---

## Round 6 — All None (sanity check)

```json
"McpServer": {
  "Permissions": {
    "HR":      "None",
    "Payroll": "None",
    "Report":  "None",
    "System":  "None"
  }
}
```

No tools should be registered. AI agent has no tools available.

| # | Check | Result | Notes |
|---|-------|:------:|-------|
| 1 | Tool list is empty | [ ] | |

---

## Summary

| Round | Config | Tested | Passed | Failed |
|-------|--------|:------:|:------:|:------:|
| 1 | HR only | ✓ | 11 | 0 |
| 2 | Payroll only | ✓ | 11 | 0 |
| 3 | Report only | ✓ | 2 | 0 |
| 4 | System only | ✓ | 7 | 0 |
| 5 | Payroll Specialist (HR + Payroll) | ✓ | 21 | 0 |
| 6 | All None | ✓ | 1 | 0 |

**Last tested:** 2026-03-18  
**Tester:** Jani Giannoudis
