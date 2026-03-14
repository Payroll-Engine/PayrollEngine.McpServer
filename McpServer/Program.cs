using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayrollEngine.Client;
using PayrollEngine.McpServer.Tools;
using PayrollEngine.McpServer.Tools.Isolation;
using PayrollEngine.Serilog;

namespace PayrollEngine.McpServer;

sealed class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Remove ALL default logging providers — stdout is reserved for the MCP protocol.
            // Any output to stdout (including .NET host startup messages) corrupts the JSON stream.
            // Serilog (file-only) is configured separately via SetupSerilog().
            builder.Logging.ClearProviders();

            // logger setup — file only, no console (stdio is reserved for MCP protocol)
            builder.Configuration.SetupSerilog();

            // payroll http client for backend communication
            var payrollHttpClient = await BuildPayrollHttpClientAsync(builder);

            // isolation context — reads McpServer:IsolationLevel, TenantIdentifier, and Permissions
            var isolationContext = BuildIsolationContext(builder);
            isolationContext.Validate();

            // MCP server with stdio transport and permission-filtered tool registration
            var mcpBuilder = builder.Services
                .AddSingleton(payrollHttpClient)
                .AddSingleton(isolationContext)
                .AddMcpServer()
                .WithStdioServerTransport();

            RegisterPermittedTools(mcpBuilder, isolationContext.Permissions);

            await builder.Build().RunAsync();
        }
        catch (Exception exception)
        {
            Log.Critical(exception, exception.GetBaseException().Message);
        }
    }

    /// <summary>Registers only the tool classes permitted by the active McpPermissions.
    /// Uses the WithTools&lt;T&gt;() extension method from the MCP SDK via reflection
    /// to support dynamic type-based registration.</summary>
    private static void RegisterPermittedTools(IMcpServerBuilder mcpBuilder, McpPermissions permissions)
    {
        var toolsAssembly = typeof(ToolsMarker).Assembly;
        var permittedTypes = ToolRegistrar.GetPermittedTypes(toolsAssembly, permissions).ToList();

        // Locate WithTools<T>(IMcpServerBuilder) in the MCP SDK via reflection.
        // This is robust across SDK refactors — finds the method by signature regardless of class.
        var withToolsMethod = typeof(IMcpServerBuilder).Assembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .FirstOrDefault(m =>
                m.Name == "WithTools" &&
                m.IsGenericMethodDefinition &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(IMcpServerBuilder));

        if (withToolsMethod == null)
            throw new InvalidOperationException(
                "WithTools<T>(IMcpServerBuilder) not found in the MCP SDK. " +
                "Ensure ModelContextProtocol package is correctly referenced.");

        foreach (var type in permittedTypes)
            withToolsMethod.MakeGenericMethod(type).Invoke(null, [mcpBuilder]);

        Log.Information($"Registered {permittedTypes.Count} tool class(es) based on active permissions.");
    }

    private static IsolationContext BuildIsolationContext(HostApplicationBuilder builder)
    {
        var levelValue = builder.Configuration["McpServer:IsolationLevel"];
        var level = Enum.TryParse<IsolationLevel>(levelValue, ignoreCase: true, out var parsed)
            ? parsed
            : IsolationLevel.MultiTenant;

        return new IsolationContext
        {
            Level = level,
            TenantIdentifier = builder.Configuration["McpServer:TenantIdentifier"],
            Permissions = McpPermissions.FromConfiguration(builder.Configuration)
        };
    }

    private static async Task<PayrollHttpClient> BuildPayrollHttpClientAsync(
        HostApplicationBuilder builder)
    {
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        };

        // insecure ssl: skip certificate validation (dev only)
        var allowInsecureSsl = builder.Configuration["AllowInsecureSsl"];
        if (bool.TryParse(allowInsecureSsl, out var skipSsl) && skipSsl)
        {
            Log.Warning("SSL certificate validation is disabled (AllowInsecureSsl=true)");
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        // http configuration from appsettings.json / apisettings.json / environment variables
        var config = await builder.Configuration.GetHttpConfigurationAsync();
        if (config == null || !config.Valid())
        {
            throw new InvalidOperationException(
                "Missing or invalid Payroll HTTP client configuration. " +
                "Set ApiSettings in appsettings.json or the environment variable " +
                $"'{SystemSpecification.PayrollApiConnection}'.");
        }

        var client = new PayrollHttpClient(handler, config);

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            client.SetApiKey(config.ApiKey);
        }

        return client;
    }
}
