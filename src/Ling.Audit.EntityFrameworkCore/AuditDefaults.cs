using Microsoft.Extensions.Configuration;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Define the audit configuration items.
/// </summary>
public static class AuditDefaults
{
    /// <summary>
    /// The switch to control whether auditing is disabled.
    /// <para>
    /// To disable auditing:
    /// <code>
    /// AppContext.SetSwitch("Ling.Audit.EntityFrameworkCore.DisableAuditing", true);
    /// </code>
    /// </para>
    /// </summary>
    public const string DisableAuditingSwitch = "Ling.Audit.EntityFrameworkCore.DisableAuditing";
}
