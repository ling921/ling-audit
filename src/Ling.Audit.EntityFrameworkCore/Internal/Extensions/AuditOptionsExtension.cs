using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore.Internal.Extensions;

internal sealed class AuditOptionsExtension<TUserProvider, TUserId> : IDbContextOptionsExtension
    where TUserProvider : class, IAuditUserProvider<TUserId>
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
        JsonSerializerOptions? jsonSerializerOptions = null;

        // Configure audit options
        if (Action is null)
        {
            services.AddOptions<AuditOptions>()
                .Configure<ICurrentDbContext>((options, context) =>
                    context.Context.GetService<IConfiguration>()
                        .GetSection(Constants.ConfigurationSection)
                        .Bind(options));
        }
        else
        {
            services.Configure(Action);

            var tempOptions = new AuditOptions();
            Action.Invoke(tempOptions);
            jsonSerializerOptions = tempOptions.PropertySerializerOptions;
        }

        // Add serializer for property conversion
        services.TryAddSingleton<IPropertySerializer>(
            new DefaultPropertySerializer(jsonSerializerOptions));

        // Add handler for anonymous audit operations
        services.TryAddSingleton<IAuditAnonymousHandler, DefaultAuditAnonymousHandler>();

        // Add the UTC time source used for audit fields and log events.
        services.TryAddSingleton<IAuditTimeProvider, SystemAuditTimeProvider>();

        // Add custom plugin for audit annotations
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IConventionSetPlugin, AuditConventionSetPlugin>());

        // Add audit context for user
        services.TryAddScoped<IAuditUserProvider<TUserId>, TUserProvider>();
    }

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
    }

    private class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private readonly AuditOptionsExtension<TUserProvider, TUserId> _extension;

        public override bool IsDatabaseProvider { get; }
        public override string LogFragment { get; } = string.Empty;

        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
            _extension = (AuditOptionsExtension<TUserProvider, TUserId>)extension;
        }

        public override int GetServiceProviderHashCode() => _extension.Action?.GetHashCode() ?? 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other is ExtensionInfo info && ReferenceEquals(_extension.Action, info._extension.Action);

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Ling.Audit:Options"] = GetServiceProviderHashCode().ToString();
        }
    }
}
