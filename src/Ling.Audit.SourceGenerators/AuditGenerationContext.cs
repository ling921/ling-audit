using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ling.Audit.SourceGenerators;

/// <summary>
/// Represents the context for generating audit properties.
/// </summary>
/// <param name="Declaration">The type declaration syntax.</param>
/// <param name="NormalizedName">
/// The normalized type name, including namespace and generic parameter counts.
/// <para>
/// For example: <c>MyNamespace.OuterClass_1.InnerClass_2</c> for
/// <code>
/// namespace MyNamespace
/// {
///     class OuterClass&lt;T&gt;
///     {
///         class InnerClass&lt;T1, T2&gt;
///         {
///         }
///     }
/// }
/// </code>
/// </para></param>
/// <param name="Properties">The properties to generate.</param>
internal record AuditGenerationContext(TypeDeclarationSyntax Declaration, string NormalizedName, EquatableArray<AuditPropertyInfo> Properties)
{
    /// <summary>
    /// Gets a value indicating whether to generate code.
    /// </summary>
    public bool ShouldGenerate => Properties is { Length: > 0 };
}

/// <summary>
/// Represents information about an audit property.
/// </summary>
/// <param name="InterfaceName">The name of the interface that defines the property.</param>
/// <param name="PropertyName">The name of the property.</param>
/// <param name="PropertyType">The type of the property.</param>
internal record AuditPropertyInfo(
    string InterfaceName,
    string PropertyName,
    string PropertyType)
{
    /// <summary>
    /// Creates an <see cref="AuditPropertyInfo"/> for the CreatedBy property.
    /// </summary>
    /// <param name="keyType">The type of the key.</param>
    /// <returns>An <see cref="AuditPropertyInfo"/> for the CreatedBy property.</returns>
    public static AuditPropertyInfo CreatedBy(string keyType) => new(
        AuditDefaults.IHasCreatorTypeFullQualifiedMetadataName,
        AuditDefaults.CreatedBy,
        keyType
    );

    /// <summary>
    /// Gets the <see cref="AuditPropertyInfo"/> for the CreatedAt property.
    /// </summary>
    public static readonly AuditPropertyInfo CreatedAt = new(
        AuditDefaults.IHasCreationTimeTypeFullQualifiedMetadataName,
        AuditDefaults.CreatedAt,
        "global::System.DateTimeOffset"
    );

    /// <summary>
    /// Creates an <see cref="AuditPropertyInfo"/> for the ModifiedBy property.
    /// </summary>
    /// <param name="keyType">The type of the key.</param>
    /// <returns>An <see cref="AuditPropertyInfo"/> for the ModifiedBy property.</returns>
    public static AuditPropertyInfo ModifiedBy(string keyType) => new(
        AuditDefaults.IHasModifierTypeFullQualifiedMetadataName,
        AuditDefaults.ModifiedBy,
        keyType
    );

    /// <summary>
    /// Gets the <see cref="AuditPropertyInfo"/> for the ModifiedAt property.
    /// </summary>
    public static readonly AuditPropertyInfo ModifiedAt = new(
        AuditDefaults.IHasModificationTimeTypeFullQualifiedMetadataName,
        AuditDefaults.ModifiedAt,
        "global::System.Nullable<global::System.DateTimeOffset>"
    );

    /// <summary>
    /// Gets the <see cref="AuditPropertyInfo"/> for the IsDeleted property.
    /// </summary>
    public static readonly AuditPropertyInfo IsDeleted = new(
        AuditDefaults.ISoftDeleteTypeFullQualifiedMetadataName,
        AuditDefaults.IsDeleted,
        "global::System.Boolean"
    );

    /// <summary>
    /// Creates an <see cref="AuditPropertyInfo"/> for the DeletedBy property.
    /// </summary>
    /// <param name="keyType">The type of the key.</param>
    /// <returns>An <see cref="AuditPropertyInfo"/> for the DeletedBy property.</returns>
    public static AuditPropertyInfo DeletedBy(string keyType) => new(
        AuditDefaults.IHasDeleterTypeFullQualifiedMetadataName,
        AuditDefaults.DeletedBy,
        keyType
    );

    /// <summary>
    /// Gets the <see cref="AuditPropertyInfo"/> for the DeletedAt property.
    /// </summary>
    public static readonly AuditPropertyInfo DeletedAt = new(
        AuditDefaults.IHasDeletionTimeTypeFullQualifiedMetadataName,
        AuditDefaults.DeletedAt,
        "global::System.Nullable<global::System.DateTimeOffset>"
    );
}
