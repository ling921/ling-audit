namespace Ling.Audit.EntityFrameworkCore.Internal.Models;

/// <summary>
/// Represents audit metadata for an entity type.
/// </summary>
internal sealed class AuditMetadata
{
    /// <summary>
    /// Gets or sets the operations that can be performed anonymously.
    /// </summary>
    public EntityOperationType AnonymousOperations { get; set; }

    /// <summary>
    /// Gets or sets the type of the user ID.
    /// </summary>
    public Type? UserIdType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has a CreatedAt property.
    /// </summary>
    public bool HasCreatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has a CreatedBy property.
    /// </summary>
    public bool HasCreatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has a ModifiedAt property.
    /// </summary>
    public bool HasModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has a ModifiedBy property.
    /// </summary>
    public bool HasModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has an IsDeleted property.
    /// </summary>
    public bool HasIsDeleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has a DeletedAt property.
    /// </summary>
    public bool HasDeletedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has a DeletedBy property.
    /// </summary>
    public bool HasDeletedBy { get; set; }

    /// <summary>
    /// Determines whether the specified operation can be performed anonymously.
    /// </summary>
    /// <param name="operate">The operation to check.</param>
    /// <returns><see langword="true"/> if the operation can be performed anonymously; otherwise, <see langword="false"/>.</returns>
    public bool AllowsAnonymousOperation(EntityOperationType operate) => AnonymousOperations.HasFlag(operate);

    /// <summary>
    /// Combines two <see cref="AuditMetadata"/> instances using the bitwise OR operator.
    /// </summary>
    public static AuditMetadata operator |(AuditMetadata left, AuditMetadata right) => new()
    {
        AnonymousOperations = left.AnonymousOperations | right.AnonymousOperations,
        UserIdType = left.UserIdType ?? right.UserIdType,
        HasCreatedAt = left.HasCreatedAt || right.HasCreatedAt,
        HasCreatedBy = left.HasCreatedBy || right.HasCreatedBy,
        HasModifiedAt = left.HasModifiedAt || right.HasModifiedAt,
        HasModifiedBy = left.HasModifiedBy || right.HasModifiedBy,
        HasIsDeleted = left.HasIsDeleted || right.HasIsDeleted,
        HasDeletedAt = left.HasDeletedAt || right.HasDeletedAt,
        HasDeletedBy = left.HasDeletedBy || right.HasDeletedBy
    };
}
