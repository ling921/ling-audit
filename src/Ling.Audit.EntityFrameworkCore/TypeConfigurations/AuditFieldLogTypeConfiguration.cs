using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ling.Audit.EntityFrameworkCore.TypeConfigurations;

/// <summary>
/// Configures the <see cref="AuditFieldChangeLog"/> entity type.
/// </summary>
internal sealed class AuditFieldLogTypeConfiguration : IEntityTypeConfiguration<AuditFieldChangeLog>
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
