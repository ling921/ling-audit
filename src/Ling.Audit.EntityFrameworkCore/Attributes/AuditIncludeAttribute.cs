namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Specifies that an entity class should be included in automatic auditing.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuditIncludeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditIncludeAttribute"/> class with specified anonymous operations.
    /// </summary>
    /// <param name="anonymousOperations">The operations that can be performed without authentication.</param>
    public AuditIncludeAttribute(EntityOperationType anonymousOperations = EntityOperationType.None)
    {
        AnonymousOperations = anonymousOperations;
    }

    /// <summary>
    /// Gets the operations that can be performed without authentication.
    /// </summary>
    public EntityOperationType AnonymousOperations { get; }
}
