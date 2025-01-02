namespace Ling.Audit.EntityFrameworkCore.Internal;

internal static class Constants
{
    internal const string AuditableAnnotationName = "Ling:Audit:Auditable";
    internal const string MetadataAnnotationName = "Ling:Audit:Metadata";

    internal const string Id = "Id";
    internal const string CreatedAt = nameof(IHasCreationTime.CreatedAt);
    internal const string CreatedBy = nameof(IHasCreator<int>.CreatedBy);
    internal const string ModifiedAt = nameof(IHasModificationTime.LastModifiedAt);
    internal const string ModifiedBy = nameof(IHasModifier<int>.LastModifiedBy);
    internal const string IsDeleted = nameof(ISoftDelete.IsDeleted);
    internal const string DeletedAt = nameof(IHasDeletionTime.DeletedAt);
    internal const string DeletedBy = nameof(IHasDeleter<int>.DeletedBy);

    internal static readonly IReadOnlyCollection<string> PropertyNames =
    [
        Id,
        CreatedAt,
        CreatedBy,
        ModifiedAt,
        ModifiedBy,
        IsDeleted,
        DeletedAt,
        DeletedBy,
    ];
}
