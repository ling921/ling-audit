using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Ling.Audit.EntityFrameworkCore.Tests.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditChangeLogTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DbContextOptions _options;

    public AuditChangeLogTests()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuditLogTest_{Guid.NewGuid()}")
            .UseAudit<TestAuditContextProvider, int?>()
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new TestDbContext(_options));
        services.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>(), 1, "TestUser"));

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task AuditEntityChangeLog_Should_Record_User_Information()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var user = new TestUser { Id = 1, Name = "TestUser" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await dbContext.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.UserId.Should().Be(1);
        auditLog.UserName.Should().Be("TestUser");
        auditLog.IPAddress.Should().Be("127.0.0.1");
        auditLog.ClientName.Should().Be("TestClient");
    }

    [Fact]
    public async Task AuditEntityChangeLog_Should_Record_Entity_Information()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var item = new TestItem { Name = "Test Item", Price = 9.99m, Stock = 10 };
        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await dbContext.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.EntityTypeName.Should().Be(nameof(TestItem));
        auditLog.EntityKey.Should().NotBeEmpty();
        auditLog.TableName.Should().Be("Items");
        auditLog.EventType.Should().Be(AuditEventType.Create);
    }

    [Fact]
    public async Task AuditEntityChangeLog_Should_Have_Details()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var item = new TestItem { Name = "Test Item", Price = 9.99m, Stock = 10 };
        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await dbContext.AuditLogs
            .Include(a => a.Details)
            .FirstOrDefaultAsync();

        auditLog.Should().NotBeNull();
        auditLog!.Details.Should().NotBeEmpty();
        auditLog.Details.Should().Contain(d => d.FieldName.Contains("Name") && d.NewValue == "Test Item");
        auditLog.Details.Should().Contain(d => d.FieldName.Contains("Price") && d.NewValue == "9.99");
        auditLog.Details.Should().Contain(d => d.FieldName.Contains("Stock") && d.NewValue == "10");
    }

    public void Dispose()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}
