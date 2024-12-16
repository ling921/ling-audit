namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks deleter information.
/// </summary>
/// <typeparam name="TKey">The type of the deleter identifier.</typeparam>
public interface IHasDeleter<TKey>;

/// <summary>
/// Defines an object that tracks deleter information.
/// </summary>
/// <typeparam name="TKey">The type of the deleter identifier.</typeparam>
public interface IHasDeletionUser<TKey>
{
    /// <summary>
    /// Gets or sets the identifier of the deleter.
    /// </summary>
    TKey DeletedBy { get; set; }
}
