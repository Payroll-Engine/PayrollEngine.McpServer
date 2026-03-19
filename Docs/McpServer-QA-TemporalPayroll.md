# MCP Server — Q&A Test Set

**Example:** TemporalPayroll · Tenant `TemporalAG`
**Purpose:** Verification of MCP tool responses against known test data
**Reference:** `PayrollEngine/Examples/TemporalPayroll/Payroll.tp.yaml`

---

## Tool Inventory

| Role | Tools |
|---|---|
| **System** | `list_tenants` · `get_tenant` · `get_tenant_attribute` · `list_users` · `get_user` · `get_user_attribute` |
| **HR** | `list_divisions` · `get_division` · `get_division_attribute` · `list_employees` · `get_employee` · `get_employee_attribute` · `list_employee_case_values` · `list_company_case_values` · `list_employee_case_changes` · `list_company_case_changes` |
| **Payroll** | `list_payrolls` · `get_payroll` · `list_payruns` · `list_payrun_jobs` · `list_payroll_wage_types` · `get_payroll_lookup_value` · `get_case_time_values` · `get_employee_pay_preview` · `list_payroll_result_values` · `get_consolidated_payroll_result` |
| **Report** | `execute_payroll_report` |

> `get_employee_pay_preview` and `execute_payroll_report` require
> `McpServer:PreviewUserIdentifier` to be configured in `appsettings.json`.

---

## Reference Data

| Parameter | Value |
|---|---|
| Tenant | `TemporalAG` |
| Employee | `alex.meyer@temporalag.com` (Alex Meyer) |
| Division | `TemporalAG.HR` |
| User | `hr@temporalag.com` |
| Payroll | `TemporalPayroll` |
| Payrun | `TemporalPayrun` |
| Regulation | `TemporalRegulation` |
| Wage Type | 100 · Salary |
| Collector | GrossIncome |
| Forecast | Budget2026 |

### Salary Entries

| # | Created | Valid From | Valid To | Value | Forecast |
|---|---|---|---|---|---|
| 1 | 2026-01-12 | 2026-02-01 | open | 5,000 | — |
| 2 | 2026-05-17 | 2026-04-01 | open | 5,500 | — |
| 3 | 2026-07-13 | 2026-10-01 | 2026-11-30 | 6,000 | Budget2026 |

**Reference point Now** = `2026-09-30`

---

## Part 1 — Per Role

### Role: System

**Tools:** `list_tenants` · `get_tenant` · `list_users` · `get_user`

---

**Q 1.1** List all tenants. How many are returned, and what is the identifier of the first one?

> **A:** At least one tenant. Identifier: `TemporalAG`, culture `en-US`.
> Tool: `list_tenants`

---

**Q 1.2** Get tenant `TemporalAG` directly by identifier.

> **A:** Returns the tenant object with `identifier = "TemporalAG"` and `culture = "en-US"`.
> Tool: `get_tenant(identifier: "TemporalAG")`

---

**Q 1.3** List all users of tenant `TemporalAG`. Who is listed?

> **A:** Exactly one user: `hr@temporalag.com` (HR Admin), `userType = TenantAdministrator`.
> Tool: `list_users(tenantIdentifier: "TemporalAG")`

---

**Q 1.4** Get user `hr@temporalag.com` within tenant `TemporalAG`.

> **A:** Returns the user object with `firstName = "HR"`, `lastName = "Admin"`, `culture = "en-US"`.
> Tool: `get_user(tenantIdentifier: "TemporalAG", userIdentifier: "hr@temporalag.com")`

---

### Role: HR

**Tools:** `list_divisions` · `list_employees` · `get_employee` · `list_employee_case_values` · `list_employee_case_changes`

---

**Q 2.1** List all divisions of tenant `TemporalAG`.

> **A:** Exactly one division: `TemporalAG.HR`.
> Tool: `list_divisions(tenantIdentifier: "TemporalAG")`

---

**Q 2.2** List all employees of tenant `TemporalAG`.

> **A:** Exactly one employee: `alex.meyer@temporalag.com` (Alex Meyer), division `TemporalAG.HR`.
> Tool: `list_employees(tenantIdentifier: "TemporalAG")`

---

**Q 2.3** Get employee `alex.meyer@temporalag.com` directly.

> **A:** Returns employee object with `firstName = "Alex"`, `lastName = "Meyer"`.
> Tool: `get_employee(tenantIdentifier: "TemporalAG", employeeIdentifier: "alex.meyer@temporalag.com")`

---

**Q 2.4** List all employees with filter `lastName eq 'Meyer'`.

> **A:** Returns one employee: Alex Meyer.
> Tool: `list_employees(tenantIdentifier: "TemporalAG", filter: "lastName eq 'Meyer'")`

---

**Q 2.5** List all case values for employee Alex Meyer. How many entries exist for field `Salary`?

> **A:** Three entries for `Salary` (incl. forecast entry #3 tagged `Budget2026`).
> Tool: `list_employee_case_values(tenantIdentifier: "TemporalAG", employeeIdentifier: "alex.meyer@temporalag.com")`

---

**Q 2.6** What are the `start`, `end`, and `value` of each salary entry?

> **A:**
> - #1: start = `2026-02-01`, end = null, value = `5000`
> - #2: start = `2026-04-01`, end = null, value = `5500`
> - #3: start = `2026-10-01`, end = `2026-11-30`, value = `6000`, forecast = `Budget2026`

---

**Q 2.7** List case changes for Alex Meyer. How many changes are recorded?

> **A:** Three case changes — one per salary entry import.
> Each change contains user `hr@temporalag.com`, the reason text, and the affected `Salary` value.
> Tool: `list_employee_case_changes(tenantIdentifier: "TemporalAG", employeeIdentifier: "alex.meyer@temporalag.com")`

---

**Q 2.8** Filter case changes with `reason eq 'Entry #2 — salary adjustment, entered retroactively in May'`.

> **A:** Returns exactly one change record showing the retroactive entry of 5,500 (created 2026-05-17, valid from 2026-04-01).
> Tool: `list_employee_case_changes(..., filter: "reason eq 'Entry #2 — salary adjustment, entered retroactively in May'")`

---

### Role: Payroll

**Tools:** `list_payrolls` · `get_payroll` · `list_payruns` · `list_payrun_jobs` · `list_payroll_wage_types` · `get_case_time_values` · `get_employee_pay_preview` · `list_payroll_result_values` · `get_consolidated_payroll_result`

---

**Q 3.1** List all payrolls of tenant `TemporalAG`.

> **A:** Exactly one payroll: `TemporalPayroll`, division `TemporalAG.HR`.
> Tool: `list_payrolls(tenantIdentifier: "TemporalAG")`

---

**Q 3.2** List all payruns of tenant `TemporalAG`.

> **A:** Exactly one payrun: `TemporalPayrun`, payroll `TemporalPayroll`.
> Tool: `list_payruns(tenantIdentifier: "TemporalAG")`

---

**Q 3.3** List all payrun jobs of tenant `TemporalAG`. How many jobs exist?

> **A:** 7 jobs, ordered by creation date descending.
> Names: `TemporalPayrun.Retro.A/B/C/D` and `TemporalPayrun.Forecast.A/B/C`.
> Tool: `list_payrun_jobs(tenantIdentifier: "TemporalAG")`

---

**Q 3.4** List all wage types of payroll `TemporalPayroll`.

> **A:** One wage type: number `100`, name `Salary`, collector `GrossIncome`.
> Tool: `list_payroll_wage_types(tenantIdentifier: "TemporalAG", payrollName: "TemporalPayroll")`

---

**Q 3.5** Get the case time value for field `Salary` with `valueDate = 2026-09-01` and `evaluationDate = 2026-09-30`.

> **A:** Returns entry #2: value `5500`, valid from `2026-04-01`.
> (Both #1 and #2 visible; #2 wins as latest start date.)
> Tool: `get_case_time_values(tenantIdentifier: "TemporalAG", payrollName: "TemporalPayroll", caseFieldNames: "Salary", valueDate: "2026-09-01", evaluationDate: "2026-09-30", employeeIdentifier: "alex.meyer@temporalag.com")`

---

**Q 3.6** Get the case time value for field `Salary` with `valueDate = 2026-06-01` and `evaluationDate = 2026-02-01`.

> **A:** Returns entry #1: value `5000` (entry #2 hidden — knowledge cutoff Feb 1).
> Tool: `get_case_time_values(..., valueDate: "2026-06-01", evaluationDate: "2026-02-01", ...)`

---

**Q 3.7** Get the case time value for field `Salary` with `valueDate = 2026-10-01`, `evaluationDate = 2026-10-31`, and `forecast = Budget2026`.

> **A:** Returns entry #3: value `6000`, valid `2026-10-01` to `2026-11-30`.
> Tool: `get_case_time_values(..., valueDate: "2026-10-01", evaluationDate: "2026-10-31", forecast: "Budget2026", ...)`

---

**Q 3.8** Preview the payroll for Alex Meyer, payrun `TemporalPayrun`, period `2026-09-01`. What result does WageType 100 show?

> **A:** 5,500 — current knowledge (EvalDate = now), entry #2 wins for September.
> Equivalent to job `TemporalPayrun.Retro.A`.
> Tool: `get_employee_pay_preview(tenantIdentifier: "TemporalAG", employeeIdentifier: "alex.meyer@temporalag.com", payrunName: "TemporalPayrun", periodStart: "2026-09-01")`

---

**Q 3.9** Preview payroll for Alex Meyer, period `2026-10-01`, with `forecast = Budget2026`. What result?

> **A:** 6,000 — entry #3 (Oct–Nov) activated by forecast.
> Equivalent to job `TemporalPayrun.Forecast.B`.
> Tool: `get_employee_pay_preview(..., periodStart: "2026-10-01", forecast: "Budget2026")`

---

**Q 3.10** List all payroll result values for tenant `TemporalAG`. How many rows are returned?

> **A:** 7 × 2 = 14 rows (7 jobs × 1 wage type + 1 collector each), plus potential payrun result rows.
> Tool: `list_payroll_result_values(tenantIdentifier: "TemporalAG")`

---

**Q 3.11** Filter payroll result values with `filter: "periodName eq 'September 2026'"`. What values appear?

> **A:** Results from `Retro.A` (5,500) and `Retro.C` (5,000) — both jobs use `periodStart = 2026-09-01`.

---

**Q 3.12** Get consolidated payroll result for Alex Meyer, period `2026-09-01` to `2026-09-30`.

> **A:** Returns all wage type, collector, and payrun results for Alex Meyer in September.
> WageType 100 from `Retro.A` = 5,500; from `Retro.C` = 5,000 (different evalDates).
> Tool: `get_consolidated_payroll_result(tenantIdentifier: "TemporalAG", employeeIdentifier: "alex.meyer@temporalag.com", periodStart: "2026-09-01", periodEnd: "2026-09-30")`

---

### Role: Report

**Tool:** `execute_payroll_report`
*(Requires `PreviewUserIdentifier` configured in `appsettings.json`)*

---

**Q 4.1** Execute a payroll report (any report available in `TemporalPayroll`). Does the response contain `reportName` and `result`?

> **A:** Yes — the response always includes `reportName`, `culture`, `parameters`, and `result`
> (with one or more named tables).
> Tool: `execute_payroll_report(tenantIdentifier: "TemporalAG", payrollName: "TemporalPayroll", reportName: "<ReportName>")`

---

## Part 2 — Per Isolation Level

### Retro A — EvalDate = Today · ValueDate = Today

**EvalDate:** 2026-09-30 | **PeriodStart:** 2026-09-01 | **Forecast:** —

---

**Q R-A.1** Which salary entries are visible at EvalDate = 2026-09-30?

> **A:** #1 (created Jan 12 ≤ Sep 30) and #2 (created May 17 ≤ Sep 30).
> Verify with: `get_case_time_values(..., valueDate: "2026-09-01", evaluationDate: "2026-09-30")`

**Q R-A.2** What result does WageType 100 show in job `TemporalPayrun.Retro.A`?

> **A:** **5,500** — entry #2 (from Apr 1) is the latest valid entry at Sep 1.
> Verify with: `list_payroll_result_values(..., filter: "jobName eq 'TemporalPayrun.Retro.A'")`

**Q R-A.3** Does the consolidated result for September also contain `Retro.A = 5,500`?

> **A:** Yes. `get_consolidated_payroll_result(..., periodStart: "2026-09-01", periodEnd: "2026-09-30")`
> returns wage type 100 = 5,500 from job `TemporalPayrun.Retro.A`.

---

### Retro B — EvalDate = Feb 1 · ValueDate = Jun 1

**EvalDate:** 2026-02-01 | **PeriodStart:** 2026-06-01 | **Forecast:** —

---

**Q R-B.1** Why is entry #2 not returned by `get_case_time_values` with `evaluationDate = 2026-02-01`?

> **A:** Entry #2 was created 2026-05-17 > 2026-02-01. The knowledge cutoff hides it.

**Q R-B.2** What result does job `TemporalPayrun.Retro.B` show for WageType 100?

> **A:** **5,000** — only entry #1 visible; open-ended, covers June.
> Verify: `list_payroll_result_values(..., filter: "jobName eq 'TemporalPayrun.Retro.B'")`

**Q R-B.3** Does `get_case_time_values` with `valueDate = 2026-06-01` and `evaluationDate = 2026-02-01` return 5,000?

> **A:** Yes — identical perspective to Retro B.

---

### Retro C — EvalDate = Feb 1 · ValueDate = Today (Sep)

**EvalDate:** 2026-02-01 | **PeriodStart:** 2026-09-01 | **Forecast:** —

---

**Q R-C.1** Does `get_case_time_values` with `valueDate = 2026-09-01` and `evaluationDate = 2026-02-01` return a different value than Retro B?

> **A:** No — same knowledge cutoff, same visible entry (#1), same result: **5,000**.

**Q R-C.2** What result does job `TemporalPayrun.Retro.C` show?

> **A:** **5,000** — Feb knowledge, even though the value date is September.

**Q R-C.3** Can `evaluationDate` be set earlier than `periodStart` in `get_case_time_values`?

> **A:** Yes. PE treats both axes as fully independent — Retro B and C prove this is valid.

---

### Retro D — EvalDate = Today · ValueDate = Jun 1

**EvalDate:** 2026-09-30 | **PeriodStart:** 2026-06-01 | **Forecast:** —

---

**Q R-D.1** What does `get_case_time_values` return with `valueDate = 2026-06-01` and `evaluationDate = 2026-09-30`?

> **A:** Entry #2: value **5,500** — today's knowledge, entry #2 (from Apr) wins at Jun 1.

**Q R-D.2** What result does job `TemporalPayrun.Retro.D` show?

> **A:** **5,500** — same perspective as R-D.1.

**Q R-D.3** Retro B and Retro D both use `periodStart = 2026-06-01`. Why do they differ?

> **A:** `evaluationDate` is the only difference: Feb 1 vs Sep 30.
> Entry #2 is invisible in Feb → 5,000; visible in Sep → 5,500.

---

### Forecast A — EvalDate = Oct 31 · ValueDate = Oct · no Forecast

**EvalDate:** 2026-10-31 | **PeriodStart:** 2026-10-01 | **Forecast:** —

---

**Q F-A.1** What does `get_case_time_values` return for `valueDate = 2026-10-01`, `evaluationDate = 2026-10-31`, no forecast?

> **A:** Entry #2: value **5,500** — entry #3 excluded (forecast-tagged).

**Q F-A.2** What result does job `TemporalPayrun.Forecast.A` show?

> **A:** **5,500**. JobStatus = `Complete`.

**Q F-A.3** Does `list_payroll_result_values` filtered by `jobName eq 'TemporalPayrun.Forecast.A'` confirm 5,500?

> **A:** Yes — WageType 100 = 5,500, GrossIncome = 5,500.

---

### Forecast B — EvalDate = Oct 31 · ValueDate = Oct · Budget2026

**EvalDate:** 2026-10-31 | **PeriodStart:** 2026-10-01 | **Forecast:** Budget2026

---

**Q F-B.1** What does `get_case_time_values` return with `forecast = Budget2026` for October?

> **A:** Entry #3: value **6,000** (Oct 1–Nov 30).

**Q F-B.2** What result does job `TemporalPayrun.Forecast.B` show?

> **A:** **6,000**. JobStatus = `Forecast`.

**Q F-B.3** Forecast A and Forecast B use the same period and EvalDate — what is the only difference?

> **A:** The `forecast` parameter. `null` → 5,500 (production); `Budget2026` → 6,000 (forecast entry #3 activated).

---

### Forecast C — EvalDate = Dec 31 · ValueDate = Dec · Budget2026

**EvalDate:** 2026-12-31 | **PeriodStart:** 2026-12-01 | **Forecast:** Budget2026

---

**Q F-C.1** What does `get_case_time_values` return with `forecast = Budget2026` for December?

> **A:** Entry #2: value **5,500** — entry #3 expires Nov 30, December falls outside its window.

**Q F-C.2** What result does job `TemporalPayrun.Forecast.C` show?

> **A:** **5,500**. JobStatus = `Forecast`.

**Q F-C.3** What does Forecast C demonstrate about forecast entry expiry?

> **A:** A forecast entry's `End` date is enforced exactly like a production entry's.
> Budget2026 is still active (forecast name on the job), but entry #3 has expired → fallback to #2.

---

## Part 3 — Cross-Cutting Questions

---

**Q X.1** How many payrun jobs in `TemporalAG` have `jobStatus = Forecast`?

> **A:** 2 — `TemporalPayrun.Forecast.B` and `TemporalPayrun.Forecast.C`.
> Verify: `list_payrun_jobs(tenantIdentifier: "TemporalAG")` → check `jobStatus` field.

**Q X.2** Which jobs return value 5,000 for WageType 100?

> **A:** `TemporalPayrun.Retro.B` and `TemporalPayrun.Retro.C`.
> Verify: `list_payroll_result_values(..., filter: "value eq 5000 and resultKind eq 'WageType'")`

**Q X.3** Which job is the only one returning value 6,000?

> **A:** `TemporalPayrun.Forecast.B`.

**Q X.4** How many result rows return value 5,500 for WageType 100?

> **A:** 4 — Retro A, Retro D, Forecast A, Forecast C.

**Q X.5** Can a preview call for Alex Meyer in October without a forecast return 6,000?

> **A:** No. Without `forecast = Budget2026`, entry #3 is excluded → result 5,500.

**Q X.6** Does `list_employee_case_values` return entry #3 (Budget2026)?

> **A:** Yes — `list_employee_case_values` returns the raw case value store including all forecast entries.
> The forecast tag is visible in the response.

**Q X.7** What is the difference between `evaluationDate` and `periodStart` in one sentence?

> **A:** `periodStart` (ValueDate) determines **which value was active on that date**;
> `evaluationDate` determines **which entries were visible in the system at that time**.

---

## MCP Tool Mapping (Complete)

| Questions | Tool |
|---|---|
| Q 1.1 | `list_tenants` |
| Q 1.2 | `get_tenant` |
| Q 1.3 | `list_users` |
| Q 1.4 | `get_user` |
| Q 2.1 | `list_divisions` |
| Q 2.2 | `list_employees` |
| Q 2.3 | `get_employee` |
| Q 2.4 | `list_employees` (with filter) |
| Q 2.5, Q 2.6 | `list_employee_case_values` |
| Q 2.7, Q 2.8 | `list_employee_case_changes` |
| Q 3.1 | `list_payrolls` |
| Q 3.2 | `list_payruns` |
| Q 3.3 | `list_payrun_jobs` |
| Q 3.4 | `list_payroll_wage_types` |
| Q 3.5–3.7, Q R-* / Q F-* | `get_case_time_values` |
| Q 3.8, Q 3.9, Q X.5 | `get_employee_pay_preview` |
| Q 3.10, Q 3.11, Q R-A.2, Q R-B.2, … | `list_payroll_result_values` |
| Q 3.12, Q R-A.3 | `get_consolidated_payroll_result` |
| Q 4.1 | `execute_payroll_report` |
| Q X.1 | `list_payrun_jobs` |
| Q X.2, Q X.4 | `list_payroll_result_values` (with filter) |
| Q X.6 | `list_employee_case_values` |

---

## Configuration Checklist

Before running Q 3.8, Q 3.9, Q 4.1 — verify `appsettings.json`:

```json
"McpServer": {
  "PreviewUserIdentifier": "hr@temporalag.com"
}
```

---

*Generated for MCP Server v0.1-preview · TemporalPayroll Example*
