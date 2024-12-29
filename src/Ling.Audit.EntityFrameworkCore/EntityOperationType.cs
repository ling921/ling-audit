namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Enum that specifies the type of operation performed on an entity.
/// </summary>
[Flags]
public enum EntityOperationType : byte
{
    /// <summary>
    /// No operation.
    /// </summary>
    None = 0,

    /// <summary>
    /// Entity creation operation.
    /// </summary>
    Create = 1 << 1,

    /// <summary>
    /// Entity update operation.
    /// </summary>
    Update = 1 << 2,

    /// <summary>
    /// Entity deletion operation.
    /// </summary>
    Delete = 1 << 3,

    /// <summary>
    /// Combination of Create, Update, and Delete operations.
    /// </summary>
    All = Create | Update | Delete,
}
