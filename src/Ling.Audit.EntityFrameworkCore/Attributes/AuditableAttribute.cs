namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Marks an entity class as requiring audit logging.
/// By default, all mapped properties are audited unless marked with [NotAudited].
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the operations that are allowed to be performed anonymously (without user context).
    /// Defaults to <see cref="DataOperation.None"/>, requiring user context for all audited operations.
    /// </summary>
    public DataOperation AllowedAnonymous { get; set; }
}
