using System.Linq;
using System.Reflection;
using PayrollEngine.McpServer.Tools;
using PayrollEngine.McpServer.Tools.Isolation;
using PayrollEngine.McpServer.Tools.PayrollTools;
using PayrollEngine.McpServer.Tools.PeopleTools;
using PayrollEngine.McpServer.Tools.ReportTools;
using PayrollEngine.McpServer.Tools.TenantTools;
using Xunit;

namespace PayrollEngine.McpServer.Tests;

/// <summary>Unit tests for ToolRegistrar permission filtering.
/// Verifies that tool classes are registered or excluded based on McpPermissions —
/// without requiring a running backend.</summary>
public class ToolRegistrarTest
{
    private static readonly Assembly ToolsAssembly = typeof(ToolsMarker).Assembly;

    // HR tool classes
    private static readonly System.Type[] HrTools =
    [
        typeof(DivisionQueryTools),
        typeof(EmployeeQueryTools),
        typeof(CaseValueQueryTools),
        typeof(CaseChangeQueryTools)
    ];

    // Payroll tool classes
    private static readonly System.Type[] PayrollTools =
    [
        typeof(PayrollQueryTools),
        typeof(PayrollResultTools),
        typeof(ConsolidatedResultTools),
        typeof(PayrollPreviewTools)
    ];

    // Report tool classes
    private static readonly System.Type[] ReportTools =
    [
        typeof(ReportQueryTools)
    ];

    // System tool classes
    private static readonly System.Type[] SystemTools =
    [
        typeof(TenantQueryTools),
        typeof(UserQueryTools)
    ];

    #region All roles Read @ MultiTenant — full registration

    [Fact]
    public void AllRead_RegistersAllHrTools()
    {
        var permitted = GetPermitted(AllRead()).ToList();
        Assert.All(HrTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void AllRead_RegistersAllPayrollTools()
    {
        var permitted = GetPermitted(AllRead()).ToList();
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void AllRead_RegistersAllReportTools()
    {
        var permitted = GetPermitted(AllRead()).ToList();
        Assert.All(ReportTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void AllRead_RegistersAllSystemTools()
    {
        var permitted = GetPermitted(AllRead()).ToList();
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    #endregion

    #region All roles None — only ServerInfo registered

    [Fact]
    public void AllNone_ExcludesAllHrTools()
    {
        var permitted = GetPermitted(AllNone()).ToList();
        Assert.All(HrTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void AllNone_ExcludesAllPayrollTools()
    {
        var permitted = GetPermitted(AllNone()).ToList();
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void AllNone_ExcludesAllReportTools()
    {
        var permitted = GetPermitted(AllNone()).ToList();
        Assert.All(ReportTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void AllNone_ExcludesAllSystemTools()
    {
        var permitted = GetPermitted(AllNone()).ToList();
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    #endregion

    #region Single role None — only that role excluded

    [Fact]
    public void HrNone_ExcludesHrTools_KeepsOthers()
    {
        var permissions = new McpPermissions { HR = McpPermission.None };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void PayrollNone_ExcludesPayrollTools_KeepsOthers()
    {
        var permissions = new McpPermissions { Payroll = McpPermission.None };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void ReportNone_ExcludesReportTools_KeepsOthers()
    {
        var permissions = new McpPermissions { Report = McpPermission.None };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(ReportTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void SystemNone_ExcludesSystemTools_KeepsOthers()
    {
        var permissions = new McpPermissions { System = McpPermission.None };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(ReportTools, t => Assert.Contains(t, permitted));
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    #endregion

    #region Persona configurations

    [Fact]
    public void HrManagerPersona_OnlyHrToolsRegistered()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.Read,
            Payroll = McpPermission.None,
            System = McpPermission.None
        };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void PayrollSpecialistPersona_HrAndPayrollRegistered()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.Read,
            Payroll = McpPermission.Read,
            System = McpPermission.None
        };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void SystemAdminPersona_OnlySystemToolsRegistered()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.None,
            Payroll = McpPermission.None,
            System = McpPermission.Read
        };
        var permitted = GetPermitted(permissions).ToList();

        Assert.All(HrTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    #endregion

    #region IsolationLevel filtering

    [Fact]
    public void Division_ExcludesSystemAndReportTools()
    {
        var permitted = GetPermitted(AllRead(), IsolationLevel.Division).ToList();
        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(ReportTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void Employee_ExcludesPayrollSystemAndReportTools()
    {
        var permitted = GetPermitted(AllRead(), IsolationLevel.Employee).ToList();
        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(ReportTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void Tenant_AllRolesApplicable()
    {
        var permitted = GetPermitted(AllRead(), IsolationLevel.Tenant).ToList();
        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(ReportTools, t => Assert.Contains(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    #endregion

    #region Tool count — detects unregistered new tool classes

    [Fact]
    public void AllRead_TotalToolCount_Is12()
    {
        // ServerInfo(1, no role) + HR(4) + Payroll(4) + Report(1) + System(2) = 12
        // Update this count whenever a new tool class is added.
        var permitted = GetPermitted(AllRead()).ToList();
        Assert.Equal(12, permitted.Count);
    }

    [Fact]
    public void AllNone_ServerInfoStillRegistered()
    {
        // ServerInfoTools has no [ToolRole] — always registered regardless of permissions
        var permitted = GetPermitted(AllNone()).ToList();
        Assert.Contains(typeof(ServerInfoTools), permitted);
    }

    [Fact]
    public void AllNone_OnlyServerInfoRegistered()
    {
        // With all roles None, only the role-less ServerInfoTools should be registered
        var permitted = GetPermitted(AllNone()).ToList();
        Assert.Single(permitted);
    }

    [Fact]
    public void HrToolCount_Is4()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.Read,
            Payroll = McpPermission.None,
            Report = McpPermission.None,
            System = McpPermission.None
        };
        var permitted = GetPermitted(permissions).ToList();
        Assert.Equal(5, permitted.Count); // HR(4) + ServerInfo(1)
    }

    [Fact]
    public void PayrollToolCount_Is4()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.None,
            Payroll = McpPermission.Read,
            Report = McpPermission.None,
            System = McpPermission.None
        };
        var permitted = GetPermitted(permissions).ToList();
        Assert.Equal(5, permitted.Count); // Payroll(4) + ServerInfo(1)
    }

    [Fact]
    public void ReportToolCount_Is1()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.None,
            Payroll = McpPermission.None,
            Report = McpPermission.Read,
            System = McpPermission.None
        };
        var permitted = GetPermitted(permissions).ToList();
        Assert.Equal(2, permitted.Count); // Report(1) + ServerInfo(1)
    }

    [Fact]
    public void SystemToolCount_Is2()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.None,
            Payroll = McpPermission.None,
            Report = McpPermission.None,
            System = McpPermission.Read
        };
        var permitted = GetPermitted(permissions).ToList();
        Assert.Equal(3, permitted.Count); // System(2) + ServerInfo(1)
    }

    #endregion

    #region Helpers

    // Default isolation level for permission-only tests: MultiTenant (all roles applicable)
    private static System.Collections.Generic.IEnumerable<System.Type> GetPermitted(
        McpPermissions permissions,
        IsolationLevel level = IsolationLevel.MultiTenant) =>
        ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions, level);

    private static McpPermissions AllRead() => new()
    {
        HR = McpPermission.Read,
        Payroll = McpPermission.Read,
        Report = McpPermission.Read,
        System = McpPermission.Read
    };

    private static McpPermissions AllNone() => new()
    {
        HR = McpPermission.None,
        Payroll = McpPermission.None,
        Report = McpPermission.None,
        System = McpPermission.None
    };

    #endregion
}
