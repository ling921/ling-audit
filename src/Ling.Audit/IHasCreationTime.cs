namespace Ling.Audit;

/// <summary>
/// Defines the creation time property.
/// </summary>
public interface IHasCreationTime
{
    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }
}
