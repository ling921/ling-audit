namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Specifies fundamental data manipulation operations, typically corresponding to CUD actions.
/// Can be combined using bitwise flags.
/// </summary>
[Flags]
public enum DataOperation : byte
{
    /// <summary>
    /// Represents no specific data operation.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents the creation of new data (e.g., SQL INSERT).
    /// </summary>
    Create = 1 << 0,

    /// <summary>
    /// Represents the modification of existing data (e.g., SQL UPDATE).
    /// </summary>
    Modify = 1 << 1,

    /// <summary>
    /// Represents the deletion of existing data (e.g., SQL DELETE).
    /// </summary>
    Delete = 1 << 2,

    /// <summary>
    /// Represents all primary data writing operations (Create, Modify, Delete).
    /// </summary>
    All = Create | Modify | Delete
}
