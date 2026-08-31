using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

internal sealed class TestAuditContextProvider : AuditContextProviderBase<int?>
{
    public TestAuditContextProvider(ICurrentDbContext current) : base(current)
    {
    }

    protected override AuditContext? GetAuditContext()
    {
        return new(1, "Tom", "192.168.1.10", "test client");
    }
}

internal sealed class AnonymousAuditContextProvider : AuditContextProviderBase<int?>
{
    public AnonymousAuditContextProvider(ICurrentDbContext current) : base(current)
    {
    }

    protected override AuditContext? GetAuditContext()
    {
        return null;
    }
}
