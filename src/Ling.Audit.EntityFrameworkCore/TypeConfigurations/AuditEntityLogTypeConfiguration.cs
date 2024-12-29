using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ling.Audit.EntityFrameworkCore.TypeConfigurations;

internal sealed class AuditEntityLogTypeConfiguration<TUserId> : IEntityTypeConfiguration<AuditEntityChangeLog<TUserId>>
{
    public void Configure(EntityTypeBuilder<AuditEntityChangeLog<TUserId>> builder)
    {
#if NET7_0_OR_GREATER
        builder.ToTable(AuditDefaults.EntityChangeAuditLogTableName, t => t.HasComment("A table to record entities changes."))
               .HasKey(al => al.Id);
#else
        builder.ToTable(AuditDefaults.EntityChangeAuditLogTableName)
               .HasComment("A table to record entities changes.");

        builder.HasKey(al => al.Id);
#endif

        builder.Property(al => al.Id)
               .ValueGeneratedOnAdd()
               .HasComment("The primary key.");

        builder.Property(al => al.DatabaseSchema)
               .IsUnicode(false)
               .HasMaxLength(64)
               .HasComment("The database schema name.");

        builder.Property(al => al.TableName)
               .IsUnicode(false)
               .HasMaxLength(128)
               .HasComment("The table name.");

        builder.Property(al => al.EntityKey)
               .IsUnicode(false)
               .HasMaxLength(128)
               .HasComment("The primary key of changed entity.");

        builder.Property(al => al.EntityTypeName)
               .IsUnicode(false)
               .HasMaxLength(64)
               .HasComment("The type of changed entity.");

        builder.Property(al => al.EventType)
               .IsUnicode(false)
               .HasMaxLength(16)
               .HasConversion<string>()
               .HasComment("The type of audit event.");

        builder.Property(al => al.EventTime)
               .HasComment("The time the audit event occurred.");

        builder.Property(al => al.UserId)
               .IsRequired(false)
               .HasComment("The identity of the user who change entity.");

        builder.Property(al => al.UserName)
               .IsUnicode(true)
               .HasMaxLength(512)
               .IsRequired(false)
               .HasComment("The name of the user who change entity.");

        builder.HasMany(al => al.Details)
               .WithOne()
               .HasForeignKey(ald => ald.EntityLogId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
