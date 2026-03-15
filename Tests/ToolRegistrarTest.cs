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

    #region All roles Read — full registration

    [Fact]
    public void AllRead_RegistersAllHrTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllRead()).ToList();
        Assert.All(HrTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void AllRead_RegistersAllPayrollTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllRead()).ToList();
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void AllRead_RegistersAllReportTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllRead()).ToList();
        Assert.All(ReportTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void AllRead_RegistersAllSystemTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllRead()).ToList();
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    #endregion

    #region All roles None — nothing registered

    [Fact]
    public void AllNone_ExcludesAllHrTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllNone()).ToList();
        Assert.All(HrTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void AllNone_ExcludesAllPayrollTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllNone()).ToList();
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void AllNone_ExcludesAllReportTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllNone()).ToList();
        Assert.All(ReportTools, t => Assert.DoesNotContain(t, permitted));
    }

    [Fact]
    public void AllNone_ExcludesAllSystemTools()
    {
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllNone()).ToList();
        Assert.All(SystemTools, t => Assert.DoesNotContain(t, permitted));
    }

    #endregion

    #region Single role None — only that role excluded

    [Fact]
    public void HrNone_ExcludesHrTools_KeepsOthers()
    {
        var permissions = new McpPermissions { HR = McpPermission.None };
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

        Assert.All(HrTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void PayrollNone_ExcludesPayrollTools_KeepsOthers()
    {
        var permissions = new McpPermissions { Payroll = McpPermission.None };
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void ReportNone_ExcludesReportTools_KeepsOthers()
    {
        var permissions = new McpPermissions { Report = McpPermission.None };
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

        Assert.All(HrTools, t => Assert.Contains(t, permitted));
        Assert.All(PayrollTools, t => Assert.Contains(t, permitted));
        Assert.All(ReportTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    [Fact]
    public void SystemNone_ExcludesSystemTools_KeepsOthers()
    {
        var permissions = new McpPermissions { System = McpPermission.None };
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

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
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

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
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

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
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();

        Assert.All(HrTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(PayrollTools, t => Assert.DoesNotContain(t, permitted));
        Assert.All(SystemTools, t => Assert.Contains(t, permitted));
    }

    #endregion

    #region Tool count — detects unregistered new tool classes

    [Fact]
    public void AllRead_TotalToolCount_Is11()
    {
        // HR(4) + Payroll(4) + Report(1) + System(2) = 11
        // Update this count whenever a new tool class is added.
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, AllRead()).ToList();
        Assert.Equal(11, permitted.Count);
    }

    [Fact]
    public void HrToolCount_Is4()
    {
        var permissions = new McpPermissions
        {
            HR = McpPermission.Read,
            Payroll = McpPermission.None,
            System = McpPermission.None
        };
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();
        Assert.Equal(4, permitted.Count);
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
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();
        Assert.Equal(4, permitted.Count);
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
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();
        Assert.Single(permitted);
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
        var permitted = ToolRegistrar.GetPermittedTypes(ToolsAssembly, permissions).ToList();
        Assert.Equal(2, permitted.Count);
    }

    #endregion

    #region Helpers

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
