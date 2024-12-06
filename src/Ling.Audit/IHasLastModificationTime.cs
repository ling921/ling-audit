namespace Ling.Audit;

/// <summary>
/// Defines the last modification time property.
/// </summary>
public interface IHasLastModificationTime
{
    /// <summary>
    /// Gets or sets the last modification time.
    /// </summary>
    DateTimeOffset? LastModifiedAt { get; set; }
}
