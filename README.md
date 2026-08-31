# Ling.Audit

English | [简体中文](README.zh-CN.md)

`Ling.Audit` adds strongly typed audit properties to domain entities at compile time, maintains them through Entity Framework Core, and can resolve the current user and request information from ASP.NET Core. The core contracts remain independent of EF Core and ASP.NET Core, so applications can adopt only the layers they need.

## Packages

| Package | Purpose | Target frameworks |
| --- | --- | --- |
| [`Ling.Audit`](https://www.nuget.org/packages/Ling.Audit/) [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.svg)](https://www.nuget.org/packages/Ling.Audit/) | Audit contracts, source generator, analyzers, and code fixes. | .NET Standard 2.0 |
| [`Ling.Audit.EntityFrameworkCore`](https://www.nuget.org/packages/Ling.Audit.EntityFrameworkCore/) [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Ling.Audit.EntityFrameworkCore/) | Audit-property maintenance, soft deletion, and entity/field change logs. | .NET 6–10 |
| [`Ling.Audit.AspNetCore`](https://www.nuget.org/packages/Ling.Audit.AspNetCore/) [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.AspNetCore.svg)](https://www.nuget.org/packages/Ling.Audit.AspNetCore/) | User, IP address, and client information from the current HTTP request. | .NET 6–10 |

The EF Core and ASP.NET Core packages reference the matching EF Core major version for each target framework.

## Features

- Generates audit properties at compile time without reflection or runtime proxies.
- Provides focused interfaces for creation, modification, deletion, and full auditing.
- Supports classes, records, generic types, inheritance, and nested partial types.
- Reports invalid audit declarations through compiler diagnostics and code fixes.
- Automatically maintains audit fields during `SaveChanges`.
- Converts audited deletes into soft deletes and composes the soft-delete filter with existing query filters.
- Records entity-level and field-level changes, including composite and shadow primary keys.
- Resolves claims from ASP.NET Core Identity, JWT Bearer, OpenID Connect, and custom authentication schemes.
- Supports custom user providers, principal resolvers, serializers, anonymous handlers, and time providers.
- Offers an explicit-transaction mode when business changes and audit logs must commit atomically.

## Quick start

Install the core package in the project that owns the entity types:

```shell
dotnet add package Ling.Audit --version 2.1.0
```

Implement the audit interface that matches the entity lifecycle. The type and every containing type must be `partial`:

```csharp
using Ling.Audit;

namespace MyApplication.Domain;

public sealed partial class Post : IFullAudited<string>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

The source generator supplies the interface properties in the same compilation:

```csharp
public sealed partial class Post
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
```

Use a basic interface for one value, or a composite interface for a complete lifecycle:

| Interface | Generated properties |
| --- | --- |
| `IHasCreator<TKey>` | `CreatedBy` |
| `IHasCreationTime` | `CreatedAt` |
| `IHasModifier<TKey>` | `LastModifiedBy` |
| `IHasModificationTime` | `LastModifiedAt` |
| `ISoftDelete` | `IsDeleted` |
| `IHasDeleter<TKey>` | `DeletedBy` |
| `IHasDeletionTime` | `DeletedAt` |
| `ICreationAudited<TKey>` | `CreatedBy`, `CreatedAt` |
| `IModificationAudited<TKey>` | `LastModifiedBy`, `LastModifiedAt` |
| `IDeletionAudited<TKey>` | `IsDeleted`, `DeletedBy`, `DeletedAt` |
| `IFullAudited<TKey>` | All creation, modification, and deletion properties |

## Entity Framework Core

Install the integration that matches the application's target framework:

```shell
dotnet add package Ling.Audit.EntityFrameworkCore --version 2.1.0
```

Create a provider for the current user and client context:

```csharp
using Ling.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public sealed class ApplicationAuditUserProvider(
    ICurrentDbContext currentDbContext)
    : AuditContextProviderBase<Guid>(currentDbContext)
{
    protected override AuditContext? GetAuditContext()
    {
        var currentUser = GetService<ICurrentUser>();

        return currentUser?.Id is { } id
            ? new AuditContext(id, currentUser.Name, null, null)
            : null;
    }
}
```

Register auditing while configuring the `DbContext`:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseAudit<ApplicationAuditUserProvider, Guid>();
});
```

Mark only entities whose changes should be written to the audit-log tables. Properties are included by default and can be excluded individually:

```csharp
using Ling.Audit.EntityFrameworkCore;

[Auditable]
public sealed partial class Account : IFullAudited<Guid>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    [NotAudited]
    public string PasswordHash { get; set; } = string.Empty;
}
```

The same configuration is available through the model builder:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Account>().IsAuditable();
    modelBuilder.Entity<Account>()
        .Property(account => account.PasswordHash)
        .IsNotAudited();

    modelBuilder.SetupSoftDeleteQueryFilter();
    modelBuilder.ConfigureAuditLogTableNames(
        "AuditEntityChanges",
        "AuditFieldChanges");
}
```

`SetupSoftDeleteQueryFilter` combines its predicate with an existing filter instead of replacing tenant or authorization filters. Create a normal EF Core migration after enabling auditing so the audit properties and log tables are added to the database.

### Transaction guarantees

The default `BestEffort` mode preserves the two-save behavior used by earlier releases. Business changes can succeed even if the subsequent audit-log save fails.

For atomic behavior, require an application-owned transaction:

```csharp
options.UseAudit<ApplicationAuditUserProvider, Guid>(audit =>
{
    audit.TransactionMode = AuditTransactionMode.RequireExplicitTransaction;
});
```

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync(cancellationToken);

dbContext.Add(order);
await dbContext.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);
```

In this mode, an audited save without an active relational transaction is rejected. The business changes and generated audit rows participate in the same transaction.

### Anonymous operations

An audited operation with no resolved user is rejected by default. Allow selected operations on an entity when anonymous access is intentional:

```csharp
[Auditable(AllowedAnonymous = DataOperation.Create)]
public sealed partial class Registration : ICreationAudited<string>
{
}
```

Global behavior can be changed with `AuditOptions.AllowAnonymous`, or customized with `UseAnonymousHandler<THandler>()`.

## ASP.NET Core

For applications whose audit user ID is a string claim, install the ASP.NET Core package instead of configuring an EF user provider manually:

```shell
dotnet add package Ling.Audit.AspNetCore --version 2.1.0
```

```csharp
using Ling.Audit.AspNetCore;

services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseAspNetCoreAudit();
});
```

The default resolver selects the first authenticated `ClaimsIdentity` and resolves values in this order:

1. Explicit `UserIdClaimType` or `UserNameClaimType` options.
2. ASP.NET Core `IdentityOptions.ClaimsIdentity` claim types, when Identity is registered.
3. `ClaimsIdentity.Name` for the user name.
4. `ClaimTypes.NameIdentifier`, then `sub`, for the user ID.
5. `ClaimTypes.Name`, then `name`, for the user name.

Override claim types without changing the application's authentication configuration:

```csharp
options.UseAspNetCoreAudit(http =>
{
    http.UserIdClaimType = "employee_id";
    http.UserNameClaimType = "preferred_username";
    http.MaxClientNameLength = 256;
});
```

No `HttpContext` is treated as a background or non-HTTP operation and flows into the configured anonymous policy. The integration reads `HttpContext.User`; it does not invoke authentication again.

For multiple identities or application-specific selection rules, implement `IAuditPrincipalResolver`:

```csharp
public sealed class ApplicationPrincipalResolver : IAuditPrincipalResolver
{
    public AuditPrincipal Resolve(HttpContext httpContext)
    {
        var user = httpContext.User;
        return new AuditPrincipal(
            user.FindFirst("actor_id")?.Value,
            user.Identity?.Name);
    }
}
```

```csharp
options.UseAspNetCoreAudit<ApplicationPrincipalResolver>();
```

`Ling.Audit.AspNetCore` deliberately uses string user IDs. Applications using `Guid`, `int`, or another key type should use `Ling.Audit.EntityFrameworkCore` and register a matching `AuditContextProviderBase<TUserId>` implementation.

## Customization

- `UseSerializer<TSerializer>()` replaces property serialization.
- `UseAnonymousHandler<THandler>()` controls rejected anonymous operations.
- `UseAuditTimeProvider<TTimeProvider>()` supplies deterministic UTC timestamps.
- `UseAspNetCoreAudit<TPrincipalResolver>()` changes HTTP principal selection.

## Design

- `Ling.Audit` owns domain-facing contracts and compile-time tooling. It has no EF Core or ASP.NET Core dependency.
- `Ling.Audit.EntityFrameworkCore` owns persistence behavior, soft deletion, and change-log models. It does not depend on HTTP or claims APIs.
- `Ling.Audit.AspNetCore` adapts the current request to the EF Core user-provider contract. Principal resolution remains replaceable and does not perform authentication.

This separation keeps entity contracts reusable across web applications, workers, tests, and other hosts while making request-specific behavior opt-in.

## Migration from 1.x

The former `Ling.EntityFrameworkCore.Audit` package was renamed to `Ling.Audit.EntityFrameworkCore`. ASP.NET Core claim settings now belong to the dedicated ASP.NET Core integration, and anonymous audited operations are rejected by default.

See the [2.0 migration guide](docs/migrating-to-2.0.md) for package, API, transaction, and data-format changes.

## Samples

The repository includes a runnable ASP.NET Core Minimal API sample in [`samples/`](samples/). It combines `Ling.Audit`, `Ling.Audit.EntityFrameworkCore`, and `Ling.Audit.AspNetCore` with SQLite, and demonstrates authenticated user resolution, explicit transactions, soft deletion, and audit-log queries.

```shell
dotnet run --project samples/Ling.Audit.Sample/Ling.Audit.Sample.csproj
```

See [`samples/README.md`](samples/README.md) for the request examples and production notes.

## Development

```shell
dotnet build Ling.Audit.sln -c Release
dotnet test Ling.Audit.sln -c Release
```

The test matrix runs the EF Core and ASP.NET Core integration suites against .NET 6, 7, 8, 9, and 10. Pull requests that change behavior should include corresponding tests.

## License

[Apache License 2.0](LICENSE)
