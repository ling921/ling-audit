using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
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
    public static DbContextOptionsBuilder UseAudit<TUserProvider, [MustNull] TUserId>(
        this DbContextOptionsBuilder builder,
        Action<AuditOptions>? setupAction = null)
        where TUserProvider : class, IAuditContextProvider<TUserId>
    {
        ArgumentNullException.ThrowIfNull(builder);

        var extension = builder.Options.FindExtension<AuditOptionsExtension<TUserProvider, TUserId>>()
            ?? new AuditOptionsExtension<TUserProvider, TUserId>(setupAction);

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

        return builder;
    }

    public static DbContextOptionsBuilder UseSerializer<TSerializer>(this DbContextOptionsBuilder builder)
        where TSerializer : class, IPropertySerializer
    {
        builder.ReplaceService<IPropertySerializer, TSerializer>();

        return builder;
    }
}
