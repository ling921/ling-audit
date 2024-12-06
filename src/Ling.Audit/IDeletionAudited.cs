namespace Ling.Audit;

/// <summary>
/// 完整的删除审计信息
/// </summary>
public interface IDeletionAudited<TKey> : ISoftDelete, IHasDeleter<TKey>, IHasDeletionTime;
