namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks deletion time.
/// </summary>
public interface IHasDeletionTime
{
    /// <summary>
    /// Gets or sets the deletion time.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }
}
