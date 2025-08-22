using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

public class TestAuditContextProvider : AuditContextProviderBase<int?>
{
    private readonly int? _userId;
    private readonly string? _userName;

    public TestAuditContextProvider(ICurrentDbContext current, int? userId = null, string? userName = null)
        : base(current)
    {
        _userId = userId;
        _userName = userName;
    }

    protected override AuditContext? GetAuditContext()
    {
        return new AuditContext(
            _userId,
            _userName,
            "127.0.0.1",
            "TestClient");
    }
}
