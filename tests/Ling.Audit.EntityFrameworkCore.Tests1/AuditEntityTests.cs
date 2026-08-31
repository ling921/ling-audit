using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditEntityTests
{
    private readonly DbContextOptions _options;
    private readonly TestDbContext _dbContext;

    public AuditEntityTests()
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "AuditTestDb")
            .Options;
        _dbContext = new TestDbContext(_options);
    }

    [Fact]
    public async Task EntityCreate_ShouldGenerateAuditLog()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid(), Name = "TestUser" };
        _dbContext.Users.Add(user);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.EntityTypeName.Should().Be(nameof(TestUser));
        auditLog.EventType.Should().Be(AuditEventType.Create);
        auditLog.Details.Should().HaveCount(2); // Id and Name
    }

    [Fact]
    public async Task EntityUpdate_ShouldGenerateAuditLog()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid(), Name = "OriginalName" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.Name = "UpdatedName";
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(2);
        var updateLog = auditLogs[0];
        updateLog.EventType.Should().Be(AuditEventType.Modify);
        updateLog.Details.Should().ContainSingle(d => d.FieldName == "Name" && d.NewValue == "UpdatedName");
    }

    [Fact]
    public async Task EntityDelete_ShouldGenerateAuditLog()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid(), Name = "TestUser" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(2);
        var deleteLog = auditLogs[0];
        deleteLog.EventType.Should().Be(AuditEventType.Delete);
    }

    [Fact]
    public async Task AuditLog_ShouldTrackAllChangedProperties()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid(), Name = "OriginalName", Age = 20 };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.Name = "NewName";
        user.Age = 21;
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLog = await _dbContext.AuditLogs
            .Include(x => x.Details)
            .OrderByDescending(x => x.EventTime)
            .FirstAsync();

        auditLog.Details.Should().HaveCount(2);
        auditLog.Details.Should().Contain(d => d.FieldName == "Name" && d.NewValue == "NewName");
        auditLog.Details.Should().Contain(d => d.FieldName == "Age" && d.NewValue == "21");
    }

    [Fact]
    public async Task ConcurrentUpdates_ShouldGenerateCorrectAuditLogs()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid(), Name = "OriginalName", Age = 20 };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var tasks = new[]
        {
            Task.Run(async () =>
            {
                using var context = new TestDbContext(_options);
                var user2 = await context.Users.FindAsync(user.Id);
                user2!.Name = "Name1";
                await context.SaveChangesAsync();
            }),
            Task.Run(async () =>
            {
                using var context = new TestDbContext(_options);
                var user3 = await context.Users.FindAsync(user.Id);
                user3!.Age = 21;
                await context.SaveChangesAsync();
            })
        };

        await Task.WhenAll(tasks);

        // Assert
        var auditLogs = await _dbContext.AuditLogs
            .Include(x => x.Details)
            .OrderByDescending(x => x.EventTime)
            .ToListAsync();

        auditLogs.Should().HaveCount(3); // 创建 + 2次更新
        var lastTwoLogs = auditLogs.Take(2).ToList();
        lastTwoLogs.Should().Contain(l => l.Details.Any(d => d.FieldName == "Name" && d.NewValue == "Name1"));
        lastTwoLogs.Should().Contain(l => l.Details.Any(d => d.FieldName == "Age" && d.NewValue == "21"));
    }

    [Fact]
    public async Task ComplexEntity_ShouldTrackNavigationProperties()
    {
        // Arrange
        var department = new TestDepartment { Id = Guid.NewGuid(), Name = "IT" };
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Name = "TestUser",
            Department = department,
            Roles = new List<TestRole>
            {
                new TestRole { Id = Guid.NewGuid(), Name = "Admin" },
                new TestRole { Id = Guid.NewGuid(), Name = "User" }
            }
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.Name = "UpdatedUser";
        user.Department.Name = "HR";
        user.Roles.Add(new TestRole { Id = Guid.NewGuid(), Name = "Manager" });
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs
            .Include(x => x.Details)
            .OrderByDescending(x => x.EventTime)
            .ToListAsync();

        auditLogs.Should().HaveCount(4); // 创建Department + 创建User + 更新User + 创建Role
        var userUpdateLog = auditLogs.First(l => l.EntityTypeName == nameof(TestUser));
        userUpdateLog.Details.Should().Contain(d => d.FieldName == "Name" && d.NewValue == "UpdatedUser");
    }

    [Fact]
    public async Task AuditLog_ShouldHandleNullNavigationProperties()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Name = "TestUser",
            Department = null,
            Roles = new List<TestRole>()
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.Department = new TestDepartment { Id = Guid.NewGuid(), Name = "IT" };
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await _dbContext.AuditLogs
            .Include(x => x.Details)
            .OrderByDescending(x => x.EventTime)
            .ToListAsync();

        auditLogs.Should().HaveCount(3); // 创建User + 创建Department + 更新User
        var userUpdateLog = auditLogs.First(l => l.EntityTypeName == nameof(TestUser));
        userUpdateLog.Details.Should().Contain(d => d.FieldName == "DepartmentId");
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestUser> Users { get; set; } = null!;
        public DbSet<TestDepartment> Departments { get; set; } = null!;
        public DbSet<TestRole> Roles { get; set; } = null!;
        public DbSet<AuditEntityChangeLog<Guid?>> AuditLogs { get; set; } = null!;

        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestUser>().HasKey(u => u.Id);
            modelBuilder.Entity<TestDepartment>().HasKey(d => d.Id);
            modelBuilder.Entity<TestRole>().HasKey(r => r.Id);
            modelBuilder.Entity<AuditEntityChangeLog<Guid?>>().HasKey(a => a.Id);
            modelBuilder.Entity<AuditFieldChangeLog>().HasKey(a => a.Id);

            modelBuilder.Entity<TestUser>()
                .HasOne(u => u.Department)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId);

            modelBuilder.Entity<TestUser>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));
        }
    }

    private class TestUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public Guid? DepartmentId { get; set; }
        public TestDepartment? Department { get; set; }
        public ICollection<TestRole> Roles { get; set; } = new List<TestRole>();
    }

    private class TestDepartment
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    private class TestRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<TestUser> Users { get; set; } = new List<TestUser>();
    }
}
