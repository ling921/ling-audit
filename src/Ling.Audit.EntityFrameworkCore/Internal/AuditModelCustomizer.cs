using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Ling.Audit.EntityFrameworkCore.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Ling.Audit.EntityFrameworkCore.Internal;

/// <summary>
/// Customizes the model building process to configure audit-related entities and behaviors.
/// </summary>
/// <typeparam name="TUserId">The type of the user ID used in audit entities.</typeparam>
internal sealed class AuditModelCustomizer<TUserId> : ModelCustomizer
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditModelCustomizer{TUserId}"/> class.
    /// </summary>
    /// <param name="dependencies">The dependencies needed for model customization.</param>
    /// <param name="loggerFactory">The factory to create loggers.</param>
    public AuditModelCustomizer(
        ModelCustomizerDependencies dependencies,
        ILoggerFactory loggerFactory) : base(dependencies)
    {
        _logger = loggerFactory.CreateLogger<AuditModelCustomizer<TUserId>>();
    }

    /// <inheritdoc/>
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.ApplyConfiguration(new AuditEntityLogTypeConfiguration<TUserId?>());
        modelBuilder.ApplyConfiguration(new AuditFieldLogTypeConfiguration());

        base.Customize(modelBuilder, context);

        var auditOptions = context.GetAuditOptions();

        modelBuilder.ConfigureAuditableEntities(auditOptions, _logger);

        modelBuilder.SetupSoftDeleteQueryFilter();

        _logger.LogInformation("Complete the audit entity model configuration.");
    }
}
