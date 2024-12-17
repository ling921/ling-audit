namespace Ling.Audit.SourceGenerators;

internal static class AuditDefaults
{
    public const string Namespace = "Ling.Audit";

    public static string Version = typeof(AuditDefaults).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

    public const string CreatedBy = "CreatedBy";
    public const string CreatedAt = "CreatedAt";
    public const string ModifiedBy = "LastModifiedBy";
    public const string ModifiedAt = "LastModifiedAt";
    public const string IsDeleted = "IsDeleted";
    public const string DeletedBy = "DeletedBy";
    public const string DeletedAt = "DeletedAt";

    public const string MustNullableAttributeFullQualifiedMetadataName = $"{Namespace}.MustNullableAttribute";

    public const string IHasCreatorTypeFullQualifiedMetadataName = $"{Namespace}.IHasCreator`1";
    public const string IHasCreationTimeTypeFullQualifiedMetadataName = $"{Namespace}.IHasCreationTime";
    public const string IHasModifierTypeFullQualifiedMetadataName = $"{Namespace}.IHasModifier`1";
    public const string IHasModificationTimeTypeFullQualifiedMetadataName = $"{Namespace}.IHasModificationTime";
    public const string ISoftDeleteTypeFullQualifiedMetadataName = $"{Namespace}.ISoftDelete";
    public const string IHasDeleterTypeFullQualifiedMetadataName = $"{Namespace}.IHasDeleter`1";
    public const string IHasDeletionTimeTypeFullQualifiedMetadataName = $"{Namespace}.IHasDeletionTime";

    public const string ICreationAuditedTypeFullQualifiedMetadataName = $"{Namespace}.ICreationAudited`1";
    public const string IModificationAuditedTypeFullQualifiedMetadataName = $"{Namespace}.IModificationAudited`1";
    public const string IDeletionAuditedTypeFullQualifiedMetadataName = $"{Namespace}.IDeletionAudited`1";
    public const string IFullAuditedTypeFullQualifiedMetadataName = $"{Namespace}.IFullAudited`1";
}
