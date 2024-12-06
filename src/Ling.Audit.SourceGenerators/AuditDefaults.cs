namespace Ling.Audit.SourceGenerators;

internal static class AuditDefaults
{
    public static string Version = typeof(AuditDefaults).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

    public const string CreatedBy = "CreatedBy";
    public const string CreatedAt = "CreatedAt";
    public const string ModifiedBy = "LastModifiedBy";
    public const string ModifiedAt = "LastModifiedAt";
    public const string IsDeleted = "IsDeleted";
    public const string DeletedBy = "DeletedBy";
    public const string DeletedAt = "DeletedAt";

    public const string CreatedByTypeDef = "Ling.Audit.IHasCreator<TKey>";
    public const string CreatedAtTypeDef = "Ling.Audit.IHasCreationTime";
    public const string ModifiedByTypeDef = "Ling.Audit.IHasLastModifier<TKey>";
    public const string ModifiedAtTypeDef = "Ling.Audit.IHasLastModificationTime";
    public const string SoftDeleteTypeDef = "Ling.Audit.ISoftDelete";
    public const string DeletedByTypeDef = "Ling.Audit.IHasDeleter<TKey>";
    public const string DeletedAtTypeDef = "Ling.Audit.IHasDeletionTime";
}
