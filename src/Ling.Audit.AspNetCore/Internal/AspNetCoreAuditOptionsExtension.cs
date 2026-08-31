using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ling.Audit.AspNetCore.Internal;

internal sealed class AspNetCoreAuditOptionsExtension<TPrincipalResolver>(
    Action<AspNetCoreAuditOptions>? setupAction) : IDbContextOptionsExtension
    where TPrincipalResolver : class, IAuditPrincipalResolver
{
    internal Action<AspNetCoreAuditOptions>? SetupAction { get; } = setupAction;

    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddOptions<AspNetCoreAuditOptions>();
        if (SetupAction is not null)
        {
            services.Configure(SetupAction);
        }

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddScoped<IAuditPrincipalResolver, TPrincipalResolver>();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension)
        : DbContextOptionsExtensionInfo(extension)
    {
        private AspNetCoreAuditOptionsExtension<TPrincipalResolver> TypedExtension =>
            (AspNetCoreAuditOptionsExtension<TPrincipalResolver>)Extension;

        public override bool IsDatabaseProvider => false;
        public override string LogFragment => "using Ling.Audit.AspNetCore ";
        public override int GetServiceProviderHashCode() =>
            HashCode.Combine(typeof(TPrincipalResolver), TypedExtension.SetupAction);
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other.Extension is AspNetCoreAuditOptionsExtension<TPrincipalResolver> extension &&
            ReferenceEquals(TypedExtension.SetupAction, extension.SetupAction);
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["Ling.Audit.AspNetCore:Resolver"] = typeof(TPrincipalResolver).FullName!;
    }
}
