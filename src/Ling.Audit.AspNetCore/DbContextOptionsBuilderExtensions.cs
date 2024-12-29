using Ling.Audit.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Sets the <see cref="ISaveChangesInterceptor"/>, <see cref="IModelCustomizer"/>, <see
    /// cref="AuditOptions"/> to be used for the auditing.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder"/>.</param>
    /// <param name="setupAction">The action used to configure the <see cref="AuditOptions"/>.</param>
    public static DbContextOptionsBuilder UseAudit(this DbContextOptionsBuilder builder, Action<AuditOptions>? setupAction = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddAuditOptions(setupAction);

        builder.AddAuditUserProvider<HttpAuditContextProvider, string?>();
        builder.AddAuditConvention();
        builder.AddAuditInterceptor<string?>();
        builder.AddModelCustomizer<string?>();

        return builder;
    }
}
