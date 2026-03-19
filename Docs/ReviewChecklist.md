# MCP Server — Review Checklist

Kompakte Checkliste für den Pre-Release Review des PayrollEngine MCP Servers (`v0.1-preview`).

**Reviewer:** Jani Giannoudis  
**Datum:** 2026-03-18  
**Version:** `0.1-preview`

---

## 1. Architektur & Struktur

| # | Punkt | OK |
|---|-------|:--:|
| 1.1 | `ToolBase` — alle Helper-Methoden dokumentiert und korrekt abgegrenzt | ✅ |
| 1.2 | `ToolRegistrar` — Role × IsolationLevel Matrix vollständig implementiert | ✅ |
| 1.3 | `IsolationContext` — alle Felder validiert (`Validate()`) | ✅ |
| 1.4 | `Program.cs` — Startup-Logging deckt alle relevanten Konfigurationswerte ab | ✅ |
| 1.5 | `ServerInfoTools` — kein `[ToolRole]`, immer registriert | ✅ |
| 1.6 | Alle Tool-Klassen haben `[McpServerToolType]` und `[ToolRole]` (ausser `ServerInfoTools`) | ✅ |

---

## 2. Isolation Level

| # | Punkt | OK |
|---|-------|:--:|
| 2.1 | `MultiTenant` — kein Filter, alle Tenants sichtbar | ✅ |
| 2.2 | `Tenant` — `EffectiveTenant` überschreibt AI-Eingabe, `IsolatedTenantQuery` filtert `list_tenants` | ✅ |
| 2.3 | `Division` — `IsolatedDivisionQueryAsync`, `IsolatedPayrollQueryAsync`, `IsolatedPayrunQueryAsync` korrekt | ✅ |
| 2.4 | `Division` — `FilterEmployeesByIsolation` client-seitig (OData `divisions/any()` nicht unterstützt) | ✅ |
| 2.5 | `Division` — Guard-Inline-Code in `get_payroll`, `list_payroll_wage_types`, `get_payroll_lookup_value` | ✅ |
| 2.6 | `Employee` — `EffectiveEmployee` überschreibt AI-Eingabe | ✅ |
| 2.7 | `Employee` — `FilterEmployeesByIsolation` auf `list_employees` | ✅ |
| 2.8 | `Employee` — Guard in `get_division`, `get_division_attribute` | ✅ |
| 2.9 | `AssertEmployeeInDivision` auf allen employee-scoped Tools bei Division-Level | ✅ |

---

## 3. Rollen & Permissions

| # | Punkt | OK |
|---|-------|:--:|
| 3.1 | HR: alle 10 Tools korrekt mit `[ToolRole(McpRole.HR)]` | ✅ |
| 3.2 | Payroll: alle 10 Tools korrekt mit `[ToolRole(McpRole.Payroll)]` | ✅ |
| 3.3 | Report: `execute_payroll_report` mit `[ToolRole(McpRole.Report)]` | ✅ |
| 3.4 | System: alle 6 Tools korrekt mit `[ToolRole(McpRole.System)]` | ✅ |
| 3.5 | `McpPermissions.FromConfiguration` liest `Permissions`-Sektion korrekt | ✅ |
| 3.6 | Default wenn kein `Permissions`-Block: alle Rollen `Read` | ✅ |

---

## 4. Sicherheit

| # | Punkt | OK |
|---|-------|:--:|
| 4.1 | `User.Password` — `[JsonIgnore]` gesetzt, kein Hash in API-Response | ✅ |
| 4.2 | `PreviewUserIdentifier` — wird geprüft bevor Preview/Report-Tools aufgerufen werden | ✅ |
| 4.3 | Employee-Guard — fremde Mitarbeiter bei Division/Employee-Level korrekt abgelehnt | ✅ |
| 4.4 | Payroll-Guard — fremde Payrolls bei Division-Level korrekt abgelehnt | ✅ |
| 4.5 | Division-Guard — fremde Divisionen bei Employee-Level korrekt abgelehnt | ✅ |
| 4.6 | Read-Only — keine Write-Operationen in Tool-Klassen | ✅ |

---

## 5. Backend-Fixes

| # | Punkt | OK |
|---|-------|:--:|
| 5.1 | MySQL `MapSpParameters` — `@`-Prefix-Handling korrekt (nur Return-Value überspringen) | ✅ |
| 5.2 | `ParameterDeleteTenant.TenantId` — `@tenantId` korrekt | ✅ |
| 5.3 | `EmployeeCaseValueRepository` — `DbParameterCollection` statt Anonymous Object | ✅ |
| 5.4 | `ReportQueryTools` — `UserId` vor Report-Execute aufgelöst | ✅ |

---

## 6. Tests

| # | Punkt | OK |
|---|-------|:--:|
| 6.1 | `ToolRegistrarTest` — alle 6 Runden (HR, Payroll, Report, System, Specialist, AllNone) | ✅ |
| 6.2 | `AllNone_ServerInfoStillRegistered` — `ServerInfoTools` immer registriert | ✅ |
| 6.3 | `AllRead_TotalToolCount_Is12` — 12 Tool-Klassen total | ✅ |
| 6.4 | `IsolationContextTest` — Validation-Tests vorhanden | ✅ |
| 6.5 | Isolation Level Tests bestanden (Tenant, Division, Employee) | ✅ |
| 6.6 | Rollen Tests bestanden (alle 6 Runden) | ✅ |

---

## 7. Konfiguration & Deployment

| # | Punkt | OK |
|---|-------|:--:|
| 7.1 | `appsettings.json` — Log-Pfad auf `C:/ProgramData/...` | ✅ |
| 7.2 | `appsettings.json` — Kommentare vollständig und korrekt | ✅ |
| 7.3 | `apisettings.json` — in `.gitignore` ausgeschlossen | ✅ |
| 7.4 | `claude_desktop_config.json` — `--no-build` Flag dokumentiert | ✅ |
| 7.5 | `Dockerfile` — vorhanden und aktuell | ✅ |
| 7.6 | `Directory.Build.props` — Version `0.1-preview` korrekt | ✅ |

---

## 8. Dokumentation

| # | Punkt | OK |
|---|-------|:--:|
| 8.1 | README — `get_server_info` in Tool-Sektion, Tool × Role, Tool × Isolation Level | ✅ |
| 8.2 | README — Fussnoten ¹ (client-seitiger Employee-Filter) und ² (`get_case_time_values` Guard-Scope) | ✅ |
| 8.3 | README — Persona Examples vollständig | ✅ |
| 8.4 | Website `McpServer.md` — `get_server_info` ergänzt | ✅ |
| 8.5 | `TestChecklist-IsolationLevel.md` — alle Tests als bestanden markiert | ✅ |
| 8.6 | `TestChecklist-Roles.md` — alle Tests als bestanden markiert | ✅ |

---

## 9. Bekannte Einschränkungen (by design)

| # | Punkt | Dokumentiert |
|---|-------|:------------:|
| 9.1 | `get_case_time_values` ohne `employeeIdentifier` bei Division-Level — kein Division-Scope | ✓ (Fussnote ²) |
| 9.2 | `list_employees` Division-Filter client-seitig (Backend kein OData Lambda) | ✓ (Fussnote ¹) |
| 9.3 | Read-Only by Design — keine Write-Operationen | ✓ (README) |
| 9.4 | `get_employee_pay_preview` und `execute_payroll_report` benötigen `PreviewUserIdentifier` | ✓ (README) |

---

## Fixes im Zuge des Reviews

| # | Datei | Änderung |
|---|-------|----------|
| F1 | `PayrollPreviewTools.cs` | `AssertEmployeeInDivision(employee)` nach `ResolveEmployeeAsync` ergänzt (Division-Guard fehlte) |
| F2 | `README.md` (McpServer) | `--no-build` Flag in Claude Desktop Config ergänzt |
| F3 | `McpServer.md` (Website) | `--no-build` Flag in Claude Desktop und Cursor Config ergänzt |


