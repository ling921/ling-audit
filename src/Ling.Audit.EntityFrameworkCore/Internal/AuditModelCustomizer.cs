using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        modelBuilder.ApplyConfiguration(new AuditEntityLogTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AuditFieldLogTypeConfiguration());

        base.Customize(modelBuilder, context);

        var auditOptions = context.GetAuditOptions();

        modelBuilder.ConfigureAuditableEntities<TUserId>(auditOptions, _logger);

        modelBuilder.SetupSoftDeleteQueryFilter();

        _logger.LogInformation("Complete the audit entity model configuration.");
    }

    /// <summary>
    /// Configures the <see cref="AuditEntityChangeLog{TUserId}"/> entity type.
    /// </summary>
    private sealed class AuditEntityLogTypeConfiguration : IEntityTypeConfiguration<AuditEntityChangeLog<TUserId>>
    {
        /// <summary>
        /// Configures the <see cref="AuditEntityChangeLog{TUserId}"/> entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<AuditEntityChangeLog<TUserId>> builder)
        {
#if NET7_0_OR_GREATER
            builder.ToTable(nameof(AuditEntityChangeLog<TUserId>), t => t.HasComment("Table to record changes in entities."))
                   .HasKey(al => al.Id);
#else
        builder.ToTable(nameof(AuditEntityChangeLog<TUserId>))
               .HasComment("Table to record changes in entities.");

        builder.HasKey(al => al.Id);
#endif

            builder.Property(al => al.Id)
                   .ValueGeneratedOnAdd()
                   .HasComment("Primary key.");

            builder.Property(al => al.DatabaseSchema)
                   .IsUnicode(false)
                   .HasMaxLength(64)
                   .HasComment("Database schema name.");

            builder.Property(al => al.TableName)
                   .IsUnicode(false)
                   .HasMaxLength(128)
                   .HasComment("Table name.");

            builder.Property(al => al.EntityKey)
                   .IsUnicode(false)
                   .HasMaxLength(512)
                   .HasComment("Primary key of the changed entity.");

            builder.Property(al => al.EntityTypeName)
                   .IsUnicode(false)
                   .HasMaxLength(64)
                   .HasComment("Type of the changed entity.");

            builder.Property(al => al.EventType)
                   .IsUnicode(false)
                   .HasMaxLength(16)
                   .HasConversion<string>()
                   .HasComment("Type of the audit event.");

            builder.Property(al => al.EventTime)
                   .HasComment("Time the audit event occurred.");

            builder.Property(al => al.UserId)
                   .IsRequired(false)
                   .HasComment("Identity of the user who changed the entity.");

            builder.Property(al => al.UserName)
                   .IsUnicode(true)
                   .HasMaxLength(256)
                   .IsRequired(false)
                   .HasComment("Name of the user who changed the entity.");

            builder.HasMany(al => al.Details)
                   .WithOne()
                   .HasForeignKey(ald => ald.EntityLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Configures the <see cref="AuditFieldChangeLog"/> entity type.
    /// </summary>
    private sealed class AuditFieldLogTypeConfiguration : IEntityTypeConfiguration<AuditFieldChangeLog>
    {
        /// <summary>
        /// Configures the <see cref="AuditFieldChangeLog"/> entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<AuditFieldChangeLog> builder)
        {
#if NET7_0_OR_GREATER
            builder.ToTable(nameof(AuditFieldChangeLog), t => t.HasComment("Table to record changes in entity fields."))
                   .HasKey(al => al.Id);
#else
        builder.ToTable(nameof(AuditFieldChangeLog))
               .HasComment("Table to record changes in entity fields.");

        builder.HasKey(al => al.Id);
#endif

            builder.Property(al => al.Id)
                   .ValueGeneratedOnAdd()
                   .HasComment("Primary key.");

            builder.Property(al => al.EntityLogId)
                   .HasComment("Primary key of the associated entity change log.");

            builder.Property(al => al.FieldName)
                   .IsUnicode(false)
                   .HasMaxLength(256)
                   .HasComment("Name of the field (format: entity class name + '.' + field name).");

            builder.Property(al => al.ValueType)
                   .IsUnicode(false)
                   .HasMaxLength(256)
                   .HasComment("Type of the field value.");

            builder.Property(al => al.OriginalValue)
                   .IsUnicode(true)
                   .HasComment("Original value of the field.");

            builder.Property(al => al.NewValue)
                   .IsUnicode(true)
                   .HasComment("New value of the field.");
        }
    }
}
