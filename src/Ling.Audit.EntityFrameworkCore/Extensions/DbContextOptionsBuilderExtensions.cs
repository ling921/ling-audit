using Ling.Audit.EntityFrameworkCore.Internal;
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
    public static DbContextOptionsBuilder UseAudit<TUserProvider, TUserId>(
        this DbContextOptionsBuilder builder,
        Action<AuditOptions>? setupAction = null)
        where TUserProvider : class, IAuditContextProvider<TUserId>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddAuditOptions(setupAction, o => o.UserIdType = typeof(TUserId));

        builder.AddAuditUserProvider<TUserProvider, TUserId>();
        builder.AddAuditConvention();
        builder.AddAuditInterceptor<TUserId>();
        builder.AddModelCustomizer<TUserId>();

        return builder;
    }

    internal static DbContextOptionsBuilder AddAuditOptions(
        this DbContextOptionsBuilder builder,
        Action<AuditOptions>? setupAction,
        Action<AuditOptions>? additional = null)
    {
        var extension = builder.Options.FindExtension<AuditOptionsExtension>();
        if (extension is null)
        {
            var combine = (setupAction ?? delegate { }) + (additional ?? delegate { });
            extension = new AuditOptionsExtension(combine);
        }

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

        return builder;
    }

    internal static DbContextOptionsBuilder AddAuditUserProvider<TUserProvider, TUserId>(this DbContextOptionsBuilder builder)
            where TUserProvider : class, IAuditContextProvider<TUserId>
    {
        var extension = builder.Options.FindExtension<UserProviderExtension<TUserProvider, TUserId>>()
            ?? new UserProviderExtension<TUserProvider, TUserId>();

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

        return builder;
    }

    internal static DbContextOptionsBuilder AddAuditConvention(this DbContextOptionsBuilder builder)
    {
        var extension = builder.Options.FindExtension<AuditConventionExtension>() ?? new AuditConventionExtension();

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

        return builder;
    }

    internal static DbContextOptionsBuilder AddAuditInterceptor<TUserId>(this DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditInterceptor<TUserId>());
        builder.ReplaceService<IModelCustomizer, AuditModelCustomizer<TUserId>>();

        return builder;
    }

    internal static DbContextOptionsBuilder AddModelCustomizer<TUserId>(this DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditInterceptor<TUserId>());
        builder.ReplaceService<IModelCustomizer, AuditModelCustomizer<TUserId>>();

        return builder;
    }
}
