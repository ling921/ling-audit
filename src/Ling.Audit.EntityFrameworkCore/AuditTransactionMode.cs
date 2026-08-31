namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Specifies the transaction guarantee used when audit logs are persisted.
/// </summary>
public enum AuditTransactionMode
{
    /// <summary>
    /// Writes audit logs after the audited save. This preserves compatibility but may use a separate transaction.
    /// </summary>
    BestEffort,

    /// <summary>
    /// Requires the caller to start a transaction so the audited save and audit logs share that transaction.
    /// </summary>
    RequireExplicitTransaction,
}
