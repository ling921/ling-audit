using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Ling.Audit.EntityFrameworkCore.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Ling.Audit.EntityFrameworkCore.Internal;

internal sealed class AuditModelCustomizer<TUserId> : ModelCustomizer
{
    private readonly ILogger _logger;

    public AuditModelCustomizer(
        ModelCustomizerDependencies dependencies,
        ILoggerFactory loggerFactory) : base(dependencies)
    {
        _logger = loggerFactory.CreateLogger<AuditModelCustomizer<TUserId>>();
    }

    /// <inheritdoc/>
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        var auditOptions = context.GetAuditOptions();

        modelBuilder.ApplyConfiguration(new AuditEntityLogTypeConfiguration<TUserId?>());
        modelBuilder.ApplyConfiguration(new AuditFieldLogTypeConfiguration());

        modelBuilder.ConfigureAuditableEntities(auditOptions);

        modelBuilder.SetupSoftDeleteQueryFilter();

        _logger.LogInformation("Complete the audit entity model configuration.");
    }
}
