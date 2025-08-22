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
    public static DbContextOptionsBuilder UseAudit<TUserProvider, [MustNull] TUserId>(
        this DbContextOptionsBuilder builder,
        Action<AuditOptions>? setupAction = null)
        where TUserProvider : AuditContextProviderBase<TUserId>
    {
        ArgumentNullException.ThrowIfNull(builder);

        var extension = builder.Options.FindExtension<AuditOptionsExtension<TUserProvider, TUserId>>()
            ?? new AuditOptionsExtension<TUserProvider, TUserId>(setupAction);

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

        // Add interceptor for audit
        builder.AddInterceptors(new AuditSaveChangesInterceptor<TUserId>());

        // Add model customizer for audit
        builder.ReplaceService<IModelCustomizer, AuditModelCustomizer<TUserId>>();

        return builder;
    }

    /// <summary>
    /// Configures the options builder to use a specified property serializer for the context.
    /// </summary>
    /// <typeparam name="TSerializer">This type parameter specifies the class that will handle property serialization.</typeparam>
    /// <param name="builder">This parameter is the options builder being configured for the database context.</param>
    /// <returns>Returns the modified options builder with the new property serializer.</returns>
    public static DbContextOptionsBuilder UseSerializer<TSerializer>(this DbContextOptionsBuilder builder)
        where TSerializer : class, IPropertySerializer
    {
        builder.ReplaceService<IPropertySerializer, TSerializer>();

        return builder;
    }

    /// <summary>
    /// Configures the options builder to use custom handler for handling anonymous audit operations.
    /// </summary>
    /// <typeparam name="THandler">The type of handler</typeparam>
    /// <param name="builder">This parameter is the options builder being configured for the database context.</param>
    /// <returns>Returns the modified options builder with the custom handler.</returns>
    public static DbContextOptionsBuilder UseAnonymousHandler<THandler>(this DbContextOptionsBuilder builder)
        where THandler : class, IAuditAnonymousHandler
    {
        builder.ReplaceService<IAuditAnonymousHandler, THandler>();

        return builder;
    }
}
