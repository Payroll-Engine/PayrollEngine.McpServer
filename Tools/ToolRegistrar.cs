using System;
using System.Collections.Generic;
using System.Reflection;
using ModelContextProtocol.Server;
using PayrollEngine.McpServer.Tools.Isolation;

namespace PayrollEngine.McpServer.Tools;

/// <summary>Filters and registers MCP tool classes based on the active McpPermissions.
/// Tool classes tagged with [ToolRole] are only registered when the deployment grants
/// at least the required permission for that role.
/// Tool classes without [ToolRole] are always registered.</summary>
public static class ToolRegistrar
{
    /// <summary>Returns all tool types from the given assembly that pass the permission filter.</summary>
    public static IEnumerable<Type> GetPermittedTypes(Assembly assembly, McpPermissions permissions)
    {
        foreach (var type in assembly.GetTypes())
        {
            // must be a concrete MCP tool class
            if (!type.IsClass || type.IsAbstract)
                continue;
            if (type.GetCustomAttribute<McpServerToolTypeAttribute>() == null)
                continue;

            var roleAttr = type.GetCustomAttribute<ToolRoleAttribute>();
            if (roleAttr == null)
            {
                // no role restriction — always register
                yield return type;
                continue;
            }

            if (permissions.IsGranted(roleAttr.Role, roleAttr.Required))
                yield return type;
        }
    }
}
