using Ling.Audit.SourceGenerators.Resources;
using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators.Diagnostics;

internal static class DiagnosticDescriptors
{
    private const string Category = "Design";

    private static LocalizableResourceString L(string name) => new(name, SR.ResourceManager, typeof(SR));

    #region Diagnostic IDs

    /// <summary>
    /// Diagnostic ID for <see cref="TypeParameterMustBeNullable"/>: Type parameter must be nullable
    /// </summary>
    public const string TypeParameterMustBeNullableId = "LA001";

    /// <summary>
    /// Diagnostic ID for <see cref="ValueType"/>: Audit operations are not supported on value types
    /// </summary>
    public const string ValueTypeId = "LA101";

    /// <summary>
    /// Diagnostic ID for <see cref="PartialType"/>: Type must be declared as partial to support audit operations
    /// </summary>
    public const string PartialTypeId = "LA102";

    /// <summary>
    /// Diagnostic ID for <see cref="KeyTypeMismatch"/>: Key type mismatch detected between multiple audit interfaces
    /// </summary>
    public const string KeyTypeMismatchId = "LA103";

    /// <summary>
    /// Diagnostic ID for <see cref="UseAuditedInterface"/>: Suggestion to use a more comprehensive audit interface
    /// </summary>
    public const string UseAuditedInterfaceId = "LA901";

    #endregion Diagnostic IDs

    #region Rules

    /// <summary>
    /// Diagnostic rule for type parameter must be nullable.
    /// <para>Message format: "Type parameter '{0}' must be nullable."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor TypeParameterMustBeNullable = new(
        TypeParameterMustBeNullableId,
        L(nameof(SR.TypeParameterMustBeNullable_Title)),
        L(nameof(SR.TypeParameterMustBeNullable_Message)),
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: L(nameof(SR.TypeParameterMustBeNullable_Description)));

    /// <summary>
    /// Diagnostic rule for audit operations not supported on value types.
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
    /// Diagnostic rule for partial type requirement.
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
    /// Diagnostic rule for key type mismatch between audit interfaces.
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
    /// Diagnostic rule suggesting the use of a more comprehensive audit interface.
    /// <para>Message format: "Consider using {0} interface instead."</para>
    /// </summary>
    public static readonly DiagnosticDescriptor UseAuditedInterface = new(
        UseAuditedInterfaceId,
        L(nameof(SR.UseAuditedInterface_Title)),
        L(nameof(SR.UseAuditedInterface_Message)),
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: L(nameof(SR.UseAuditedInterface_Description)));

    #endregion Rules
}
