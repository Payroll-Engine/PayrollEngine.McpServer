# MCP Server — Q&A Testset

**Beispiel:** TemporalPayroll · Tenant `TemporalAG`
**Referenz:** `PayrollEngine/Examples/TemporalPayroll/Payroll.tp.yaml`

---

## Referenzdaten

| Parameter | Wert |
|---|---|
| Tenant | `TemporalAG` |
| Mitarbeiter | `alex.meyer@temporalag.com` (Alex Meyer) · Division `TemporalAG.HR` |
| Benutzer | `hr@temporalag.com` (HR Admin, TenantAdministrator) |
| Payroll / Payrun | `TemporalPayroll` / `TemporalPayrun` |
| Lohnart / Kollektor | 100 · Salary / GrossIncome |
| Forecast | `Budget2026` |
| Referenzpunkt Now | `2026-09-30` |

### Gehaltseinträge

| # | Erstellt | Gültig ab | Gültig bis | Wert | Forecast |
|---|---|---|---|---|---|
| 1 | 2026-01-12 | 2026-02-01 | offen | 5'000 | — |
| 2 | 2026-05-17 | 2026-04-01 | offen | 5'500 | — |
| 3 | 2026-07-13 | 2026-10-01 | 2026-11-30 | 6'000 | Budget2026 |

---

## Teil 1 — Pro Rolle

### System

| # | Frage | Erwartete Antwort | Tool |
|---|---|---|---|
| 1.1 | Welche Tenants gibt es im System? | Mindestens `TemporalAG`, culture `en-US` | `list_tenants` |
| 1.2 | Was weisst du über den Tenant `TemporalAG`? | `identifier = "TemporalAG"`, `culture = "en-US"` | `get_tenant` |
| 1.3 | Wer hat Zugang zum Tenant `TemporalAG`? | Genau 1 Benutzer: `hr@temporalag.com`, TenantAdministrator | `list_users` |
| 1.4 | Was weisst du über `hr@temporalag.com`? | `firstName = "HR"`, `lastName = "Admin"`, `culture = "en-US"` | `get_user` |

### HR

| # | Frage | Erwartete Antwort | Tool |
|---|---|---|---|
| 2.1 | Welche Divisionen gibt es bei `TemporalAG`? | Genau eine: `TemporalAG.HR` | `list_divisions` |
| 2.2 | Wer arbeitet bei `TemporalAG`? | Genau einer: Alex Meyer, `TemporalAG.HR` | `list_employees` |
| 2.3 | Was weisst du über Alex Meyer? | `firstName = "Alex"`, `lastName = "Meyer"` | `get_employee` |
| 2.4 | Gibt es jemanden mit Nachnamen Meyer? | Ja: Alex Meyer | `list_employees` mit `filter: "lastName eq 'Meyer'"` |
| 2.5 | Wie viele Gehaltseinträge hat Alex Meyer? | 3 Einträge für `Salary` (inkl. Forecast #3) | `list_employee_case_values` |
| 2.6 | Von wann bis wann gilt jeder Eintrag? | #1: Feb–offen 5'000 · #2: Apr–offen 5'500 · #3: Okt–Nov 6'000 Budget2026 | `list_employee_case_values` |
| 2.7 | Wer hat die Stammdaten von Alex Meyer wann geändert? | 3 Änderungen, alle durch `hr@temporalag.com` | `list_employee_case_changes` |
| 2.8 | Zeig mir die rückwirkende Maikorrektur. | Eintrag #2: erstellt 17. Mai, gültig ab 1. April, Wert 5'500 | `list_employee_case_changes` mit Reason-Filter |

### Payroll

| # | Frage | Erwartete Antwort | Tool |
|---|---|---|---|
| 3.1 | Welche Lohnabrechnungen gibt es bei `TemporalAG`? | Genau eine: `TemporalPayroll`, Division `TemporalAG.HR` | `list_payrolls` |
| 3.2 | Welche Payruns laufen unter `TemporalAG`? | Genau einer: `TemporalPayrun` | `list_payruns` |
| 3.3 | Welche Abrechnungsläufe wurden ausgeführt? | 7 Jobs: Retro.A/B/C/D + Forecast.A/B/C | `list_payrun_jobs` |
| 3.4 | Welche Lohnarten sind aktiv? | LW 100 `Salary`, Kollektor `GrossIncome` | `list_payroll_wage_types` |
| 3.5 | Was verdient Alex Meyer im September — heutiger Wissensstand? | 5'500 (Eintrag #2 gewinnt) | `get_case_time_values` `valueDate: 2026-09-01` `evalDate: 2026-09-30` |
| 3.6 | Was wussten wir am 1. Februar über das Junigehalt? | 5'000 (Eintrag #2 noch nicht erfasst) | `get_case_time_values` `valueDate: 2026-06-01` `evalDate: 2026-02-01` |
| 3.7 | Geplantes Oktobergehalt laut Budget2026? | 6'000 (Eintrag #3 freigegeben) | `get_case_time_values` `valueDate: 2026-10-01` `forecast: Budget2026` |
| 3.8 | Was würde die Septemberabrechnung heute ergeben? | 5'500 — entspricht `Retro.A` | `get_employee_pay_preview` `periodStart: 2026-09-01` |
| 3.9 | Oktoberabrechnung mit Forecast Budget2026? | 6'000 — entspricht `Forecast.B` | `get_employee_pay_preview` `periodStart: 2026-10-01` `forecast: Budget2026` |
| 3.10 | Wie viele Abrechnungsergebnisse liegen vor? | 14+ Zeilen (7 Jobs × LW + Kollektor) | `list_payroll_result_values` |
| 3.11 | Was hat die Septemberabrechnung geliefert? | `Retro.A` = 5'500 · `Retro.C` = 5'000 | `list_payroll_result_values` mit Period-Filter |
| 3.12 | Zeig mir alles, was im September abgerechnet wurde. | Alle LW-, Kollektor- und Payrun-Ergebnisse für September | `get_consolidated_payroll_result` `2026-09-01..30` |

### Report

| # | Frage | Erwartete Antwort | Tool |
|---|---|---|---|
| 4.1 | Kannst du mir den Report ausgeben? | Antwort enthält `reportName`, `culture`, `parameters`, `result` | `execute_payroll_report` ⚠️ PreviewUserIdentifier erforderlich |

---

## Teil 2 — Pro Isolation-Level

### Übersicht der 7 Payrun-Jobs

| Job | EvalDate | ValueDate | Forecast | Sichtbare Einträge | Ergebnis LW 100 |
|---|---|---|---|---|---|
| Retro.A | 2026-09-30 | Sep | — | #1 + #2 | **5'500** |
| Retro.B | 2026-02-01 | Jun | — | #1 | **5'000** |
| Retro.C | 2026-02-01 | Sep | — | #1 | **5'000** |
| Retro.D | 2026-09-30 | Jun | — | #1 + #2 | **5'500** |
| Forecast.A | 2026-10-31 | Okt | — | #1 + #2 | **5'500** |
| Forecast.B | 2026-10-31 | Okt | Budget2026 | #1 + #2 + #3 | **6'000** |
| Forecast.C | 2026-12-31 | Dez | Budget2026 | #1 + #2 + #3* | **5'500** |

*#3 abgelaufen (Ende Nov 30) → Fallback auf #2

---

### Retro A — EvalDate = Today · ValueDate = Sep

| # | Frage | Erwartete Antwort |
|---|---|---|
| R-A.1 | Welche Einträge sind am 30. Sep sichtbar? | #1 (Jan 12) und #2 (Mai 17) — beide vor Sep 30 erfasst |
| R-A.2 | Welches Ergebnis zeigt `Retro.A` für LW 100? | **5'500** — Eintrag #2 (ab Apr) hat das späteste Startdatum |
| R-A.3 | Taucht 5'500 auch im konsolidierten Septemberergebnis auf? | Ja — LW 100 = 5'500 aus Job `Retro.A` |

### Retro B — EvalDate = Feb 1 · ValueDate = Jun 1

| # | Frage | Erwartete Antwort |
|---|---|---|
| R-B.1 | Warum fehlt Eintrag #2 beim Stand vom 1. Feb? | Erstellt 17. Mai > 1. Feb — existierte noch nicht |
| R-B.2 | Ergebnis `Retro.B` für LW 100? | **5'000** — nur #1 sichtbar, offen, deckt Juni ab |
| R-B.3 | Was wäre das Junigehalt gewesen, hätte man am 1. Feb abgerechnet? | 5'000 — identische Perspektive wie `Retro.B` |

### Retro C — EvalDate = Feb 1 · ValueDate = Sep

| # | Frage | Erwartete Antwort |
|---|---|---|
| R-C.1 | Ändert sich das Ergebnis mit Feb-Wissen auf September angewendet? | Nein — gleicher Wissen-Cutoff, gleiches Ergebnis: **5'000** |
| R-C.2 | Ergebnis `Retro.C` für LW 100? | **5'000** — Feb-Wissen, auch wenn der Monat September ist |
| R-C.3 | Darf `evaluationDate` vor `periodStart` liegen? | Ja — PE behandelt beide Achsen unabhängig |

### Retro D — EvalDate = Today · ValueDate = Jun 1

| # | Frage | Erwartete Antwort |
|---|---|---|
| R-D.1 | Was wissen wir heute über das Junigehalt? | 5'500 — Eintrag #2 (ab Apr) ist bekannt und gewinnt |
| R-D.2 | Ergebnis `Retro.D` für LW 100? | **5'500** — gleiche Perspektive wie R-D.1 |
| R-D.3 | Retro B und D rechnen beide für Juni — warum verschiedene Werte? | Nur `evaluationDate` unterscheidet sich: Feb → 5'000, Sep → 5'500 |

### Forecast A — EvalDate = Oct 31 · ValueDate = Okt · kein Forecast

| # | Frage | Erwartete Antwort |
|---|---|---|
| F-A.1 | Oktobergehalt ohne Budget-Forecast? | 5'500 — Eintrag #3 bleibt unsichtbar |
| F-A.2 | Ergebnis `Forecast.A` für LW 100? | **5'500** · JobStatus = `Complete` |
| F-A.3 | Stimmt 5'500 in den gespeicherten Resultaten? | Ja — LW 100 = 5'500, GrossIncome = 5'500 |

### Forecast B — EvalDate = Oct 31 · ValueDate = Okt · Budget2026

| # | Frage | Erwartete Antwort |
|---|---|---|
| F-B.1 | Geplantes Oktobergehalt laut Budget2026? | 6'000 — Eintrag #3 (Okt–Nov) wird sichtbar |
| F-B.2 | Ergebnis `Forecast.B` für LW 100? | **6'000** · JobStatus = `Forecast` |
| F-B.3 | Forecast A und B: gleiche Periode, warum verschiedene Werte? | Nur `forecast`-Parameter unterscheidet sich: null → 5'500, Budget2026 → 6'000 |

### Forecast C — EvalDate = Dec 31 · ValueDate = Dez · Budget2026

| # | Frage | Erwartete Antwort |
|---|---|---|
| F-C.1 | Dezembergehalt laut Budget2026? | 5'500 — Eintrag #3 endet 30. Nov, Dez fällt heraus |
| F-C.2 | Ergebnis `Forecast.C` für LW 100? | **5'500** · JobStatus = `Forecast` |
| F-C.3 | Budget2026 aktiv — warum greift der geplante 6'000-Wert nicht? | End-Datum von #3 gilt unabhängig vom Forecast-Tag |

---

## Teil 3 — Übergreifende Fragen

| # | Frage | Erwartete Antwort | Tool |
|---|---|---|---|
| Ü.1 | Wie viele Läufe haben JobStatus `Forecast`? | 2 — `Forecast.B` und `Forecast.C` | `list_payrun_jobs` |
| Ü.2 | Bei welchen Läufen kam 5'000 heraus? | `Retro.B` und `Retro.C` | `list_payroll_result_values` mit Value-Filter |
| Ü.3 | Bei welchem Lauf kam 6'000 heraus? | Nur `Forecast.B` | `list_payroll_result_values` |
| Ü.4 | Wie oft wurde 5'500 für LW 100 abgerechnet? | 4-mal — Retro.A, Retro.D, Forecast.A, Forecast.C | `list_payroll_result_values` |
| Ü.5 | Oktober-Preview ohne Forecast — kann 6'000 erscheinen? | Nein — ohne `Budget2026` bleibt #3 unsichtbar → 5'500 | `get_employee_pay_preview` |
| Ü.6 | Zeigt `list_employee_case_values` auch den Budget2026-Eintrag? | Ja — inkl. Forecast-Einträge mit sichtbarem Forecast-Tag | `list_employee_case_values` |
| Ü.7 | Was ist der Unterschied zwischen `evaluationDate` und `periodStart`? | `periodStart`: welcher Wert war gültig · `evaluationDate`: was wusste das System | — |

---

## Teil 4 — Drei temporale Perspektiven (`get_case_time_values`)

### Perspektivenübersicht

| Perspektive | valueDate | evaluationDate | forecast | Beispiel (Alex Meyer) | Ergebnis |
|---|---|---|---|---|---|
| **Historisch** | Stichtag | = valueDate | — | Apr 1 / Apr 1 | **5'000** |
| **Aktuelles Wissen** | Stichtag | heute (Standard) | — | Apr 1 / heute | **5'500** |
| **Forecast** | Zieldatum | = valueDate | Name | Okt 1 / Okt 1 / Budget2026 | **6'000** |

---

### Perspektive 1 — Historische Sicht

**Regel:** `evaluationDate = valueDate` — spätere Korrekturen ausgeblendet

| # | Frage | Erwartete Antwort |
|---|---|---|
| P1.1 | Was verdiente Alex Meyer am 1. April, so wie das System es damals wusste? | 5'000 — Eintrag #2 (erstellt Mai 17) noch nicht erfasst |
| P1.2 | Was verdiente er am 1. September, so wie das System es damals wusste? | 5'500 — Eintrag #2 (erstellt Mai 17 ≤ Sep 1) bereits vorhanden und gilt ab Apr |
| P1.3 | Wofür eignet sich die historische Sicht? | Audit, Compliance, nachträgliche Lohnprüfungen |

### Perspektive 2 — Aktuelle Wissenssicht

**Regel:** `evaluationDate` weglassen (Default: heute)

| # | Frage | Erwartete Antwort |
|---|---|---|
| P2.1 | Was verdiente Alex Meyer am 1. April — heutiger Wissensstand? | 5'500 — Eintrag #2 rückwirkend ab Apr bekannt |
| P2.2 | Wo liegt der Unterschied zwischen P1.1 (historisch) und P2.1 (aktuell)? | Gleicher `valueDate` (Apr 1), anderes `evaluationDate`: Apr 1 → 5'000 · heute → 5'500 |
| P2.3 | Wofür eignet sich die aktuelle Wissenssicht? | Controlling, Reporting, Qualitätsprüfungen |
| P2.4 | `get_case_time_values` ohne `employeeIdentifier` — was passiert? | Liefert Werte aller aktiven Mitarbeiter des Tenants; `EmployeeId` identifiziert den MA |

### Perspektive 3 — Forecast-Sicht

**Regel:** `evaluationDate = valueDate` + `forecast = "<n>"`

| # | Frage | Erwartete Antwort |
|---|---|---|
| P3.1 | Geplantes Oktobergehalt laut Budget2026? | 6'000 — Eintrag #3 (Okt 1–Nov 30) durch Forecast freigegeben |
| P3.2 | Geplantes Dezembergehalt laut Budget2026? | 5'500 — Eintrag #3 endet 30. Nov, Fallback auf #2 |
| P3.3 | Warum `evaluationDate = valueDate` und nicht `evaluationDate = heute`? | Mit `evalDate = heute`: #3 akzeptiert, aber `Start > heute` → würde am Stichtag nicht greifen. `evalDate = valueDate` schliesst alle geplanten Änderungen bis zum Zieldatum ein. |
| P3.4 | Wofür eignet sich die Forecast-Sicht? | Budgetplanung, Gehaltsprojektionen, Personalplanung |

---

## Tool-Zuordnung

| Tool | Abgedeckte Fragen |
|---|---|
| `list_tenants` | 1.1 |
| `get_tenant` | 1.2 |
| `list_users` | 1.3 |
| `get_user` | 1.4 |
| `list_divisions` | 2.1 |
| `list_employees` | 2.2, 2.4 |
| `get_employee` | 2.3 |
| `list_employee_case_values` | 2.5, 2.6, Ü.6 |
| `list_employee_case_changes` | 2.7, 2.8 |
| `list_payrolls` | 3.1 |
| `list_payruns` | 3.2 |
| `list_payrun_jobs` | 3.3, Ü.1 |
| `list_payroll_wage_types` | 3.4 |
| `get_case_time_values` | 3.5–3.7, R-*, F-*, P1–P3 |
| `get_employee_pay_preview` | 3.8, 3.9, Ü.5 |
| `list_payroll_result_values` | 3.10, 3.11, R-A.2, R-B.2, Ü.2–Ü.4 |
| `get_consolidated_payroll_result` | 3.12, R-A.3 |
| `execute_payroll_report` | 4.1 |

---

## Konfigurationscheckliste

`get_employee_pay_preview` und `execute_payroll_report` erfordern einen konfigurierten Service-Account:

```json
"McpServer": {
  "PreviewUserIdentifier": "hr@temporalag.com"
}
```

---

*MCP Server v0.1-preview · TemporalPayroll Example*
