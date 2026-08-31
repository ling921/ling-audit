using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Ling.Audit.EntityFrameworkCore.Tests.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DbContextOptions _options;

    public AuditTests()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuditTest_{Guid.NewGuid()}")
            .UseAudit<TestAuditContextProvider, int?>(options =>
            {
                options.AllowAnonymousCreate = false;
                options.AllowAnonymousModify = false;
                options.AllowAnonymousDelete = false;
            })
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new TestDbContext(_options));
        services.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>(), 1, "TestUser"));

        _serviceProvider = services.BuildServiceProvider();

        // 初始化数据库
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task Create_Entity_Should_Generate_AuditLog()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        var user = new TestUser
        {
            Id = 1,
            Name = "TestUser",
            Age = 25
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await dbContext.AuditLogs
            .Include(a => a.Details)
            .FirstOrDefaultAsync();

        auditLog.Should().NotBeNull();
        auditLog!.EventType.Should().Be(AuditEventType.Create);
        auditLog.UserId.Should().Be(1);
        auditLog.UserName.Should().Be("TestUser");
        auditLog.EntityTypeName.Should().Be(nameof(TestUser));
        auditLog.Details.Should().NotBeEmpty();
        auditLog.Details.Should().Contain(d => d.FieldName.Contains("Name") && d.NewValue == "TestUser");
        auditLog.Details.Should().Contain(d => d.FieldName.Contains("Age") && d.NewValue == "25");
    }

    [Fact]
    public async Task Update_Entity_Should_Generate_AuditLog()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var user = new TestUser
        {
            Id = 1,
            Name = "OriginalName",
            Age = 30
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // 清除更改跟踪
        dbContext.ChangeTracker.Clear();

        // Act
        var userToUpdate = await dbContext.Users.FindAsync(user.Id);
        userToUpdate!.Name = "UpdatedName";
        userToUpdate.Age = 35;
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs
            .Include(a => a.Details)
            .OrderByDescending(a => a.EventTime)
            .ToListAsync();

        auditLogs.Should().HaveCount(2);
        var updateLog = auditLogs.First();
        updateLog.EventType.Should().Be(AuditEventType.Modify);
        updateLog.UserId.Should().Be(1);
        updateLog.Details.Should().Contain(d => d.FieldName.Contains("Name") && d.OriginalValue == "OriginalName" && d.NewValue == "UpdatedName");
        updateLog.Details.Should().Contain(d => d.FieldName.Contains("Age") && d.OriginalValue == "30" && d.NewValue == "35");
    }

    [Fact]
    public async Task Delete_Entity_Should_Generate_AuditLog()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var user = new TestUser
        {
            Id = 1,
            Name = "UserToDelete",
            Age = 40
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // 清除更改跟踪
        dbContext.ChangeTracker.Clear();

        // Act
        var userToDelete = await dbContext.Users.FindAsync(user.Id);
        dbContext.Users.Remove(userToDelete!);
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs
            .Include(a => a.Details)
            .OrderByDescending(a => a.EventTime)
            .ToListAsync();

        auditLogs.Should().HaveCount(2);
        var deleteLog = auditLogs.First();
        deleteLog.EventType.Should().Be(AuditEventType.SoftDelete);
        deleteLog.UserId.Should().Be(1);
    }

    [Fact]
    public async Task Anonymous_Creation_Should_Throw_Exception()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuditTest_{Guid.NewGuid()}")
            .UseAudit<TestAuditContextProvider, int?>(options =>
            {
                options.AllowAnonymousCreate = false;
            })
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new TestDbContext(options));
        services.AddScoped<IAuditUserProvider<int?>>(sp =>
            new TestAuditContextProvider(sp.GetRequiredService<ICurrentDbContext>()));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act & Assert
        var user = new TestUser
        {
            Id = 1,
            Name = "AnonymousUser"
        };
        dbContext.Users.Add(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync());
    }

    public void Dispose()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}
