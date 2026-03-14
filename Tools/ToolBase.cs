using System;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service;
using PayrollEngine.Client.Service.Api;

namespace PayrollEngine.McpServer.Tools;

/// <summary>Base class for all MCP tool classes.
/// Provides typed service factories, identifier-to-id resolvers, and uniform error handling.
/// AI agents work exclusively with external string keys (Identifier/Name);
/// internal integer IDs are resolved here before calling id-based service methods.</summary>
public abstract class ToolBase(PayrollHttpClient httpClient)
{
    /// <summary>The Payroll HTTP client used for backend communication</summary>
    protected PayrollHttpClient HttpClient { get; } = httpClient;

    #region Error handling

    /// <summary>Serializes a caught exception as a structured JSON error result.
    /// All public tool methods must catch exceptions and delegate to this method
    /// so the MCP client always receives valid JSON, never a raw exception message.</summary>
    /// <param name="exception">The caught exception</param>
    /// <returns>JSON: { "error": "...", "type": "..." }</returns>
    protected static string Error(Exception exception)
    {
        var message = exception.GetBaseException().Message;
        var type = exception.GetBaseException().GetType().Name;
        return JsonSerializer.Serialize(new { error = message, type });
    }

    #endregion

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

    /// <summary>Creates a new lookup value service</summary>
    protected LookupValueService LookupValueService() => new(HttpClient);

    /// <summary>Creates a new payroll service</summary>
    protected PayrollService PayrollService() => new(HttpClient);

    /// <summary>Creates a new payrun service</summary>
    protected PayrunService PayrunService() => new(HttpClient);

    /// <summary>Creates a new payrun job service</summary>
    protected PayrunJobService PayrunJobService() => new(HttpClient);

    /// <summary>Creates a new payroll result service</summary>
    protected PayrollResultService PayrollResultService() => new(HttpClient);

    /// <summary>Creates a new employee case value service</summary>
    protected EmployeeCaseValueService EmployeeCaseValueService() => new(HttpClient);

    /// <summary>Creates a new company case value service</summary>
    protected CompanyCaseValueService CompanyCaseValueService() => new(HttpClient);

    #endregion

    #region Resolvers — Identifier/Name → object with Id

    private static readonly RootServiceContext RootContext = new();

    /// <summary>Resolves a tenant by its identifier</summary>
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

    /// <summary>Resolves a TenantServiceContext by tenant identifier</summary>
    protected async Task<TenantServiceContext> ResolveTenantContextAsync(string tenantIdentifier)
    {
        var tenant = await ResolveTenantAsync(tenantIdentifier);
        return new TenantServiceContext(tenant.Id);
    }

    /// <summary>Resolves a user by tenant identifier and user identifier</summary>
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

    /// <summary>Resolves a division by tenant identifier and division name</summary>
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

    /// <summary>Resolves an employee by tenant identifier and employee identifier</summary>
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

    /// <summary>Resolves an EmployeeServiceContext by tenant and employee identifier.
    /// Required for EmployeeCaseValueService which needs EmployeeServiceContext.</summary>
    protected async Task<EmployeeServiceContext> ResolveEmployeeContextAsync(
        string tenantIdentifier, string employeeIdentifier)
    {
        var (tenantContext, employee) = await ResolveEmployeeAsync(tenantIdentifier, employeeIdentifier);
        return new EmployeeServiceContext(tenantContext.TenantId, employee.Id);
    }

    /// <summary>Resolves a RegulationServiceContext by tenant identifier and regulation name</summary>
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

    /// <summary>Resolves a LookupServiceContext by tenant, regulation and lookup name.
    /// Required for LookupValueService which needs LookupServiceContext.</summary>
    protected async Task<LookupServiceContext> ResolveLookupContextAsync(
        string tenantIdentifier, string regulationName, string lookupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupName);
        var regulationContext = await ResolveRegulationContextAsync(tenantIdentifier, regulationName);
        var lookup = await LookupService().GetAsync<Lookup>(regulationContext, lookupName);
        if (lookup == null)
        {
            throw new InvalidOperationException(
                $"Lookup '{lookupName}' not found in regulation '{regulationName}' of tenant '{tenantIdentifier}'.");
        }
        return new LookupServiceContext(regulationContext.TenantId, regulationContext.RegulationId, lookup.Id);
    }

    /// <summary>Resolves a PayrollServiceContext by tenant identifier and payroll name</summary>
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
