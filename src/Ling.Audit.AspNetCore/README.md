# Ling.Audit.AspNetCore

ASP.NET Core request-context integration for Ling.Audit.EntityFrameworkCore.

This package resolves the current user from `HttpContext.User` and supplies user ID, user name, remote IP address, and a bounded client name to the EF Core audit pipeline. It does not invoke authentication itself.

## Installation

```shell
dotnet add package Ling.Audit.AspNetCore
```

## Quick start

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseAspNetCoreAudit(http =>
    {
        http.UserIdClaimType = "sub";
        http.UserNameClaimType = "name";
        http.MaxClientNameLength = 256;
    });
});
```

The default principal resolver prefers explicit claim-type settings, then ASP.NET Core Identity claim settings, the authenticated identity's `Name`, and common `name identifier`/`sub` fallbacks. Register an `IAuditPrincipalResolver` when the application uses custom claims, multiple identities, or a non-standard selection rule.

```csharp
public sealed class TenantPrincipalResolver : IAuditPrincipalResolver
{
    public AuditPrincipal Resolve(HttpContext context) => new(
        context.User.FindFirst("employee_id")?.Value,
        context.User.Identity?.Name);
}

options.UseAspNetCoreAudit<TenantPrincipalResolver>();
```

The integration uses string user IDs. Applications using `Guid`, `int`, or another key type can configure `Ling.Audit.EntityFrameworkCore` directly with a matching user provider.

## Related package

This package depends on [Ling.Audit.EntityFrameworkCore](https://www.nuget.org/packages/Ling.Audit.EntityFrameworkCore), which provides the EF Core model configuration and audit persistence behavior.
