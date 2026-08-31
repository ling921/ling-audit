namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Handles an operation for which no audit user could be resolved.
/// </summary>
public interface IAuditAnonymousHandler
{
    /// <summary>
    /// Handles an anonymous audit operation for the specified entity.
    /// </summary>
    Task HandleAsync(
        Type entityType,
        DataOperation operation,
        object? entity,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Rejects anonymous operations unless they were explicitly allowed.
/// </summary>
internal sealed class DefaultAuditAnonymousHandler : IAuditAnonymousHandler
{
    public Task HandleAsync(
        Type entityType,
        DataOperation operation,
        object? entity,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            $"Anonymous {operation} operation is not allowed for entity type '{entityType.FullName}'.");
    }
}
