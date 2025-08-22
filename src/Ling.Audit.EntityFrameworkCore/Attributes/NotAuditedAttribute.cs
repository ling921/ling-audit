namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Marks an entity property to be excluded from audit logging,
/// even if the containing entity is marked as [Auditable].
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class NotAuditedAttribute : Attribute;
