namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks soft delete information.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether this object is deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
