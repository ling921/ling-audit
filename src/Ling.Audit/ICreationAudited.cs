namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks creation time and creator information.
/// </summary>
/// <typeparam name="TKey">The type of the creator identifier.</typeparam>
public interface ICreationAudited<TKey> : IHasCreator<TKey>, IHasCreationTime;
