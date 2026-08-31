# Ling.Audit.EntityFrameworkCore

Entity Framework Core persistence integration for Ling.Audit.

This package applies audit metadata during `SaveChanges`, maintains creation, modification, and deletion fields, records entity and field-level change logs, and provides soft-delete query filtering.

## Installation

```shell
dotnet add package Ling.Audit.EntityFrameworkCore
```

The package targets .NET 6.0 through .NET 10.0 and uses the matching EF Core major version.

## Quick start

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AuditEntityChangeLog<string>> EntityChanges =>
        Set<AuditEntityChangeLog<string>>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().IsAuditable();
        modelBuilder.SetupSoftDeleteQueryFilter();
    }
}

services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseAudit<MyUserProvider, string>();
});
```

Entities must be declared `partial` and implement one of the audit contracts from `Ling.Audit`, such as `ICreationAudited<TKey>` or `IFullAudited<TKey>`. The source generator supplies the matching properties at compile time.

## User context and transactions

Register an `AuditContextProviderBase<TUserId>` implementation to supply the current user, IP address, or client information. For ASP.NET Core request context integration, use [Ling.Audit.AspNetCore](https://www.nuget.org/packages/Ling.Audit.AspNetCore).

When using `AuditTransactionMode.RequireExplicitTransaction`, wrap business changes in an explicit EF Core transaction so the business write and its audit records commit or roll back together.

## Further reading

See the [repository documentation](https://github.com/ling921/ling-audit) for migration notes, transaction behavior, and the runnable sample.
