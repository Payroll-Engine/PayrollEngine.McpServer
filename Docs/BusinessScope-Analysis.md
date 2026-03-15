# MCP Server — Access Control: Role-Based Permissions

## Three Orthogonal Dimensions

The MCP Server access model has three independent axes:

```
IsolationLevel  — which DATA is visible        (Tenant / Division / Employee)
McpRole         — which DOMAIN a tool belongs  (HR / Payroll / Regulation / System)
McpPermission   — what ACCESS level is granted (None / Read / Full)
```

`IsolationLevel` and the role-permission model are fully independent and combine freely.

---

## Why Role + Permission instead of a Flags Enum

The flags-enum BusinessScope approach asks: *"Which domain blocks are active?"*
It is binary per domain — a domain is either on or off.

The role-permission approach asks: *"What can I do in each domain?"*
This is more expressive in two ways:

**1 — Mixed permission levels across roles**

A payroll specialist reads employee master data but does not modify it.
A flags enum cannot express "HR read-only + Payroll write".
A role permission can:

```
HR:      Read    ← can query employees, case values
Payroll: Full    ← can query and submit payrun jobs
```

**2 — Write tools without config redesign**

All tools today are read-only. When write tools arrive (`create_case_change`,
`submit_payrun_job`), the config model does not need to change. An existing deployment
with `HR: Read` automatically excludes future HR write tools without any migration.
Setting `HR: Full` enables them. The concept is established from day one.

---

## Enum Definitions

```csharp
/// <summary>Functional domain to which a tool belongs.</summary>
public enum McpRole
{
    /// <summary>Employee lifecycle, master data, case values.</summary>
    HR,

    /// <summary>Payroll processing: payruns, jobs, payrolls,
    /// temporal case value queries.</summary>
    Payroll,

    /// <summary>Regulation structure: regulations, wage types, lookups.</summary>
    Regulation,

    /// <summary>System administration: tenant setup, user management.</summary>
    System
}

/// <summary>Access level granted for a role. Ordered: None &lt; Read &lt; Full.</summary>
public enum McpPermission
{
    /// <summary>Role tools are not registered — effectively disabled.</summary>
    None,

    /// <summary>Read/query tools only. Write tools are excluded even if available.</summary>
    Read,

    /// <summary>Read and write tools. Required for future mutation operations.</summary>
    Full
}
```

`McpPermission` is intentionally ordered so that `>=` comparisons work:
`Full >= Read >= None`.

---

## Configuration

### appsettings.json

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

### Environment variables

```
McpServer__IsolationLevel=Tenant
McpServer__TenantIdentifier=acme-corp
McpServer__Permissions__HR=Read
McpServer__Permissions__Payroll=Full
McpServer__Permissions__Regulation=None
McpServer__Permissions__System=None
```

### Default behaviour

| Config state | Effective permission |
|:-------------|:--------------------|
| `Permissions` section absent | All roles: `Full` — backward-compatible |
| Section present, role key absent | That role: `None` |
| `None` | Tool class not registered — not visible to AI agent |

---

## Tool-to-Role Mapping

Each tool class carries a `[ToolRole]` attribute declaring its role and the minimum
permission required to register it.

| Tool class | Role | Min. permission |
|:-----------|:-----|:----------------|
| `TenantQueryTools` | System | Read |
| `StartTenantTool` | System | Full |
| `UserQueryTools` | System | Read |
| `DivisionQueryTools` | HR | Read |
| `EmployeeQueryTools` | HR | Read |
| `CaseValueQueryTools` | HR | Read |
| `CaseChangeQueryTools` | HR | Read |
| `RegulationQueryTools` | Regulation | Read |
| `PayrollQueryTools` | Payroll | Read |
| `PayrollResultTools` | Payroll | Read |
| `ConsolidatedResultTools` | Payroll | Read |
| `CaseValueTimeTools` | Payroll | Read |

Future write tool classes:

| Tool class (planned) | Role | Min. permission |
|:---------------------|:-----|:----------------|
| `CaseChangeTools` | HR | Full |
| `EmployeeMutationTools` | HR | Full |
| `PayrunSubmitTools` | Payroll | Full |
| `RegulationMutationTools` | Regulation | Full |

---

## Persona Configuration Examples

### HR Manager
Access to employee lifecycle and case values. No payroll, no regulation, no admin.

```json
"Permissions": { "HR": "Full", "Payroll": "None", "Regulation": "None", "System": "None" }
```

### Payroll Specialist
Reads employee master data (needed as context), full payroll access.

```json
"Permissions": { "HR": "Read", "Payroll": "Full", "Regulation": "None", "System": "None" }
```

### HR Business Partner
Employee data plus payrun history and budget planning (temporal case value queries).

```json
"Permissions": { "HR": "Full", "Payroll": "Read", "Regulation": "None", "System": "None" }
```

### Controller / Analyst
Read-only across HR and Payroll for reporting. No regulation internals, no admin.

```json
"Permissions": { "HR": "Read", "Payroll": "Read", "Regulation": "None", "System": "None" }
```

### Regulation Developer
Needs employee and payroll context to verify regulation logic. Full regulation access.

```json
"Permissions": { "HR": "Read", "Payroll": "Read", "Regulation": "Full", "System": "None" }
```

### System Administrator
No operational data. Only tenant setup and user management.

```json
"Permissions": { "HR": "None", "Payroll": "None", "Regulation": "None", "System": "Full" }
```

### Developer / All Access
Full access across all domains. Default when `Permissions` is absent.

```json
"Permissions": { "HR": "Full", "Payroll": "Full", "Regulation": "Full", "System": "Full" }
```

---

## Persona Matrix

| Persona | HR | Payroll | Regulation | System |
|:--------|:--:|:-------:|:----------:|:------:|
| HR Manager | Full | None | None | None |
| Payroll Specialist | Read | Full | None | None |
| HR Business Partner | Full | Read | None | None |
| Controller / Analyst | Read | Read | None | None |
| Regulation Developer | Read | Read | Full | None |
| Payroll Developer | None | Full | Read | None |
| System Administrator | None | None | None | Full |
| Developer | Full | Full | Full | Full |

---

## Implementation

### Configuration model

```csharp
/// <summary>Permissions per role for this MCP Server deployment.</summary>
public sealed class McpPermissions
{
    public McpPermission HR         { get; init; } = McpPermission.Full;
    public McpPermission Payroll    { get; init; } = McpPermission.Full;
    public McpPermission Regulation { get; init; } = McpPermission.Full;
    public McpPermission System     { get; init; } = McpPermission.Full;

    public McpPermission GetPermission(McpRole role) => role switch
    {
        McpRole.HR         => HR,
        McpRole.Payroll    => Payroll,
        McpRole.Regulation => Regulation,
        McpRole.System     => System,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
}
```

Integrated into `IsolationContext`:

```csharp
public sealed class IsolationContext
{
    public IsolationLevel  Level             { get; init; } = IsolationLevel.MultiTenant;
    public string          TenantIdentifier  { get; init; }
    public McpPermissions  Permissions       { get; init; } = new();  // default: all Full
}
```

### Tool attribute

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class ToolRoleAttribute(McpRole role, McpPermission required = McpPermission.Read)
    : Attribute
{
    public McpRole       Role     { get; } = role;
    public McpPermission Required { get; } = required;
}
```

Usage on tool classes:

```csharp
[McpServerToolType]
[ToolRole(McpRole.HR)]                          // Read is the default minimum
public sealed class EmployeeQueryTools(...) { }

[McpServerToolType]
[ToolRole(McpRole.System, McpPermission.Full)]  // write tool — requires Full
public sealed class StartTenantTool(...) { }
```

### Startup registration filter

```csharp
var permissions = isolationContext.Permissions;

foreach (var toolType in GetAllToolTypes())
{
    var attr = toolType.GetCustomAttribute<ToolRoleAttribute>();
    if (attr == null)
    {
        // Untagged tool class — always register (future-proof extension point)
        services.AddSingleton(toolType);
        continue;
    }

    var granted = permissions.GetPermission(attr.Role);
    if (granted >= attr.Required)
        services.AddSingleton(toolType);
}
```

### Startup validation

```csharp
public void Validate()
{
    // IsolationLevel checks (existing) ...

    if (IsolationLevel == IsolationLevel.Employee &&
        Permissions.System >= McpPermission.Read)
        throw new InvalidOperationException(
            "System role is not permitted with IsolationLevel=Employee.");

    if (IsolationLevel == IsolationLevel.Employee &&
        Permissions.Payroll >= McpPermission.Read)
        throw new InvalidOperationException(
            "Payroll role is not permitted with IsolationLevel=Employee.");
}
```

---

## Interaction with IsolationLevel

The two axes are independent — any combination is valid except the two cases above.

| IsolationLevel | Typical HR | Typical Payroll | Typical Regulation | Typical System |
|:---------------|:----------:|:---------------:|:------------------:|:--------------:|
| MultiTenant | Read/Full | Read/Full | Full | Full |
| Tenant | Read/Full | Read/Full | Read/Full | Read |
| Division | Read/Full | Read | None | None |
| Employee *(vX.x)* | Read | None | None | None |

---

## Comparison: BusinessScope Flags vs. Role Permissions

| Aspect | Flags (`HR \| Payroll`) | Role permissions |
|:-------|:------------------------|:-----------------|
| **Expressiveness** | Binary per domain | Permission level per domain |
| **Mixed read/write** | Not expressible | `HR: Read, Payroll: Full` |
| **Future write tools** | Requires new config concept | Already modelled (`Full`) |
| **Org mapping** | Needs explanation | Maps to known job function separation |
| **Config complexity** | One value | One value per role (4 keys) |
| **Registration logic** | `HasFlag` | `>=` comparison |

---

## Implementation Roadmap

| Release | Deliverable |
|:--------|:------------|
| **v1.0** | `McpRole`, `McpPermission`, `McpPermissions`. `[ToolRole]` attribute on all tool classes (all with `Read`, `StartTenantTool` with `Full`). Registration filter. Validation. Default `Full` for all roles when `Permissions` absent. |
| **v1.x** | Write tools: `CaseChangeTools` (HR Full), `PayrunSubmitTools` (Payroll Full). `Permissions` docs and examples updated. |
| **vX.x** | `IsolationLevel=Employee` with HR:Read. SelfService persona documented. Division-scoped validation. |
