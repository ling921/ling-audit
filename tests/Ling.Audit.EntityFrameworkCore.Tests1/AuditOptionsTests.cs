using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditOptionsTests
{
    [Fact]
    public void AuditOptions_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new AuditOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.IncludeEntityObjects.Should().BeFalse();
        options.SaveTempProperties.Should().BeFalse();
        options.AuditEventType.Should().Be(AuditEventType.All);
        options.IncludeIndirectChanges.Should().BeFalse();
    }

    [Fact]
    public void AuditOptions_ShouldAllowCustomConfiguration()
    {
        // Arrange
        var options = new AuditOptions
        {
            Enabled = false,
            IncludeEntityObjects = true,
            SaveTempProperties = true,
            AuditEventType = AuditEventType.Created | AuditEventType.Deleted,
            IncludeIndirectChanges = true
        };

        // Assert
        options.Enabled.Should().BeFalse();
        options.IncludeEntityObjects.Should().BeTrue();
        options.SaveTempProperties.Should().BeTrue();
        options.AuditEventType.Should().Be(AuditEventType.Created | AuditEventType.Deleted);
        options.IncludeIndirectChanges.Should().BeTrue();
    }

    [Fact]
    public async Task AuditOptions_ShouldRespectEnabledSetting()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "AuditTestDb")
            .Options;
        var dbContext = new TestDbContext(options, new AuditOptions { Enabled = false });

        var user = new TestUser { Id = Guid.NewGuid(), Name = "TestUser" };
        dbContext.Users.Add(user);

        // Act
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task AuditOptions_ShouldRespectAuditEventTypeSetting()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "AuditTestDb")
            .Options;
        var dbContext = new TestDbContext(options, new AuditOptions { AuditEventType = AuditEventType.Created });

        var user = new TestUser { Id = Guid.NewGuid(), Name = "TestUser" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        user.Name = "UpdatedName";
        await dbContext.SaveChangesAsync();

        // Assert
        var auditLogs = await dbContext.AuditLogs.ToListAsync();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].EventType.Should().Be(AuditEventType.Created);
    }

    private class TestDbContext : DbContext
    {
        private readonly AuditOptions _auditOptions;

        public DbSet<TestUser> Users { get; set; } = null!;
        public DbSet<AuditEntityChangeLog<Guid>> AuditLogs { get; set; } = null!;

        public TestDbContext(DbContextOptions options, AuditOptions auditOptions)
            : base(options)
        {
            _auditOptions = auditOptions;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestUser>().HasKey(u => u.Id);
            modelBuilder.Entity<AuditEntityChangeLog<Guid>>().HasKey(a => a.Id);
            modelBuilder.Entity<AuditFieldChangeLog>().HasKey(a => a.Id);
        }
    }

    private class TestUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}