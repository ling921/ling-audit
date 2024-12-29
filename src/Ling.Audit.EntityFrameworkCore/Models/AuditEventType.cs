namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Enum that specifies audit event type.
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// No event.
    /// </summary>
    None,

    /// <summary>
    /// Event for creating an entity.
    /// </summary>
    Create,

    /// <summary>
    /// Event for modifying an entity.
    /// </summary>
    Modify,

    /// <summary>
    /// Event for deleting an entity.
    /// </summary>
    Delete,

    /// <summary>
    /// Event for soft deleting an entity.
    /// </summary>
    SoftDelete,

    /// <summary>
    /// Event for recovering a soft-deleted entity.
    /// </summary>
    Recovery,
}
