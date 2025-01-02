using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ling.Audit.EntityFrameworkCore.Internal.Models;

/// <summary>
/// Represents an entity entry for auditing purposes, containing temporary tracking information
/// before and during the save operation.
/// </summary>
internal sealed class AuditEntityEntry
{
    /// <summary>
    /// Gets the original entity entry.
    /// </summary>
    public EntityEntry EntityEntry { get; }

    /// <summary>
    /// Gets the database schema name.
    /// </summary>
    public string? Schema { get; }

    /// <summary>
    /// Gets the database table name.
    /// </summary>
    public string? Table { get; }

    /// <summary>
    /// Gets the primary key value of the entity.
    /// </summary>
    public string PrimaryKey => EntityEntry.GetPrimaryKey();

    /// <summary>
    /// Gets or sets the entity type name.
    /// </summary>
    public string EntityType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of change event.
    /// </summary>
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the collection of property changes.
    /// </summary>
    public List<AuditPropertyEntry> Properties { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditEntityEntry"/> class.
    /// </summary>
    /// <param name="entry">The entity entry being audited.</param>
    public AuditEntityEntry(EntityEntry entry)
    {
        EntityEntry = entry;
        Schema = entry.Metadata.GetSchema();
        Table = entry.Metadata.GetTableName();
        EntityType = entry.Metadata.DisplayName();
    }
}
