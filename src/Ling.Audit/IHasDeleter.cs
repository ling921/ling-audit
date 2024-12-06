namespace Ling.Audit;

/// <summary>
/// Defines the deleter property.
/// </summary>
/// <typeparam name="TKey">The type of the deleter identifier.</typeparam>
public interface IHasDeleter<TKey>
{
    /// <summary>
    /// Gets or sets the deleter identifier.
    /// </summary>
    TKey? DeletedBy { get; set; }
}
