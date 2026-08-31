using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Option to configure the behavior of audited entities.
/// </summary>
public class AuditOptions
{
    /// <summary>
    /// Whether to allow anonymous operation of audit entities, default to <see langword="false"/>.
    /// <para>
    /// <see cref="IAuditAnonymousHandler.HandleAsync(Type, DataOperation, object?, CancellationToken)"/> will be invoke when anonymous operation occurred.
    /// </para>
    /// </summary>
    public bool AllowAnonymous { get; set; }

    /// <summary>
    /// Comments.
    /// </summary>
    public AuditEntityComments Comments { get; set; } = new();

    /// <summary>
    /// Whether to audit entities that has no field changes, default to <see langword="false"/>.
    /// </summary>
    public bool AuditNoFieldChangeEntity { get; set; }

    /// <summary>
    /// Gets or sets the transaction guarantee required when audit logs are persisted.
    /// </summary>
    public AuditTransactionMode TransactionMode { get; set; }

    /// <summary>
    /// Gets or sets <see cref="JsonSerializerOptions"/> for default <see cref="IPropertySerializer"/>.
    /// </summary>
    public JsonSerializerOptions? PropertySerializerOptions { get; set; }
}

/// <summary>
/// Comments to the audited entities.
/// </summary>
public class AuditEntityComments
{
    /// <summary>
    /// Comment to CreatedAt property.
    /// </summary>
    public string CreatedAt { get; set; } = "The date and time when created this entity.";

    /// <summary>
    /// Comment to CreatedBy property.
    /// </summary>
    public string CreatedBy { get; set; } = "The primary key of the user who created this entity.";

    /// <summary>
    /// Comment to ModifiedAt property.
    /// </summary>
    public string ModifiedAt { get; set; } = "The date and time when modified this entity.";

    /// <summary>
    /// Comment to ModifiedBy property.
    /// </summary>
    public string ModifiedBy { get; set; } = "The primary key of the user who modified this entity.";

    /// <summary>
    /// Comment to IsDeleted property.
    /// </summary>
    public string IsDeleted { get; set; } = "A flag indicating if the entity has mark as deleted instead of actually deleting it.";

    /// <summary>
    /// Comment to DeletedAt property.
    /// </summary>
    public string DeletedAt { get; set; } = "The date and time when marked this entity as deleted.";

    /// <summary>
    /// Comment to DeletedBy property.
    /// </summary>
    public string DeletedBy { get; set; } = "The primary key of the user who marked this entity as deleted.";
}
