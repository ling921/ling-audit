# Migrating to Ling.Audit.EntityFrameworkCore 2.0

The EF Core package has moved from `Ling.EntityFrameworkCore.Audit` to `Ling.Audit.EntityFrameworkCore`. The old package should be deprecated on NuGet.org with the new package configured as its recommended replacement.

## Package changes

```shell
dotnet remove package Ling.EntityFrameworkCore.Audit
dotnet add package Ling.Audit.EntityFrameworkCore --version 2.0.1
```

ASP.NET Core applications can additionally install `Ling.Audit.AspNetCore`. The core EF package does not depend on HTTP or claims APIs.

## Configuration changes

- Use `UseAudit<TUserProvider, TUserId>()` for framework-independent EF Core auditing.
- Use `UseAspNetCoreAudit()` for the built-in string claim provider.
- Claim settings now belong to `AspNetCoreAuditOptions`, not `AuditOptions`.
- Custom HTTP principal selection is implemented with `IAuditPrincipalResolver`.
- Anonymous operations are rejected by default. Set `AuditOptions.AllowAnonymous`, configure `AuditableAttribute.AllowedAnonymous`, or register a custom `IAuditAnonymousHandler` to opt in.
- Primary keys in new audit rows are stored as JSON objects, including composite and shadow keys.

## Transaction behavior

The default `BestEffort` mode writes audit rows immediately after the audited save and is backward-compatible, but the two saves may use separate transactions. Set `TransactionMode` to `RequireExplicitTransaction` and begin a transaction before saving when atomicity is required.

## Target frameworks

Version 2.0 of the EF Core and ASP.NET Core packages provides assets for .NET 6, 7, 8 and 9, each compiled against the corresponding EF Core major version. .NET 10 support is planned as an additive 2.1 release.
