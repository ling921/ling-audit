namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Represents the detail of <see cref="AuditEntityChangeLog{TUserId}"/>.
/// </summary>
public class AuditFieldChangeLog
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the primary key of <see cref="AuditEntityChangeLog{TUserId}"/>.
    /// </summary>
    public long EntityLogId { get; set; }

    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string FieldName { get; set; } = default!;

    public string ValueType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the original value.
    /// </summary>
    public string? OriginalValue { get; set; }

    /// <summary>
    /// Gets or sets the new value.
    /// </summary>
    public string? NewValue { get; set; }
}
