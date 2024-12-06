namespace Ling.Audit;

/// <summary>
/// Defines the deletion time property.
/// </summary>
public interface IHasDeletionTime
{
    /// <summary>
    /// Gets or sets the deletion time.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }
}
