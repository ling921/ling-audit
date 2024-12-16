namespace Ling.Audit;

/// <summary>
/// Defines an object that tracks last modification time and last modifier information.
/// </summary>
/// <typeparam name="TKey">The type of the last modifier identifier.</typeparam>
public interface IModificationAudited<TKey> : IHasModifier<TKey>, IHasModificationTime;
