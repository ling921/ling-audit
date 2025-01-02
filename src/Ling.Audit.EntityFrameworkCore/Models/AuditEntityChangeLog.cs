using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Represents the audit log for entities.
/// </summary>
/// <typeparam name="TUserId">The type of the user ID.</typeparam>
public class AuditEntityChangeLog<TUserId>
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the schema of the database.
    /// </summary>
    public string? DatabaseSchema { get; set; }

    /// <summary>
    /// Gets or sets the table name of the data changed.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the <see cref="EntityEntry"/>.
    /// </summary>
    public string EntityKey { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type name of the changed entity.
    /// </summary>
    public string EntityTypeName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of the audit event.
    /// </summary>
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the time the audit event occurred.
    /// </summary>
    public DateTimeOffset EventTime { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user who changed the entity.
    /// </summary>
    public TUserId? UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who changed the entity.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the client that made the change.
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// Gets or sets the name of the client that made the change.
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// Gets or sets the details of this audit log.
    /// </summary>
    public virtual ICollection<AuditFieldChangeLog> Details { get; set; } = new List<AuditFieldChangeLog>();
}
