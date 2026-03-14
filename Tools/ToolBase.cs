using System;
using System.Threading.Tasks;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service;
using PayrollEngine.Client.Service.Api;

namespace PayrollEngine.McpServer.Tools;

/// <summary>Base class for all MCP tool classes.
/// Provides typed service factories and identifier-to-id resolvers for all PE objects.
/// AI agents work exclusively with external string keys (Identifier/Name);
/// internal integer IDs are resolved here before calling id-based service methods.</summary>
public abstract class ToolBase(PayrollHttpClient httpClient)
{
    /// <summary>The Payroll HTTP client used for backend communication</summary>
    protected PayrollHttpClient HttpClient { get; } = httpClient;

    #region Service factories

    /// <summary>Creates a new tenant service</summary>
    protected TenantService TenantService() => new(HttpClient);

    /// <summary>Creates a new user service</summary>
    protected UserService UserService() => new(HttpClient);

    /// <summary>Creates a new division service</summary>
    protected DivisionService DivisionService() => new(HttpClient);

    /// <summary>Creates a new employee service</summary>
    protected EmployeeService EmployeeService() => new(HttpClient);

    /// <summary>Creates a new regulation service</summary>
    protected RegulationService RegulationService() => new(HttpClient);

    /// <summary>Creates a new wage type service</summary>
    protected WageTypeService WageTypeService() => new(HttpClient);

    /// <summary>Creates a new lookup service</summary>
    protected LookupService LookupService() => new(HttpClient);

    /// <summary>Creates a new payroll service</summary>
    protected PayrollService PayrollService() => new(HttpClient);

    /// <summary>Creates a new payrun service</summary>
    protected PayrunService PayrunService() => new(HttpClient);

    /// <summary>Creates a new payrun job service</summary>
    protected PayrunJobService PayrunJobService() => new(HttpClient);

    /// <summary>Creates a new payroll result service</summary>
    protected PayrollResultService PayrollResultService() => new(HttpClient);

    #endregion

    #region Resolvers — Identifier/Name → object with Id

    private static readonly RootServiceContext RootContext = new();

    /// <summary>Resolves a tenant by its identifier.
    /// Unlocks all id-based tenant service methods (attributes, shared regulations).</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <returns>The resolved tenant including its internal Id</returns>
    /// <exception cref="InvalidOperationException">Thrown when the tenant is not found</exception>
    protected async Task<Tenant> ResolveTenantAsync(string tenantIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantIdentifier);
        var tenant = await TenantService().GetAsync<Tenant>(RootContext, tenantIdentifier);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant '{tenantIdentifier}' not found.");
        }
        return tenant;
    }

    /// <summary>Resolves a tenant context by identifier.
    /// Use when only the TenantServiceContext is needed for subsequent child-object calls.</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <returns>A TenantServiceContext containing the resolved tenant Id</returns>
    protected async Task<TenantServiceContext> ResolveTenantContextAsync(string tenantIdentifier)
    {
        var tenant = await ResolveTenantAsync(tenantIdentifier);
        return new TenantServiceContext(tenant.Id);
    }

    /// <summary>Resolves a user by tenant identifier and user identifier.
    /// Unlocks all id-based user service methods (attributes, password).</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <param name="userIdentifier">The external user identifier</param>
    /// <returns>The tenant context and the resolved user including its internal Id</returns>
    protected async Task<(TenantServiceContext Context, User User)> ResolveUserAsync(
        string tenantIdentifier, string userIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userIdentifier);
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var user = await UserService().GetAsync<User>(context, userIdentifier);
        if (user == null)
        {
            throw new InvalidOperationException(
                $"User '{userIdentifier}' not found in tenant '{tenantIdentifier}'.");
        }
        return (context, user);
    }

    /// <summary>Resolves a division by tenant identifier and division name.
    /// Unlocks all id-based division service methods (attributes).
    /// Note: Division uses Name as external key, not Identifier.</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <param name="divisionName">The division name</param>
    /// <returns>The tenant context and the resolved division including its internal Id</returns>
    protected async Task<(TenantServiceContext Context, Division Division)> ResolveDivisionAsync(
        string tenantIdentifier, string divisionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(divisionName);
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var division = await DivisionService().GetAsync<Division>(context, divisionName);
        if (division == null)
        {
            throw new InvalidOperationException(
                $"Division '{divisionName}' not found in tenant '{tenantIdentifier}'.");
        }
        return (context, division);
    }

    /// <summary>Resolves an employee by tenant identifier and employee identifier.
    /// Unlocks all id-based employee service methods (attributes).</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <param name="employeeIdentifier">The external employee identifier</param>
    /// <returns>The tenant context and the resolved employee including its internal Id</returns>
    protected async Task<(TenantServiceContext Context, Employee Employee)> ResolveEmployeeAsync(
        string tenantIdentifier, string employeeIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeIdentifier);
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var employee = await EmployeeService().GetAsync<Employee>(context, employeeIdentifier);
        if (employee == null)
        {
            throw new InvalidOperationException(
                $"Employee '{employeeIdentifier}' not found in tenant '{tenantIdentifier}'.");
        }
        return (context, employee);
    }

    /// <summary>Resolves a regulation context by tenant identifier and regulation name.
    /// Required for WageTypeService and LookupService which need RegulationServiceContext.</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <param name="regulationName">The regulation name</param>
    /// <returns>A RegulationServiceContext containing tenant Id and regulation Id</returns>
    protected async Task<RegulationServiceContext> ResolveRegulationContextAsync(
        string tenantIdentifier, string regulationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regulationName);
        var tenantContext = await ResolveTenantContextAsync(tenantIdentifier);
        var regulation = await RegulationService().GetAsync<Regulation>(tenantContext, regulationName);
        if (regulation == null)
        {
            throw new InvalidOperationException(
                $"Regulation '{regulationName}' not found in tenant '{tenantIdentifier}'.");
        }
        return new RegulationServiceContext(tenantContext.TenantId, regulation.Id);
    }

    /// <summary>Resolves a payroll context by tenant identifier and payroll name.
    /// Required for PayrollService methods that query regulation items (WageTypes, Lookups, etc.).</summary>
    /// <param name="tenantIdentifier">The external tenant identifier</param>
    /// <param name="payrollName">The payroll name</param>
    /// <returns>A PayrollServiceContext containing tenant Id and payroll Id</returns>
    protected async Task<PayrollServiceContext> ResolvePayrollContextAsync(
        string tenantIdentifier, string payrollName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payrollName);
        var tenantContext = await ResolveTenantContextAsync(tenantIdentifier);
        var payroll = await PayrollService().GetAsync<Payroll>(tenantContext, payrollName);
        if (payroll == null)
        {
            throw new InvalidOperationException(
                $"Payroll '{payrollName}' not found in tenant '{tenantIdentifier}'.");
        }
        return new PayrollServiceContext(tenantContext.TenantId, payroll.Id);
    }

    #endregion
}
