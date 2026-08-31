using Ling.Audit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public sealed class AuditIntegrationTests
{
    [Fact]
    public async Task SaveChanges_WritesAuditLogAndUsesSingleUtcTimestamp()
    {
        await using var fixture = await AuditFixture.CreateAsync();
        var entity = new TestEntity { Name = "created", TenantId = 1 };
        fixture.Context.Entities.Add(entity);

        await fixture.Context.SaveChangesAsync();

        var log = await fixture.Context.Set<AuditEntityChangeLog<int?>>()
            .Include(x => x.Details)
            .SingleAsync();

        entity.CreatedBy.Should().Be(42);
        entity.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
        log.EventTime.Should().Be(entity.CreatedAt);
        log.UserId.Should().Be(42);
        log.Details.Should().Contain(x => x.FieldName.EndsWith(".Name"));
    }

    [Fact]
    public async Task QueryFilter_CombinesTenantAndSoftDeleteFilters()
    {
        await using var fixture = await AuditFixture.CreateAsync();
        fixture.Context.Entities.AddRange(
            new TestEntity { Name = "visible", TenantId = 1 },
            new TestEntity { Name = "other tenant", TenantId = 2 },
            new TestEntity { Name = "deleted", TenantId = 1, IsDeleted = true });
        await fixture.Context.SaveChangesAsync();

        var entities = await fixture.Context.Entities.ToListAsync();

        entities.Should().ContainSingle(x => x.Name == "visible");
    }

    [Fact]
    public async Task RequireExplicitTransaction_RejectsSaveWithoutTransaction()
    {
        await using var fixture = await AuditFixture.CreateAsync(AuditTransactionMode.RequireExplicitTransaction);
        fixture.Context.Entities.Add(new TestEntity { Name = "requires transaction", TenantId = 1 });

        var action = () => fixture.Context.SaveChangesAsync();

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*explicit database transaction*");
    }

    [Fact]
    public async Task RequireExplicitTransaction_RollsBackEntityAndAuditLogTogether()
    {
        await using var fixture = await AuditFixture.CreateAsync(AuditTransactionMode.RequireExplicitTransaction);
        await using (var transaction = await fixture.Context.Database.BeginTransactionAsync())
        {
            fixture.Context.Entities.Add(new TestEntity { Name = "rollback", TenantId = 1 });
            await fixture.Context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        fixture.Context.ChangeTracker.Clear();
        (await fixture.Context.Entities.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await fixture.Context.Set<AuditEntityChangeLog<int?>>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AuditableAttribute_AllowsConfiguredAnonymousOperation()
    {
        await using var fixture = await AuditFixture.CreateAsync(anonymous: true);
        fixture.Context.AnonymousEntities.Add(new AnonymousEntity { Name = "allowed" });

        await fixture.Context.SaveChangesAsync();

        (await fixture.Context.Set<AuditEntityChangeLog<int?>>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DefaultAnonymousHandler_RejectsOperation()
    {
        await using var fixture = await AuditFixture.CreateAsync(anonymous: true);
        fixture.Context.Entities.Add(new TestEntity { Name = "rejected", TenantId = 1 });

        var action = () => fixture.Context.SaveChangesAsync();

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Anonymous Create operation is not allowed*");
    }

    [Fact]
    public async Task SerializerOptions_AreAppliedToDefaultSerializer()
    {
        await using var fixture = await AuditFixture.CreateAsync(configure: options =>
            options.PropertySerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        var serializer = fixture.Context.GetService<IPropertySerializer>();

        var value = serializer.Serialize(new ComplexValue { DisplayName = "value" }, typeof(ComplexValue));

        value.Should().Contain("\"displayName\"");
    }

    private sealed class AuditFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public TestDbContext Context { get; }

        private AuditFixture(SqliteConnection connection, TestDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public static async Task<AuditFixture> CreateAsync(
            AuditTransactionMode transactionMode = AuditTransactionMode.BestEffort,
            bool anonymous = false,
            Action<AuditOptions>? configure = null)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var builder = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection);

            if (anonymous)
            {
                builder.UseAudit<AnonymousUserProvider, int?>(o =>
                {
                    o.TransactionMode = transactionMode;
                    configure?.Invoke(o);
                });
            }
            else
            {
                builder.UseAudit<TestUserProvider, int?>(o =>
                {
                    o.TransactionMode = transactionMode;
                    configure?.Invoke(o);
                });
            }

            var context = new TestDbContext(builder.Options);
            await context.Database.EnsureCreatedAsync();
            return new AuditFixture(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();
        public DbSet<AnonymousEntity> AnonymousEntities => Set<AnonymousEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasQueryFilter(x => x.TenantId == 1);
            });
            modelBuilder.Entity<AnonymousEntity>().HasKey(x => x.Id);
        }
    }

    [Auditable]
    private sealed class TestEntity : IFullAudited<int?>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }

    [Auditable(AllowedAnonymous = DataOperation.Create)]
    private sealed class AnonymousEntity : ICreationAudited<int?>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    private sealed class TestUserProvider(ICurrentDbContext current) : AuditContextProviderBase<int?>(current)
    {
        protected override AuditContext GetAuditContext() => new(42, "Tester", "127.0.0.1", "Tests");
    }

    private sealed class AnonymousUserProvider(ICurrentDbContext current) : AuditContextProviderBase<int?>(current)
    {
        protected override AuditContext? GetAuditContext() => null;
    }

    private sealed class ComplexValue
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
