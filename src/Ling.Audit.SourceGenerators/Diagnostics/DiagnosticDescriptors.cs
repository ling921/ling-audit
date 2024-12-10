using Ling.Audit.SourceGenerators.Resources;
using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators.Diagnostics;

internal static class DiagnosticDescriptors
{
    private const string Category = "Design";

    private static LocalizableResourceString L(string name) => new(name, SR.ResourceManager, typeof(SR));

    #region Diagnostic IDs

    /// <summary>
    /// Diagnostic ID for <see cref="ValueType"/>: audit not supported on value types
    /// </summary>
    public const string ValueTypeId = "LA001";

    /// <summary>
    /// Diagnostic ID for <see cref="PartialType"/>: type must be declared as partial
    /// </summary>
    public const string PartialTypeId = "LA002";

    /// <summary>
    /// Diagnostic ID for <see cref="KeyTypeMismatch"/>: key type mismatch in audit interfaces
    /// </summary>
    public const string KeyTypeMismatchId = "LA003";

    /// <summary>
    /// Diagnostic ID for <see cref="UseCreationAudited"/>: suggesting to use ICreationAudited interface
    /// </summary>
    public const string UseCreationAuditedId = "LA901";

    /// <summary>
    /// Diagnostic ID for <see cref="UseModificationAudited"/>: suggesting to use
    /// IModificationAudited interface
    /// </summary>
    public const string UseModificationAuditedId = "LA902";

    /// <summary>
    /// Diagnostic ID for <see cref="UseDeletionAudited"/>: suggesting to use IDeletionAudited interface
    /// </summary>
    public const string UseDeletionAuditedId = "LA903";

    /// <summary>
    /// Diagnostic ID for <see cref="UseFullAudited"/>: suggesting to use IFullAudited interface
    /// </summary>
    public const string UseFullAuditedId = "LA904";

    #endregion Diagnostic IDs

    #region Rules

    /// <summary>
    /// Diagnostic rule for audit not supported on value types.
    /// <para>Message format: "Type '{0}' is a value type and cannot be audited."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor ValueType = new(
        ValueTypeId,
        L(nameof(SR.AuditNotSupportedForValueType_Title)),
        L(nameof(SR.AuditNotSupportedForValueType_Message)),
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: L(nameof(SR.AuditNotSupportedForValueType_Description)));

    /// <summary>
    /// Diagnostic rule for type must be declared as partial.
    /// <para>Message format: "Type '{0}' must be declared as partial to support auditing."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor PartialType = new(
        PartialTypeId,
        L(nameof(SR.AuditRequiresPartialType_Title)),
        L(nameof(SR.AuditRequiresPartialType_Message)),
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: L(nameof(SR.AuditRequiresPartialType_Description)));

    /// <summary>
    /// Diagnostic rule for key type mismatch in audit interfaces.
    /// <para>Message format: "Type '{0}' has multiple key types in audit interfaces: {1}."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor KeyTypeMismatch = new(
        KeyTypeMismatchId,
        L(nameof(SR.AuditKeyTypeMismatch_Title)),
        L(nameof(SR.AuditKeyTypeMismatch_Message)),
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: L(nameof(SR.AuditKeyTypeMismatch_Description)));

    /// <summary>
    /// Diagnostic rule for suggesting to use ICreationAudited interface.
    /// <para>Message format: "Consider using ICreationAudited interface instead."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor UseCreationAudited = new(
        UseCreationAuditedId,
        L(nameof(SR.UseCreationAudited_Title)),
        L(nameof(SR.UseCreationAudited_Message)),
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Diagnostic rule for suggesting to use IModificationAudited interface.
    /// <para>Message format: "Consider using IModificationAudited interface instead."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor UseModificationAudited = new(
        UseModificationAuditedId,
        L(nameof(SR.UseModificationAudited_Title)),
        L(nameof(SR.UseModificationAudited_Message)),
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Diagnostic rule for suggesting to use IDeletionAudited interface.
    /// <para>Message format: "Consider using IDeletionAudited interface instead."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor UseDeletionAudited = new(
        UseDeletionAuditedId,
        L(nameof(SR.UseDeletionAudited_Title)),
        L(nameof(SR.UseDeletionAudited_Message)),
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Diagnostic rule for suggesting to use IFullAudited interface.
    /// <para>Message format: "Consider using IFullAudited interface instead."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor UseFullAudited = new(
        UseFullAuditedId,
        L(nameof(SR.UseFullAudited_Title)),
        L(nameof(SR.UseFullAudited_Message)),
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    #endregion Rules
}
