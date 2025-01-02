namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Specifies that a property or field should be excluded from auditing in an audited entity.
/// <para>
/// This attribute is only effective when the containing entity is marked with <see cref="AuditIncludeAttribute"/>.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class AuditIgnoreAttribute : Attribute;
