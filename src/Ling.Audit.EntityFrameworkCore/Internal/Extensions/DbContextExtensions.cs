using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ling.Audit.EntityFrameworkCore.Internal.Extensions;

internal static class DbContextExtensions
{
    internal static AuditOptions GetAuditOptions(this DbContext context)
    {
        var configuration = context.GetService<IConfiguration>();
        var options = configuration.GetSection(AuditDefaults.ConfigurationSection).Get<AuditOptions>() ?? new();

        var extension = context.GetService<IDbContextOptions>()
            .Extensions
            .OfType<AuditOptionsExtension>()
            .FirstOrDefault();

        extension?.Action?.Invoke(options);

        return options;
    }
}
