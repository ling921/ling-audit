namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks creation time.
/// </summary>
public interface IHasCreationTime
{
    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }
}
