namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Provides timestamps for audit operations.
/// </summary>
public interface IAuditTimeProvider
{
    /// <summary>
    /// Gets the current time in UTC.
    /// </summary>
    DateTimeOffset GetUtcNow();
}

internal sealed class SystemAuditTimeProvider : IAuditTimeProvider
{
    public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
}
