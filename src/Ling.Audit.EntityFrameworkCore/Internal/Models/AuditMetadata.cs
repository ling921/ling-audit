namespace Ling.Audit.EntityFrameworkCore.Internal.Models;

internal sealed class AuditMetadata
{
    public EntityOperationType AllowAnonymousOperate { get; set; }
    public Type? UserIdType { get; set; }
    public bool HasCreatedAt { get; set; }
    public bool HasCreatedBy { get; set; }
    public bool HasModifiedAt { get; set; }
    public bool HasModifiedBy { get; set; }
    public bool HasIsDeleted { get; set; }
    public bool HasDeletedAt { get; set; }
    public bool HasDeletedBy { get; set; }

    public void Append(AuditMetadata other)
    {
        if (!ReferenceEquals(this, other))
        {
            AllowAnonymousOperate |= other.AllowAnonymousOperate;
            UserIdType ??= other.UserIdType;
            HasCreatedAt = HasCreatedAt || other.HasCreatedAt;
            HasCreatedBy = HasCreatedBy || other.HasCreatedBy;
            HasModifiedAt = HasModifiedAt || other.HasModifiedAt;
            HasModifiedBy = HasModifiedBy || other.HasModifiedBy;
            HasIsDeleted = HasIsDeleted || other.HasIsDeleted;
            HasDeletedAt = HasDeletedAt || other.HasDeletedAt;
            HasDeletedBy = HasDeletedBy || other.HasDeletedBy;
        }
    }

    public bool IsAllowedAnonymousOperate(EntityOperationType operate)
    {
        return AllowAnonymousOperate.HasFlag(operate);
    }
}
