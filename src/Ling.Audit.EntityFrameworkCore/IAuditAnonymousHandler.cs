using Microsoft.Extensions.Logging;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Interface for handling anonymous audit operations.
/// </summary>
public interface IAuditAnonymousHandler
{
    /// <summary>
    /// Handles anonymous audit operation for the specified entity.
    /// </summary>
    /// <param name="entityType">The type of the entity being audited.</param>
    /// <param name="operation">The data operation being performed.</param>
    /// <param name="entity">The entity instance being audited.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(
        Type entityType,
        DataOperation operation,
        object? entity,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IAuditAnonymousHandler"/> that logs anonymous operations.
/// </summary>
internal sealed class DefaultAuditAnonymousHandler(
    ILogger<DefaultAuditAnonymousHandler> logger)
    : IAuditAnonymousHandler
{
    /// <inheritdoc/>
    public Task HandleAsync(
        Type entityType,
        DataOperation operation,
        object? entity,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Anonymous {Operation} operation detected on entity type {EntityType}. Entity: {@Entity}",
            operation,
            entityType.FullName,
            entity);

        return Task.CompletedTask;
    }
}
