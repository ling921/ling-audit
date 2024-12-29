namespace Ling.Audit.EntityFrameworkCore.Internal;

internal static class Constants
{
    internal const string IncludeAnnotationName = "Ling:Audit:Include";
    internal const string MetadataAnnotationName = "Ling:Audit:Metadata";

    internal const string Id = "Id";
    internal const string CreatedAt = "CreatedAt";
    internal const string CreatedBy = "CreatedBy";
    internal const string ModifiedAt = "ModifiedAt";
    internal const string ModifiedBy = "ModifiedBy";
    internal const string IsDeleted = "IsDeleted";
    internal const string DeletedAt = "DeletedAt";
    internal const string DeletedBy = "DeletedBy";

    internal static readonly IReadOnlyCollection<string> PropertyNames = new[]
    {
        Id,
        CreatedAt,
        CreatedBy,
        ModifiedAt,
        ModifiedBy,
        IsDeleted,
        DeletedAt,
        DeletedBy,
    };
}
