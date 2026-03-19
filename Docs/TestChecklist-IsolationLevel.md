# MCP Server — Test Checklist: Isolation Level

Verify that each tool returns only records within the configured scope.  
Run one section at a time — each requires a server restart with the corresponding `appsettings.json`.

**Legend**
- `[ ]` — not yet tested
- `[✓]` — passed
- `[✗]` — failed (document actual vs. expected in notes)
- `F` — filter: list result must contain only records of the configured scope
- `G` — guard: request for out-of-scope record must be rejected with an error

---

## Level 1 — MultiTenant

```json
// No McpServer block required (default)
```

No scoping. All tools return all records across all tenants.  
**Skip — no verification required.**

---

## Level 2 — Tenant

```json
"McpServer": {
  "IsolationLevel": "Tenant",
  "TenantIdentifier": "ACME.International"
}
```

Restart server, then verify each tool is scoped to `ACME.International`.

### System Tools

| # | Tool | Check | Call | Expected | Result | Notes |
|---|------|:-----:|------|----------|:------:|-------|
| 1 | `list_tenants` | F | `list_tenants` | Only `ACME.International` | [ ✓ ] | |
| 2 | `get_tenant` | G | `get_tenant(ACME.International)` | Returns tenant | [ ✓ ] | |
| 3 | `get_tenant` | G | `get_tenant(OtherTenant)` | Error / not found | [ ✓ ] | |
| 4 | `get_tenant_attribute` | ✓ | `get_tenant_attribute(ACME.International, ...)` | Returns attribute | [ empty ] | |
| 5 | `list_users` | F | `list_users(ACME.International)` | Only ACME users, **no password hash** | [ ✓ ] | |

### HR Tools

| # | Tool | Check | Call | Expected | Result | Notes |
|---|------|:-----:|------|----------|:------:|-------|
| 6 | `list_divisions` | F | `list_divisions(ACME.International)` | Only ACME divisions | [ ✓ ] | |
| 7 | `get_division` | ✓ | `get_division(ACME.International, ACME.DE)` | Returns division | [ ✓ ] | |
| 8 | `list_employees` | F | `list_employees(ACME.International)` | Only ACME employees | [ ✓ ] | |
| 9 | `get_employee` | ✓ | `get_employee(ACME.International, anna.weber@...)` | Returns employee | [ ✓ ] | |
| 10 | `list_employee_case_values` | ✓ | `list_employee_case_values(ACME.International, anna.weber@...)` | Returns values | [ ✓ ] | |
| 11 | `list_employee_case_changes` | ✓ | `list_employee_case_changes(ACME.International, anna.weber@...)` | Returns changes | [ ✓ ] | |
| 12 | `list_company_case_values` | ✓ | `list_company_case_values(ACME.International)` | Returns values | [ ✓ ] | |
| 13 | `list_company_case_changes` | ✓ | `list_company_case_changes(ACME.International)` | Returns changes | [ ✓ ] | |

### Payroll Tools

| # | Tool | Check | Call | Expected | Result | Notes |
|---|------|:-----:|------|----------|:------:|-------|
| 14 | `list_payrolls` | F | `list_payrolls(ACME.International)` | Only ACME payrolls | [ ✓ ] | |
| 15 | `list_payruns` | F | `list_payruns(ACME.International)` | Only ACME payruns | [ ✓ ] | |
| 16 | `list_payrun_jobs` | F | `list_payrun_jobs(ACME.International)` | Only ACME jobs | [ ✓ ] | |
| 17 | `list_payroll_result_values` | F | `list_payroll_result_values(ACME.International)` | Only ACME results | [ ✓ ] | |
| 18 | `get_case_time_values` | ✓ | `get_case_time_values(ACME.International, ACME.Payroll.DE, ...)` | Returns values | [ ✓ ] | |
| 19 | `get_case_time_values` | F | `get_case_time_values(ACME.International, ACME.Payroll.DE)` without employeeId | All ACME employees | [ ✓ ] | |
| 20 | `get_consolidated_payroll_result` | ✓ | `get_consolidated_payroll_result(ACME.International, anna.weber@..., ...)` | Returns result | [ ✓ ] | |

---

## Level 3 — Division

```json
"McpServer": {
  "IsolationLevel": "Division",
  "TenantIdentifier": "ACME.International",
  "DivisionName": "ACME.DE",
  "Permissions": {
    "HR":      "Read",
    "Payroll": "Read",
    "Report":  "None",
    "System":  "None"
  }
}
```

> `Report` and `System` must not be registered at Division level — verify they are invisible.

Restart server, then verify each tool is scoped to division `ACME.DE`.

### Tools Not Registered (must be invisible)

| # | Tool | Role | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 1 | `execute_payroll_report` | Report | Not visible to AI agent | [ ✓ ] | |
| 2 | `list_tenants` | System | Not visible to AI agent | [ ✓ ] | |
| 3 | `get_tenant` | System | Not visible to AI agent | [ ✓ ] | |
| 4 | `get_tenant_attribute` | System | Not visible to AI agent | [ ✓ ] | |
| 5 | `list_users` | System | Not visible to AI agent | [ ✓ ] | |
| 6 | `get_user` | System | Not visible to AI agent | [ ✓ ] | |
| 7 | `get_user_attribute` | System | Not visible to AI agent | [ ✓ ] | |

### HR Tools — Filter (F)

| # | Tool | Call | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 8 | `list_divisions` | `list_divisions(ACME.International)` | Only `ACME.DE` | [ ✓ ] | |
| 9 | `list_employees` | `list_employees(ACME.International)` | Only ACME.DE employees (Anna Weber, Sophie Klein) | [ ✓ ] | |

### HR Tools — Guard (G)

| # | Tool | Call | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 10 | `list_employee_case_values` | `list_employee_case_values(ACME.International, anna.weber@...)` | Returns values (ACME.DE ✓) | [ ✓ ] | |
| 11 | `list_employee_case_values` | `list_employee_case_values(ACME.International, pierre.dubois@...)` | **Error: access denied** (ACME.FR ✗) | [ ✓ ] | |
| 12 | `list_employee_case_changes` | `list_employee_case_changes(ACME.International, anna.weber@...)` | Returns changes (ACME.DE ✓) | [ ✓ ] | |
| 13 | `list_employee_case_changes` | `list_employee_case_changes(ACME.International, pierre.dubois@...)` | **Error: access denied** (ACME.FR ✗) | [ ✓ ] | |

### Payroll Tools — Filter (F)

| # | Tool | Call | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 14 | `list_payrolls` | `list_payrolls(ACME.International)` | Only `ACME.Payroll.DE` | [ ✓ ] | |
| 15 | `list_payruns` | `list_payruns(ACME.International)` | Only `ACME.Payrun.DE` | [ ✓ ] | |
| 16 | `list_payrun_jobs` | `list_payrun_jobs(ACME.International)` | Only ACME.DE jobs | [ ✓ ] | |
| 17 | `list_payroll_result_values` | `list_payroll_result_values(ACME.International)` | Only ACME.DE results | [ ✓ ] | |

### Payroll Tools — Guard (G)

| # | Tool | Call | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 18 | `get_payroll` | `get_payroll(ACME.International, ACME.Payroll.DE)` | Returns payroll (ACME.DE ✓) | [ ✓ ] | |
| 19 | `get_payroll` | `get_payroll(ACME.International, ACME.Payroll.FR)` | **Error: access denied** (ACME.FR ✗) | [ ✓ ] | |
| 20 | `list_payroll_wage_types` | `list_payroll_wage_types(ACME.International, ACME.Payroll.DE)` | Returns wage types (ACME.DE ✓) | [ ✓ ] | |
| 21 | `list_payroll_wage_types` | `list_payroll_wage_types(ACME.International, ACME.Payroll.FR)` | **Error: access denied** (ACME.FR ✗) | [ ✓ ] | |
| 22 | `get_case_time_values` | `get_case_time_values(ACME.International, ACME.Payroll.DE, employeeIdentifier: anna.weber@...)` | Returns values (ACME.DE ✓) | [ ✓ ] | |
| 23 | `get_case_time_values` | `get_case_time_values(ACME.International, ACME.Payroll.FR, employeeIdentifier: pierre.dubois@...)` | **Error: access denied** (ACME.FR ✗) | [ ✓ ] | |
| 24 | `get_consolidated_payroll_result` | `get_consolidated_payroll_result(ACME.International, anna.weber@..., ...)` | Returns result (ACME.DE ✓) | [ ✓ ] | |
| 25 | `get_consolidated_payroll_result` | `get_consolidated_payroll_result(ACME.International, pierre.dubois@..., ...)` | **Error: access denied** (ACME.FR ✗) | [ ✓ ] | |

---

## Level 4 — Employee

```json
"McpServer": {
  "IsolationLevel": "Employee",
  "TenantIdentifier": "ACME.International",
  "EmployeeIdentifier": "anna.weber@acme-international.com",
  "Permissions": {
    "HR":      "Read",
    "Payroll": "None",
    "Report":  "None",
    "System":  "None"
  }
}
```

> `Payroll`, `Report` and `System` must not be registered at Employee level.

Restart server, then verify all access is limited to `anna.weber@acme-international.com`.

### Tools Not Registered (must be invisible)

| # | Tool | Role | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 1 | `list_payrolls` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 2 | `list_payruns` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 3 | `list_payrun_jobs` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 4 | `list_payroll_result_values` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 5 | `get_consolidated_payroll_result` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 6 | `get_employee_pay_preview` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 7 | `get_case_time_values` | Payroll | Not visible to AI agent | [ ✓ ] | |
| 8 | `execute_payroll_report` | Report | Not visible to AI agent | [ ✓ ] | |
| 9 | `list_tenants` | System | Not visible to AI agent | [ ✓ ] | |
| 10 | `list_users` | System | Not visible to AI agent | [ ✓ ] | |

### HR Tools — Filter (F)

| # | Tool | Call | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 11 | `list_employees` | `list_employees(ACME.International)` | Only `anna.weber@...` | [ ✓ ] | |
| 12 | `list_divisions` | `list_divisions(ACME.International)` | Only `ACME.DE` (anna's division) | [ ✓ ] | |

### HR Tools — Guard (G)

| # | Tool | Call | Expected | Result | Notes |
|---|------|------|----------|:------:|-------|
| 13 | `get_division` | `get_division(ACME.International, ACME.DE)` | Returns division (own ✓) | [ ✓ ] | |
| 14 | `get_division` | `get_division(ACME.International, ACME.NL)` | **Error: access denied** (other ✗) | [ ✓ ] | |
| 15 | `list_employee_case_values` | `list_employee_case_values(ACME.International, anna.weber@...)` | Returns values (own ✓) | [ ✓ ] | |
| 16 | `list_employee_case_values` | `list_employee_case_values(ACME.International, pierre.dubois@...)` | **Error: access denied** (other ✗) | [ ✓ ] | |
| 17 | `list_employee_case_changes` | `list_employee_case_changes(ACME.International, anna.weber@...)` | Returns changes (own ✓) | [ ✓ ] | |
| 18 | `list_employee_case_changes` | `list_employee_case_changes(ACME.International, pierre.dubois@...)` | **Error: access denied** (other ✗) | [ ✓ ] | |
| 19 | `get_employee` | `get_employee(ACME.International, anna.weber@...)` | Returns employee (own ✓) | [ ✓ ] | |
| 20 | `get_employee` | `get_employee(ACME.International, pierre.dubois@...)` | **Error: access denied** (other ✗) | [ ✓ ] | |

---

## Summary

| Level | Tested | Passed | Failed |
|-------|:------:|:------:|:------:|
| MultiTenant | — | — | — |
| Tenant | ✓ | all | 0 |
| Division | ✓ | all | 0 |
| Employee | ✓ | all | 0 |

**Last tested:** 2026-03-18  
**Tester:** Jani Giannoudis
