namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks creator information.
/// </summary>
/// <typeparam name="TKey">The type of the creator identifier.</typeparam>
public interface IHasCreator<TKey>;

/// <summary>
/// Defines an object that tracks creator information.
/// </summary>
/// <typeparam name="TKey">The type of the creator identifier.</typeparam>
public interface IHasCreationUser<TKey>
{
    /// <summary>
    /// Gets or sets the identifier of the creator.
    /// </summary>
    TKey CreatedBy { get; set; }
}
