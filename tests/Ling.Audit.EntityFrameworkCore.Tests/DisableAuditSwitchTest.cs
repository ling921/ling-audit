using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Ling.Audit.EntityFrameworkCore.Tests.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class DisableAuditSwitchTest : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DbContextOptions _options;

    public DisableAuditSwitchTest()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"DisableAuditTest_{Guid.NewGuid()}")
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
    public async Task When_DisableAuditingSwitch_Set_ShouldNot_GenerateAuditLogs()
    {
        // Arrange
        AppContext.SetSwitch(AuditDefaults.DisableAuditingSwitch, true);
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var user = new TestUser { Id = 1, Name = "TestUser" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().BeEmpty();

        // Clean up
        AppContext.SetSwitch(AuditDefaults.DisableAuditingSwitch, false);
    }

    [Fact]
    public async Task When_DisableAuditingSwitch_NotSet_Should_GenerateAuditLogs()
    {
        // Arrange
        AppContext.SetSwitch(AuditDefaults.DisableAuditingSwitch, false);
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var user = new TestUser { Id = 1, Name = "TestUser" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().NotBeEmpty();
    }

    public void Dispose()
    {
        // 恢复开关状态
        AppContext.SetSwitch(AuditDefaults.DisableAuditingSwitch, false);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}
