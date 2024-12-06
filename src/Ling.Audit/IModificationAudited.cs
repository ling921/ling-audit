namespace Ling.Audit;

/// <summary>
/// Defines the modification audit information.
/// </summary>
/// <typeparam name="TKey">The type of the last modifier identifier.</typeparam>
public interface IModificationAudited<TKey> : IHasLastModifier<TKey>, IHasLastModificationTime;
