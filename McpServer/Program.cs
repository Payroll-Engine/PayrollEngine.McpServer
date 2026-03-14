using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayrollEngine.Client;
using PayrollEngine.McpServer.Tools;
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

            // MCP server with stdio transport
            builder.Services
                .AddSingleton(payrollHttpClient)
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly(typeof(ToolsMarker).Assembly);

            await builder.Build().RunAsync();
        }
        catch (Exception exception)
        {
            Log.Critical(exception, exception.GetBaseException().Message);
        }
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
