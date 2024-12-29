using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Represents the audit log for entities.
/// </summary>
public class AuditEntityChangeLog<TUserId>
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the schema of database.
    /// </summary>
    public string? DatabaseSchema { get; set; }

    /// <summary>
    /// Gets or sets the table of data changed.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the <see cref="EntityEntry"/>.
    /// </summary>
    public string EntityKey { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of changed entity.
    /// </summary>
    public string EntityTypeName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of audit event.
    /// </summary>
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the time the audit event occurred.
    /// </summary>
    public DateTimeOffset EventTime { get; set; }

    /// <summary>
    /// Gets or sets the identity of user who change entity.
    /// </summary>
    public TUserId? UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of user who change entity.
    /// </summary>
    public string? UserName { get; set; }

    public string? IPAddress { get; set; }

    public string? ClientName { get; set; }

    /// <summary>
    /// Gets or sets the details of this audit log.
    /// </summary>
    public virtual ICollection<AuditFieldChangeLog> Details { get; set; } = new List<AuditFieldChangeLog>();
}
