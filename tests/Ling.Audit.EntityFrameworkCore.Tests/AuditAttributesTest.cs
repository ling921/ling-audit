using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class AuditAttributesTest
{
    [Fact]
    public void AuditIgnoreAttribute_Should_BeDecoratedCorrectly()
    {
        // Assert
        typeof(NotAuditedAttribute).Should().BeDecoratedWith<AttributeUsageAttribute>(
            attr => attr.AllowMultiple == false &&
                   attr.Inherited == true &&
                   attr.ValidOn == AttributeTargets.Property);
    }

    [Fact]
    public void AuditIncludeAttribute_Should_BeDecoratedCorrectly()
    {
        // Assert
        typeof(AuditableAttribute).Should().BeDecoratedWith<AttributeUsageAttribute>(
            attr => attr.AllowMultiple == false &&
                   attr.Inherited == true);
    }

    [Fact]
    public void AuditIncludeAttribute_Should_StoreOperationType()
    {
        // Arrange & Act
        var attribute = new AuditableAttribute(DataOperation.Create | DataOperation.Modify);

        // Assert
        attribute.AnonymousOperations.Should().Be(DataOperation.Create | DataOperation.Modify);
        attribute.AnonymousOperations.Should().HaveFlag(DataOperation.Create);
        attribute.AnonymousOperations.Should().HaveFlag(DataOperation.Modify);
        attribute.AnonymousOperations.Should().NotHaveFlag(DataOperation.Delete);
    }

    [Fact]
    public async Task AuditIgnoreAttribute_Should_ExcludeProperty_FromAuditLogs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIgnoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"IgnoreTest_{Guid.NewGuid()}")
            .UseAudit<TestAuditContextProvider, int?>()
            .Options;

        // Act
        using var context = new TestIgnoreDbContext(options);
        context.Database.EnsureCreated();
        var entity = new TestEntityWithIgnore
        {
            Id = 1,
            Name = "Test",
            IgnoredProperty = "Should be ignored"
        };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        var auditLog = await context.AuditLogs
            .Include(a => a.Details)
            .FirstOrDefaultAsync();

        auditLog.Should().NotBeNull();
        auditLog!.Details.Should().NotContain(d => d.FieldName.Contains("IgnoredProperty"));
        auditLog.Details.Should().Contain(d => d.FieldName.Contains("Name"));
    }

    private class TestEntityWithIgnore
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        [NotAudited]
        public string IgnoredProperty { get; set; } = null!;
    }

    private class TestIgnoreDbContext : DbContext
    {
        public DbSet<TestEntityWithIgnore> TestEntities { get; set; } = null!;
        public DbSet<AuditEntityChangeLog<int?>> AuditLogs { get; set; } = null!;

        public TestIgnoreDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntityWithIgnore>(b =>
            {
                b.HasKey(e => e.Id);
                b.ToTable("TestEntities");
                b.IsAuditable(DataOperation.All);
            });

            modelBuilder.Entity<AuditEntityChangeLog<int?>>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasMany(e => e.Details)
                 .WithOne()
                 .HasForeignKey(e => e.EntityLogId);
                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity<AuditFieldChangeLog>(b =>
            {
                b.HasKey(e => e.Id);
                b.ToTable("AuditLogDetails");
            });
        }
    }
}
