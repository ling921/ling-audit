namespace Ling.Audit;

/// <summary>
/// Defines the full audit information.
/// </summary>
/// <typeparam name="TKey">The type of the user identifier.</typeparam>
public interface IFullAudited<TKey> : ICreationAudited<TKey>, IModificationAudited<TKey>, IDeletionAudited<TKey>;
