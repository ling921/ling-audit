using Microsoft.EntityFrameworkCore;

namespace Ling.Audit.EntityFrameworkCore.Tests.Base;

public class TestDbContext(DbContextOptions options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
            {
                builder.IsAuditable(DataOperation.Create);
                builder.HasData(new User()
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john@example.com",
                    Age = 20,
                });
            });

        modelBuilder.Entity<Product>(builder =>
        {
            builder.IsAuditable(DataOperation.Create | DataOperation.Modify);
            builder.Property(x => x.Stock).IsNotAudited(false);
        });
    }
}

public partial class User : ICreationAudited<Guid?>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int Age { get; set; }
}

[Auditable]
public partial class Product : ICreationAudited<Guid?>, IModificationAudited<Guid?>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

[Auditable(DataOperation.All)]
public partial class Order : IFullAudited<Guid?>
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}
