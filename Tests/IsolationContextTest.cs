using System;
using PayrollEngine.McpServer.Tools.Isolation;
using Xunit;

namespace PayrollEngine.McpServer.Tests;

/// <summary>Unit tests for IsolationContext.Validate().
/// Verifies that required configuration keys are enforced per isolation level.</summary>
public class IsolationContextTest
{
    #region MultiTenant — always valid

    [Fact]
    public void MultiTenant_NoIdentifiers_IsValid()
    {
        var ctx = new IsolationContext { Level = IsolationLevel.MultiTenant };
        ctx.Validate(); // must not throw
    }

    #endregion

    #region Tenant

    [Fact]
    public void Tenant_WithIdentifier_IsValid()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Tenant,
            TenantIdentifier = "acme-corp"
        };
        ctx.Validate(); // must not throw
    }

    [Fact]
    public void Tenant_MissingIdentifier_Throws()
    {
        var ctx = new IsolationContext { Level = IsolationLevel.Tenant };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    [Fact]
    public void Tenant_WhitespaceIdentifier_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Tenant,
            TenantIdentifier = "   "
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    #endregion

    #region Division

    [Fact]
    public void Division_WithAllRequired_IsValid()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Division,
            TenantIdentifier = "acme-corp",
            DivisionName = "sales"
        };
        ctx.Validate(); // must not throw
    }

    [Fact]
    public void Division_MissingTenantIdentifier_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Division,
            DivisionName = "sales"
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    [Fact]
    public void Division_MissingDivisionName_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Division,
            TenantIdentifier = "acme-corp"
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    [Fact]
    public void Division_WhitespaceDivisionName_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Division,
            TenantIdentifier = "acme-corp",
            DivisionName = "   "
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    #endregion

    #region Employee

    [Fact]
    public void Employee_WithAllRequired_IsValid()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Employee,
            TenantIdentifier = "acme-corp",
            EmployeeIdentifier = "mario.nunez@acme.com"
        };
        ctx.Validate(); // must not throw
    }

    [Fact]
    public void Employee_MissingTenantIdentifier_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Employee,
            EmployeeIdentifier = "mario.nunez@acme.com"
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    [Fact]
    public void Employee_MissingEmployeeIdentifier_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Employee,
            TenantIdentifier = "acme-corp"
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    [Fact]
    public void Employee_WhitespaceEmployeeIdentifier_Throws()
    {
        var ctx = new IsolationContext
        {
            Level = IsolationLevel.Employee,
            TenantIdentifier = "acme-corp",
            EmployeeIdentifier = "   "
        };
        Assert.Throws<InvalidOperationException>(ctx.Validate);
    }

    #endregion
}
