namespace Ling.Audit;

/// <summary>
/// 完整的创建审计信息
/// </summary>
public interface ICreationAudited<TKey> : IHasCreator<TKey>, IHasCreationTime;
