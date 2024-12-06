namespace Ling.Audit;

/// <summary>
/// Defines the creator property.
/// </summary>
/// <typeparam name="TKey">The type of the creator identifier.</typeparam>
public interface IHasCreator<TKey>
{
    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    TKey? CreatedBy { get; set; }
}
