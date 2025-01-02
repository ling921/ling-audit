namespace Ling.Audit.EntityFrameworkCore.Internal.Models;

/// <summary>
/// Represents temporary tracking information for a property change during auditing.
/// </summary>
internal sealed class AuditPropertyEntry
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the original value before change.
    /// </summary>
    public object? OriginalValue { get; set; }

    /// <summary>
    /// Gets or sets the new value after change.
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Gets or sets the type of the property value.
    /// </summary>
    public string ValueType { get; set; } = default!;
}
