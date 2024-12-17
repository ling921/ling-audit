namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks last modifier information.
/// </summary>
/// <typeparam name="TKey">The type of the last modifier identifier.</typeparam>
public interface IHasModifier<[MustNull] TKey>
{
    /// <summary>
    /// Gets or sets the identifier of the last modifier.
    /// </summary>
    TKey? LastModifiedBy { get; set; }
}
