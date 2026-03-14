# Case Value Temporal Perspectives

## Overview

PayrollEngine stores case values with temporal metadata: every value has a `Start` and `End`
date that defines the period in which it is valid. This allows the system to maintain a full
history of changes — for example, salary adjustments, address changes, or bank account updates
over time.

When querying case values, two independent date parameters control *which data is returned*:

| Parameter | Controls |
|:--|:--|
| `valueDate` | **What** is valid — selects values whose `Start ≤ valueDate < End` |
| `evaluationDate` | **When** the query is evaluated — filters out values created after this date |

The combination of these two parameters defines the **temporal perspective** of the query.

---

## The Two Time Axes

### valueDate — "What was valid on this day?"

`valueDate` answers the question: *"Which value was active on a given date?"*

A case field like `Salary` might have multiple entries in the database:

```
Salary = 5000   Start = 2022-01-01   End = 2024-12-31
Salary = 6000   Start = 2025-01-01   End = null       (open-ended, currently active)
```

With `valueDate = 2023-06-01`, the result is `5000`.  
With `valueDate = 2025-06-01`, the result is `6000`.

### evaluationDate — "What did the system know at this point?"

`evaluationDate` answers the question: *"Which entries were already recorded on a given date?"*

It filters out entries whose `Created` timestamp is later than the evaluation date. This is
relevant for retroactive corrections: if a salary correction is entered today but backdated to
January 1st, it has `Start = 2025-01-01` but `Created = today`.

- With `evaluationDate = today`: the correction **is visible**.
- With `evaluationDate = 2025-01-01`: the correction **is not visible** (it was entered later).

---

## Three Temporal Perspectives

### 1 — Historical View

> "What was the value on date X, as the system knew it at that time?"

```
valueDate      = 2026-01-01
evaluationDate = 2026-01-01   ← same as valueDate
forecast       = null
```

**Use case:** Audit, compliance, retroactive payroll verification. Answers the question exactly
as the system would have answered it on that date — retroactive corrections entered later are
excluded.

**Key rule:** `evaluationDate = valueDate`

---

### 2 — Current Knowledge View

> "What was the value on date X, applying everything we know today?"

```
valueDate      = 2026-01-01
evaluationDate = today        ← or omitted (defaults to today)
forecast       = null
```

**Use case:** Controlling, reporting, data quality checks. All retroactive corrections that have
been entered since the original date are included. This is typically what a payroll analyst
wants when reviewing historical periods.

**Key rule:** `evaluationDate` is today (or omitted)

---

### 3 — Forecast View

> "What will the value be on a future date, including planned changes?"

```
valueDate      = 2026-07-01
evaluationDate = 2026-07-01   ← same as valueDate (not today)
forecast       = "PlanName"
```

**Use case:** Budget planning, salary projections, headcount forecasting.

The `forecast` parameter selects a named set of planned case value entries. These entries exist
in the database alongside real values but are tagged with the forecast name — they are excluded
from normal queries.

**Why `evaluationDate = valueDate` for forecasts?**

Forecast entries are future-dated: their `Created` timestamp is today, but their `Start` date
is in the future. If `evaluationDate = today`, the filter `Created ≤ evaluationDate` would
accept forecast entries created today — but entries whose `Start` is between today and `valueDate`
(the actual planned changes) would be filtered out because their `Start` exceeds today.

Setting `evaluationDate = valueDate` ensures that all planned changes up to the target date are
included in the result.

**Key rule:** `evaluationDate = valueDate`, `forecast = "<name>"`

---

## Summary Table

| Perspective | valueDate | evaluationDate | forecast | Typical question |
|:--|:--|:--|:--|:--|
| **Historical** | target date | = valueDate | null | "What was true on Jan 1, as of Jan 1?" |
| **Current knowledge** | target date | today (default) | null | "What do we now know about Jan 1?" |
| **Forecast** | future date | = valueDate | name | "What will be true on Jul 1 per the plan?" |

---

## API Parameters

The `get_case_time_values` MCP tool and the underlying `GetCaseTimeValuesAsync` method expose
these parameters:

| Parameter | Type | Description |
|:--|:--|:--|
| `valueDate` | `string` (ISO 8601) | The point in time for value validity. Default: today. |
| `evaluationDate` | `string` (ISO 8601) | The knowledge cutoff date. Default: today. |
| `forecast` | `string` | Forecast name. `null` = real values only. |
| `employeeIdentifier` | `string` | For `CaseType.Employee`: employee identifier, or omit for all employees. Resolved internally to the employee ID. |
| `caseFieldNames` | `string` (comma-separated) | Filter by specific fields, e.g. `"Salary,City,IBAN"`. Omit for all fields. |

---

## Example: Salary Report as of December 31, 2025

**Scenario:** A payroll controller wants to see the salary of all employees as it was recorded
at year-end 2025 — excluding any corrections entered in 2026.

```
caseType            = Employee
caseFieldNames      = "Salary"
valueDate           = 2025-12-31
evaluationDate      = 2025-12-31   ← historical: only what was known on that date
forecast            = null
employeeIdentifier  = (omit)       ← all employees (tenant-wide)
```

**Scenario:** Same report, but including corrections entered retroactively after year-end:

```
valueDate      = 2025-12-31
evaluationDate = (today)      ← current knowledge: all corrections visible
```

**Scenario:** Budget planning — projected salary costs for July 2026 per forecast "Budget2026":

```
valueDate      = 2026-07-01
evaluationDate = 2026-07-01   ← must match valueDate for forecasts
forecast       = "Budget2026"
```

---

## Implementation Notes

The backend endpoint `GET /payrolls/{id}/cases/values/time` accepts all three parameters.
The `PayrollService.GetCaseTimeValuesAsync` method in `Client.Core` maps them directly to query
string parameters.

The MCP tool accepts `employeeIdentifier` (a human-readable string) and resolves it to the
internal employee ID automatically via `ResolveEmployeeAsync`.

When `caseType = Employee` and `employeeIdentifier` is omitted, the backend executes the
tenant-wide stored procedure `GetEmployeeCaseValuesByTenant`, which returns values for all
active employees in a single query. The `EmployeeId` field on each returned `CaseValue`
identifies the employee.

See also: `GetEmployeeCaseValuesByTenant.sql` in `Persistence.SqlServer/StoredProcedures`.
