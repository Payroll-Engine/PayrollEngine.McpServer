using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;
using PayrollEngine.Client.Service;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.McpServer.Tools.TenantTools;

/// <summary>Result of a StartTenant operation</summary>
public sealed class StartTenantResult
{
    /// <summary>The tenant identifier</summary>
    public string TenantIdentifier { get; init; }

    /// <summary>The internal tenant id assigned by the backend</summary>
    public int TenantId { get; init; }

    /// <summary>Identifiers of users created during this operation</summary>
    public List<string> CreatedUsers { get; init; } = [];

    /// <summary>Names of divisions created during this operation</summary>
    public List<string> CreatedDivisions { get; init; } = [];

    /// <summary>Error messages collected during this operation</summary>
    public List<string> Errors { get; init; } = [];

    /// <summary>True when no errors occurred</summary>
    public bool Success => Errors.Count == 0;
}

/// <summary>MCP tools for tenant setup</summary>
[McpServerToolType]
public sealed class StartTenantTool(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    private static readonly RootServiceContext RootContext = new();

    /// <summary>
    /// Creates a new tenant with an initial admin user and one or more divisions.
    /// Equivalent to the Console StartTenant command, but callable by AI agents.
    /// Idempotent: skips creation of objects that already exist.
    /// </summary>
    [McpServerTool(Name = "start_tenant"), Description(
        "Set up a new tenant with an admin user and divisions. " +
        "Skips objects that already exist (idempotent). " +
        "Returns a summary with created objects and any errors.")]
    public async Task<string> StartTenantAsync(
        [Description("Unique tenant identifier, e.g. 'AcmeCorp'")] string tenantIdentifier,
        [Description("Tenant culture (RFC 4646), e.g. 'en-US' or 'de-CH'")] string culture,
        [Description("Admin user identifier (typically an email address)")] string adminUserIdentifier,
        [Description("Admin user first name")] string adminFirstName,
        [Description("Admin user last name")] string adminLastName,
        [Description("Comma-separated list of division names to create, e.g. 'HQ,Sales,IT'")] string divisionNames)
    {
        var result = new StartTenantResult { TenantIdentifier = tenantIdentifier };
        try
        {
            var tenant = await EnsureTenantAsync(tenantIdentifier, culture);
            var tenantContext = new TenantServiceContext(tenant.Id);
            await EnsureUserAsync(tenantContext, adminUserIdentifier, adminFirstName, adminLastName, culture, result);
            foreach (var rawName in divisionNames.Split(',',
                System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
            {
                await EnsureDivisionAsync(tenantContext, rawName, culture, result);
            }
            return JsonSerializer.Serialize(new StartTenantResult
            {
                TenantIdentifier = tenantIdentifier,
                TenantId = tenant.Id,
                CreatedUsers = result.CreatedUsers,
                CreatedDivisions = result.CreatedDivisions,
                Errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.GetBaseException().Message);
            return JsonSerializer.Serialize(result);
        }
    }

    private async Task<Tenant> EnsureTenantAsync(string identifier, string culture)
    {
        var existing = await TenantService().GetAsync<Tenant>(RootContext, identifier);
        if (existing != null)
        {
            return existing;
        }
        return await TenantService().CreateAsync(RootContext, new Tenant
        {
            Identifier = identifier,
            Culture = culture
        });
    }

    private async Task EnsureUserAsync(TenantServiceContext context, string identifier,
        string firstName, string lastName, string culture, StartTenantResult result)
    {
        var existing = await UserService().GetAsync<User>(context, identifier);
        if (existing != null)
        {
            return;
        }
        await UserService().CreateAsync(context, new User
        {
            Identifier = identifier,
            FirstName = firstName,
            LastName = lastName,
            Culture = culture,
            UserType = UserType.TenantAdministrator
        });
        result.CreatedUsers.Add(identifier);
    }

    private async Task EnsureDivisionAsync(TenantServiceContext context, string name,
        string culture, StartTenantResult result)
    {
        var existing = await DivisionService().GetAsync<Division>(context, name);
        if (existing != null)
        {
            return;
        }
        await DivisionService().CreateAsync(context, new Division
        {
            Name = name,
            Culture = culture
        });
        result.CreatedDivisions.Add(name);
    }
}
