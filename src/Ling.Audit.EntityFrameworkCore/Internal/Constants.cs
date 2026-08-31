using Microsoft.Extensions.Configuration;

namespace Ling.Audit.EntityFrameworkCore.Internal;

internal static class Constants
{
    /// <summary>
    /// The key for the <see cref="AuditOptions"/> configuration.
    /// It will be used to get the <see cref="AuditOptions"/> from <see cref="IConfiguration"/>'s section.
    /// </summary>
    public const string ConfigurationSection = "Audit";

    public const string AuditableAnnotationName = "Ling:Audit:Auditable";
    public const string MetadataAnnotationName = "Ling:Audit:Metadata";

    public const string Id = "Id";
    public const string CreatedAt = nameof(IHasCreationTime.CreatedAt);
    public const string CreatedBy = nameof(IHasCreator<int>.CreatedBy);
    public const string ModifiedAt = nameof(IHasModificationTime.LastModifiedAt);
    public const string ModifiedBy = nameof(IHasModifier<int>.LastModifiedBy);
    public const string IsDeleted = nameof(ISoftDelete.IsDeleted);
    public const string DeletedAt = nameof(IHasDeletionTime.DeletedAt);
    public const string DeletedBy = nameof(IHasDeleter<int>.DeletedBy);

    public static readonly IReadOnlyCollection<string> PropertyNames =
    [
        CreatedAt,
        CreatedBy,
        ModifiedAt,
        ModifiedBy,
        IsDeleted,
        DeletedAt,
        DeletedBy,
    ];
}
