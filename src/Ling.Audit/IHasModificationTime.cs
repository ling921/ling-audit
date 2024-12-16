namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks last modification time.
/// </summary>
public interface IHasModificationTime
{
    /// <summary>
    /// Gets or sets the last modification time.
    /// </summary>
    DateTimeOffset? LastModifiedAt { get; set; }
}
