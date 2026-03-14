using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using PayrollEngine.Client;
using PayrollEngine.Client.Model;

namespace PayrollEngine.McpServer.Tools.PeopleTools;

/// <summary>MCP tools for user queries</summary>
[McpServerToolType]
public sealed class UserQueryTools(PayrollHttpClient httpClient) : ToolBase(httpClient)
{
    /// <summary>List all users of a tenant</summary>
    [McpServerTool(Name = "list_users"), Description("List all users of a tenant")]
    public async Task<string> ListUsersAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier)
    {
        var context = await ResolveTenantContextAsync(tenantIdentifier);
        var users = await UserService().QueryAsync<User>(context);
        return JsonSerializer.Serialize(users);
    }

    /// <summary>Get a user by identifier within a tenant</summary>
    [McpServerTool(Name = "get_user"), Description("Get a user by identifier within a tenant")]
    public async Task<string> GetUserAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The user identifier (typically an email address)")] string userIdentifier)
    {
        var (_, user) = await ResolveUserAsync(tenantIdentifier, userIdentifier);
        return JsonSerializer.Serialize(user);
    }

    /// <summary>Get a single attribute value of a user</summary>
    [McpServerTool(Name = "get_user_attribute"), Description("Get a single attribute value of a user")]
    public async Task<string> GetUserAttributeAsync(
        [Description("The unique tenant identifier")] string tenantIdentifier,
        [Description("The user identifier")] string userIdentifier,
        [Description("The attribute name")] string attributeName)
    {
        var (context, user) = await ResolveUserAsync(tenantIdentifier, userIdentifier);
        return await UserService().GetAttributeAsync(context, user.Id, attributeName);
    }
}
