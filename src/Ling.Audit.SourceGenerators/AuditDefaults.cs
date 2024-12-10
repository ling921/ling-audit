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

    public const string IHasCreatorTypeFullQualifiedName = $"{Namespace}.IHasCreator`1";
    public const string IHasCreationTimeTypeFullQualifiedName = $"{Namespace}.IHasCreationTime";
    public const string IHasLastModifierTypeFullQualifiedName = $"{Namespace}.IHasLastModifier`1";
    public const string IHasLastModificationTimeTypeFullQualifiedName = $"{Namespace}.IHasLastModificationTime";
    public const string ISoftDeleteTypeFullQualifiedName = $"{Namespace}.ISoftDelete";
    public const string IHasDeleterTypeFullQualifiedName = $"{Namespace}.IHasDeleter`1";
    public const string IHasDeletionTimeTypeFullQualifiedName = $"{Namespace}.IHasDeletionTime";

    public const string ICreationAuditedTypeFullQualifiedName = $"{Namespace}.ICreationAudited`1";
    public const string IModificationAuditedTypeFullQualifiedName = $"{Namespace}.IModificationAudited`1";
    public const string IDeletionAuditedTypeFullQualifiedName = $"{Namespace}.IDeletionAudited`1";
    public const string IFullAuditedTypeFullQualifiedName = $"{Namespace}.IFullAudited`1";
}
