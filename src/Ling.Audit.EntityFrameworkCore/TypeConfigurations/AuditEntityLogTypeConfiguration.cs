using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ling.Audit.EntityFrameworkCore.TypeConfigurations;

/// <summary>
/// Configures the <see cref="AuditEntityChangeLog{TUserId}"/> entity type.
/// </summary>
/// <typeparam name="TUserId">The type of the user ID.</typeparam>
internal sealed class AuditEntityLogTypeConfiguration<TUserId> : IEntityTypeConfiguration<AuditEntityChangeLog<TUserId>>
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
