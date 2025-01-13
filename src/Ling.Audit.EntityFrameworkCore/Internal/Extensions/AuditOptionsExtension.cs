using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Ling.Audit.EntityFrameworkCore.Internal.Extensions;

internal sealed class AuditOptionsExtension<TUserProvider, TUserId> : IDbContextOptionsExtension
    where TUserProvider : class, IAuditContextProvider<TUserId>
{
    public Action<AuditOptions>? Action { get; }

    /// <inheritdoc/>
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public AuditOptionsExtension(Action<AuditOptions>? setupAction)
    {
        Action = setupAction;
    }

    public AuditOptionsExtension([NotNull] AuditOptionsExtension<TUserProvider, TUserId> copyFrom)
    {
        Action = copyFrom.Action;
    }

    /// <inheritdoc/>
    public void ApplyServices(IServiceCollection services)
    {
        // Configure audit options
        if (Action is null)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<AuditOptions>, AuditOptionsConfigure>());
        }
        else
        {
            services.Configure(Action);
        }

        // Add serializer for property conversion
        services.TryAddSingleton<IPropertySerializer, DefaultPropertySerializer>();

        // Add custom plugin for audit annotations
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IConventionSetPlugin, AuditConventionSetPlugin>());

        // Add audit context for user
        services.TryAddScoped<IAuditContextProvider<TUserId>, TUserProvider>();
    }

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
    }

    private class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public override bool IsDatabaseProvider { get; }
        public override string LogFragment { get; } = string.Empty;

        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
