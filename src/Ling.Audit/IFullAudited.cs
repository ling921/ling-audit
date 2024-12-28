namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks creation time, creator, last modification time, last modifier, and deletion time and deleter information.
/// </summary>
/// <typeparam name="TKey">The type of the user identifier.</typeparam>
public interface IFullAudited<TKey> : ICreationAudited<TKey>, IModificationAudited<TKey>, IDeletionAudited<TKey>;
