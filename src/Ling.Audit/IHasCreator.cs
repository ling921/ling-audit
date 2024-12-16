namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks creator information.
/// </summary>
/// <typeparam name="TKey">The type of the creator identifier.</typeparam>
public interface IHasCreator<TKey>;

/// <summary>
/// Defines an object that tracks creator information.
/// <para>
/// This interface is intended for source generation or pattern matching only.
/// For normal inheritance, use <see cref="IHasCreator{TKey}"/> instead.
/// </para>
/// </summary>
/// <typeparam name="TKey">The type of the creator identifier.</typeparam>
public interface IHasCreationUser<TKey>
{
    /// <summary>
    /// Gets or sets the identifier of the creator.
    /// </summary>
    TKey? CreatedBy { get; set; }
}
