using Microsoft.AspNetCore.Http;

namespace Ling.Audit.AspNetCore;

/// <summary>
/// Resolves audit principal information from an HTTP request.
/// </summary>
public interface IAuditPrincipalResolver
{
    /// <summary>
    /// Resolves audit principal information from <paramref name="httpContext"/>.
    /// </summary>
    AuditPrincipal Resolve(HttpContext httpContext);
}

/// <summary>
/// Represents user information resolved from a claims principal.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="UserName">The user name.</param>
public sealed record AuditPrincipal(string? UserId, string? UserName);
