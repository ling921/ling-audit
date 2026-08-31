using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

internal sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<CategoryEntity> Categories { get; set; } = default!;
    public DbSet<PostEntity> Posts { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEntity>(b =>
        {
            b.ToTable("Categories");
            b.HasKey(e => e.Id);
        });
        modelBuilder.Entity<PostEntity>(b =>
        {
            b.ToTable("Posts");
            b.HasKey(e => e.Id);
            b.IsAuditable(EntityOperationType.Delete);
            b.Property(e => e.CategoryId).IsAuditable();
        });
    }
}
