using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Ling.Audit.EntityFrameworkCore.Tests.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditOptionsTests : IDisposable
{
    private readonly string _dbName;

    public AuditOptionsTests()
    {
        _dbName = $"AuditOptionsTest_{Guid.NewGuid()}";
    }

    [Fact]
    public async Task AllowAnonymousCreate_True_Should_Allow_Anonymous_Creation()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .UseAudit<TestAuditContextProvider, int?>(options =>
            {
                options.AllowAnonymousCreate = true;
            })
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new TestDbContext(options));
        services.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>()));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var user = new TestUser
        {
            Id = 1,
            Name = "AnonymousUser"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await dbContext.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.UserId.Should().BeNull();
        auditLog.UserName.Should().BeNull();
    }

    [Fact]
    public async Task AllowAnonymousModify_False_Should_Throw_Exception()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .UseAudit<TestAuditContextProvider, int?>(options =>
            {
                options.AllowAnonymousCreate = true;
                options.AllowAnonymousModify = false;
            })
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new TestDbContext(options));

        // 先用有用户ID的上下文添加实体
        services.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>(), 1, "TestUser"));

        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var user = new TestUser
            {
                Id = 1,
                Name = "OriginalName"
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        // 再用匿名上下文修改实体
        var anonymousServices = new ServiceCollection();
        anonymousServices.AddScoped(sp => new TestDbContext(options));
        anonymousServices.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>()));

        var anonymousProvider = anonymousServices.BuildServiceProvider();
        using var anonymousScope = anonymousProvider.CreateScope();
        var anonymousContext = anonymousScope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act & Assert
        var user1 = await anonymousContext.Users.FirstAsync();
        user1.Name = "UpdatedName";

        await Assert.ThrowsAsync<InvalidOperationException>(() => anonymousContext.SaveChangesAsync());
    }

    [Fact]
    public async Task AuditNoFieldChangeEntity_True_Should_Audit_Unchanged_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .UseAudit<TestAuditContextProvider, int?>(options =>
            {
                options.AuditNoFieldChangeEntity = true;
            })
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new TestDbContext(options));
        services.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>(), 1, "TestUser"));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // 添加实体
        var user = new TestUser { Id = 1, Name = "TestUser" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // 清除跟踪
        dbContext.ChangeTracker.Clear();

        // Act - 不修改任何字段，但标记为修改状态
        var userToModify = await dbContext.Users.FindAsync(user.Id);
        dbContext.Entry(userToModify!).State = EntityState.Modified;
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs
            .Include(a => a.Details)
            .OrderByDescending(a => a.EventTime)
            .ToListAsync();

        auditLogs.Should().HaveCount(2);
        var modifyLog = auditLogs.First();
        modifyLog.EventType.Should().Be(AuditEventType.Modify);
        modifyLog.Details.Should().BeEmpty();
    }

    public void Dispose()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .Options;

        using var context = new TestDbContext(options);
        context.Database.EnsureDeleted();
    }
}
