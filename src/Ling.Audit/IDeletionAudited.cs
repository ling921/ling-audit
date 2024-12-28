namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks deletion time and deleter information.
/// </summary>
/// <typeparam name="TKey">The type of the deleter identifier.</typeparam>
public interface IDeletionAudited<TKey> : ISoftDelete, IHasDeleter<TKey>, IHasDeletionTime;
