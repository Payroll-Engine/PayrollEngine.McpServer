using System;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.QueryExpression;
using PayrollEngine.Client.Service;
using PayrollEngine.Client.Service.Api;
using PayrollEngine.McpServer.Tools.Isolation;

namespace PayrollEngine.McpServer.Tools;

/// <summary>Base class for all MCP tool classes.
/// Provides typed service factories, identifier-to-id resolvers, and uniform error handling.
/// AI agents work exclusively with external string keys (Identifier/Name);
/// internal integer IDs are resolved here before calling id-based service methods.</summary>
public abstract class ToolBase(PayrollHttpClient httpClient, IsolationContext isolation)
{
    /// <summary>The Payroll HTTP client used for backend communication</summary>
    private PayrollHttpClient HttpClient { get; } = httpClient;

    /// <summary>The active data isolation context. Every tool call is filtered through this context.</summary>
    private IsolationContext Isolation { get; } = isolation;

    #region Query helpers

    /// <summary>Builds a Query with status=Active filter, optionally combined with an additional OData filter.
    /// All list operations use this to exclude inactive objects by default.</summary>
    /// <param name="userFilter">Optional additional OData filter from the caller</param>
    /// <param name="top">Optional result limit</param>
    /// <param name="orderBy">Optional order-by expression</param>
    /// <returns>Query with active-only filter</returns>
    protected static Query ActiveQuery(string userFilter = null, int? top = null, string orderBy = null)
    {
        var activeFilter = new ActiveStatus();
        var combinedFilter = string.IsNullOrWhiteSpace(userFilter)
            ? activeFilter.Expression
            : new Filter(activeFilter.Expression).And(new Filter(userFilter)).Expression;
        return new Query
        {
            Filter = combinedFilter,
            Top = top,
            OrderBy = orderBy
        };
    }

    /// <summary>Builds a DivisionQuery with status=Active filter.
    /// Required for EmployeeService.QueryAsync which expects DivisionQuery.</summary>
    /// <param name="userFilter">Optional additional OData filter from the caller</param>
    /// <param name="top">Optional result limit</param>
    /// <returns>DivisionQuery with active-only filter</returns>
    protected static DivisionQuery ActiveDivisionQuery(string userFilter = null, int? top = null)
    {
        var activeFilter = new ActiveStatus();
        var combinedFilter = string.IsNullOrWhiteSpace(userFilter)
            ? activeFilter.Expression
            : new Filter(activeFilter.Expression).And(new Filter(userFilter)).Expression;
        return new DivisionQuery
        {
            Filter = combinedFilter,
            Top = top
        };
    }

    /// <summary>Builds a tenant-scoped active query.
    /// In Tenant isolation mode, adds an identifier filter so list_tenants only returns the configured tenant.</summary>
    protected Query IsolatedTenantQuery()
    {
        if (Isolation.Level == IsolationLevel.Tenant &&
            !string.IsNullOrWhiteSpace(Isolation.TenantIdentifier))
        {
            return ActiveQuery($"identifier eq '{Isolation.TenantIdentifier}'");
        }
        return ActiveQuery();
    }

    #endregion

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

    /// <summary>Creates a new employee case change service</summary>
    protected EmployeeCaseChangeService EmployeeCaseChangeService() => new(HttpClient);

    /// <summary>Creates a new company case change service</summary>
    protected CompanyCaseChangeService CompanyCaseChangeService() => new(HttpClient);

    /// <summary>Creates a new payroll result value service</summary>
    protected PayrollResultValueService PayrollResultValueService() => new(HttpClient);

    /// <summary>Creates a new payroll consolidated result service</summary>
    protected PayrollConsolidatedResultService PayrollConsolidatedResultService() => new(HttpClient);

    #endregion

    #region Resolvers — Identifier/Name → object with Id

    /// <summary>Returns the effective tenant identifier, enforcing isolation.
    /// In Tenant mode the configured identifier overrides whatever the AI agent passed.
    /// Division and Employee levels are not yet supported and throw NotSupportedException.</summary>
    /// <param name="tenantIdentifier">Identifier provided by the AI agent</param>
    private string EffectiveTenant(string tenantIdentifier) => Isolation.Level switch
    {
        IsolationLevel.MultiTenant => tenantIdentifier,
        IsolationLevel.Tenant      => Isolation.TenantIdentifier,
        _                          => throw new NotSupportedException(
            $"Isolation level '{Isolation.Level}' is architecturally reserved and will be available in a future release.")
    };

    private static readonly RootServiceContext RootContext = new();

    /// <summary>Resolves a tenant by its identifier, applying isolation enforcement.
    /// In Tenant mode, the configured tenant identifier always takes precedence.</summary>
    protected async Task<Tenant> ResolveTenantAsync(string tenantIdentifier)
    {
        var effective = EffectiveTenant(tenantIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(effective);
        var tenant = await TenantService().GetAsync<Tenant>(RootContext, effective);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant '{effective}' not found.");
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
