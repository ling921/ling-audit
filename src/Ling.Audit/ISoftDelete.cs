namespace Ling.Audit;

/// <summary>
/// Defines the soft delete property.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets a value indicating whether this object is deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
