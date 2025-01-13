using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// 
/// </summary>
public interface IAuditLogRepository<[MustNull] TUserId>
{
    Task<AuditEntityChangeLog<TUserId>?> GetByIdAsync(long auditEntityChangeLogId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EntityHistory<TEntity, TUserId>>> GetHistoryAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class;
}

internal class AuditLogRepository<TUserId> : IAuditLogRepository<TUserId>
{
    private readonly DbContext _dbContext;

    public AuditLogRepository(ICurrentDbContext currentDbContext)
    {
        _dbContext = currentDbContext.Context;
    }

    public async Task<AuditEntityChangeLog<TUserId>?> GetByIdAsync(long auditEntityChangeLogId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<AuditEntityChangeLog<TUserId>>()
            .AsNoTracking()
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == auditEntityChangeLogId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityHistory<TEntity, TUserId>>> GetHistoryAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var entry = _dbContext.Attach(entity);

        //var entityType = entity.GetType();
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType is null)
        {
            throw new InvalidOperationException($"The entity type {typeof(TEntity).Name} is not found in the model.");
        }

        var xxx = entityType.GetProperties();
        var yyy = entityType.GetDeclaredProperties();
        var zzz = entityType.GetDerivedProperties();
        var entityName = entityType.DisplayName();
        var key = entityType.FindPrimaryKey();
        if (key is null)
        {
            throw new InvalidOperationException($"The entity type {typeof(TEntity).Name} does not have a primary key.");
        }

        var entityKey = key.GetPrimaryKey(entity);

        var auditEntityChangeLogs = await _dbContext.Set<AuditEntityChangeLog<TUserId>>()
            .AsNoTracking()
            .Where(x => x.EntityTypeName == entityName && x.EntityKey == entityKey)
            .OrderByDescending(x => x.EventTime)
            .ToListAsync(cancellationToken);
        return auditEntityChangeLogs.Select(x => new EntityHistory<TEntity, TUserId>(
            x,
            entity,
            entity)).ToList();
    }
}

public sealed record EntityHistory<TEntity, [MustNull] TUserId>(
    TEntity? Original,
    TEntity? New,
    AuditEventType EventType,
    DateTimeOffset EventTime,
    TUserId? UserId,
    string? UserName,
    string? IPAddress,
    string? ClientName)
    where TEntity : class
{
    public EntityHistory(AuditEntityChangeLog<TUserId> changeLog, TEntity? Original, TEntity? New)
        : this(
            Original,
            New,
            changeLog.EventType,
            changeLog.EventTime,
            changeLog.UserId,
            changeLog.UserName,
            changeLog.IPAddress,
            changeLog.ClientName)
    {
    }
}
