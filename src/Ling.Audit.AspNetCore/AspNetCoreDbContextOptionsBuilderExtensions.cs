using Ling.Audit.AspNetCore.Internal;
using Ling.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ling.Audit.AspNetCore;

/// <summary>
/// Extension methods for configuring HTTP request based auditing.
/// </summary>
public static class AspNetCoreDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures auditing to resolve string user identifiers from the current HTTP request.
    /// </summary>
    public static DbContextOptionsBuilder UseAspNetCoreAudit(
        this DbContextOptionsBuilder builder,
        Action<AspNetCoreAuditOptions>? setupAction = null,
        Action<AuditOptions>? auditSetupAction = null)
    {
        return builder.UseAspNetCoreAudit<DefaultAuditPrincipalResolver>(setupAction, auditSetupAction);
    }

    /// <summary>
    /// Configures auditing with a custom HTTP principal resolver.
    /// </summary>
    public static DbContextOptionsBuilder UseAspNetCoreAudit<TPrincipalResolver>(
        this DbContextOptionsBuilder builder,
        Action<AspNetCoreAuditOptions>? setupAction = null,
        Action<AuditOptions>? auditSetupAction = null)
        where TPrincipalResolver : class, IAuditPrincipalResolver
    {
        ArgumentNullException.ThrowIfNull(builder);

        var extension = builder.Options.FindExtension<AspNetCoreAuditOptionsExtension<TPrincipalResolver>>()
            ?? new AspNetCoreAuditOptionsExtension<TPrincipalResolver>(setupAction);

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);
        return builder.UseAudit<HttpAuditUserProvider, string>(auditSetupAction);
    }
}
