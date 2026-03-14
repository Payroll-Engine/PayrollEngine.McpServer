namespace PayrollEngine.McpServer.Tools.Isolation;

/// <summary>Functional domain to which a tool class belongs.
/// Each tool class is assigned to exactly one role.</summary>
public enum McpRole
{
    /// <summary>Employee master data and organisational structure:
    /// divisions, employees, case values.</summary>
    HR,

    /// <summary>Payroll execution and result verification:
    /// payrolls, payruns, payrun jobs, temporal case value queries.</summary>
    Payroll,

    /// <summary>Regulation definitions: regulations, wage types, lookups.</summary>
    Regulation,

    /// <summary>Tenant and user management.</summary>
    System
}
