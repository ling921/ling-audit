namespace Ling.Audit;

/// <summary>
/// Defines the last modifier property.
/// </summary>
/// <typeparam name="TKey">The type of the last modifier identifier.</typeparam>
public interface IHasLastModifier<TKey>
{
    /// <summary>
    /// Gets or sets the last modifier identifier.
    /// </summary>
    TKey? LastModifiedBy { get; set; }
}
