using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Ling.Audit.AspNetCore;

internal sealed class DefaultAuditPrincipalResolver(
    IOptions<AspNetCoreAuditOptions> auditOptions,
    IOptions<IdentityOptions> identityOptions) : IAuditPrincipalResolver
{
    public AuditPrincipal Resolve(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var options = auditOptions.Value;
        var principal = httpContext.User;
        var identity = principal.Identities.FirstOrDefault(i => i.IsAuthenticated);

        if (identity is null && !options.RequireAuthenticatedIdentity)
        {
            identity = principal.Identities.FirstOrDefault();
        }

        if (identity is null)
        {
            return new AuditPrincipal(null, null);
        }

        var identityClaims = identityOptions.Value.ClaimsIdentity;
        var userId = FindFirst(identity, options.UserIdClaimType)
            ?? FindFirst(identity, identityClaims.UserIdClaimType)
            ?? FindFirst(identity, ClaimTypes.NameIdentifier)
            ?? FindFirst(identity, "sub");

        var userName = FindFirst(identity, options.UserNameClaimType)
            ?? identity.Name
            ?? FindFirst(identity, identityClaims.UserNameClaimType)
            ?? FindFirst(identity, ClaimTypes.Name)
            ?? FindFirst(identity, "name");

        return new AuditPrincipal(userId, userName);
    }

    private static string? FindFirst(ClaimsIdentity identity, string? claimType)
    {
        return string.IsNullOrWhiteSpace(claimType)
            ? null
            : identity.FindFirst(claimType)?.Value;
    }
}
