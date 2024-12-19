using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ling.Audit.SourceGenerators;

partial class AuditPropertyGenerator
{
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is TypeDeclarationSyntax { BaseList.Types.Count: > 0 } typeDeclaration &&
            (typeDeclaration is ClassDeclarationSyntax or RecordDeclarationSyntax) &&
            typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    private static (TypeDeclarationSyntax Declaration, EquatableArray<AuditPropertyInfo> Properties) GetTargetForGeneration(GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return (typeDeclaration, EquatableArray<AuditPropertyInfo>.Empty);
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);
        var allInterfaces = classSymbol.AllInterfaces;
        var propertiesToGenerate = new List<AuditPropertyInfo>();

        var existingProperties = classSymbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .Cast<IPropertySymbol>()
            .Where(p => !p.IsImplicitlyDeclared &&
                       p.Locations.Any(l => l.IsInSource))
            .Select(p => p.Name)
            .ToList();

        var format = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

        foreach (var @interface in allInterfaces)
        {
            var originalDefinition = @interface.OriginalDefinition;

            if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreator) &&
                !existingProperties.Contains(AuditDefaults.CreatedBy) &&
                @interface.TypeArguments[0].CanAssignNull())
            {
                var keyType = @interface.TypeArguments[0].ToDisplayString(format);
                propertiesToGenerate.Add(AuditPropertyInfo.CreatedBy(keyType));
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreationTime) &&
                !existingProperties.Contains(AuditDefaults.CreatedAt))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.CreatedAt);
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasLastModifier) &&
                !existingProperties.Contains(AuditDefaults.ModifiedBy) &&
                @interface.TypeArguments[0].CanAssignNull())
            {
                var keyType = @interface.TypeArguments[0].ToDisplayString(format);
                propertiesToGenerate.Add(AuditPropertyInfo.ModifiedBy(keyType));
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasLastModificationTime) &&
                !existingProperties.Contains(AuditDefaults.ModifiedAt))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.ModifiedAt);
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.ISoftDelete) &&
                !existingProperties.Contains(AuditDefaults.IsDeleted))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.IsDeleted);
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeleter) &&
                !existingProperties.Contains(AuditDefaults.DeletedBy) &&
                @interface.TypeArguments[0].CanAssignNull())
            {
                var keyType = @interface.TypeArguments[0].ToDisplayString(format);
                propertiesToGenerate.Add(AuditPropertyInfo.DeletedBy(keyType));
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeletionTime) &&
                !existingProperties.Contains(AuditDefaults.DeletedAt))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.DeletedAt);
            }
        }

        return (typeDeclaration, new(propertiesToGenerate));
    }

    private record AuditPropertyInfo(
        string InterfaceName,
        string PropertyName,
        string PropertyType)
    {
        public static AuditPropertyInfo CreatedBy(string keyType) => new(
            AuditDefaults.IHasCreatorTypeFullQualifiedMetadataName,
            AuditDefaults.CreatedBy,
            keyType
        );

        public static readonly AuditPropertyInfo CreatedAt = new(
            AuditDefaults.IHasCreationTimeTypeFullQualifiedMetadataName,
            AuditDefaults.CreatedAt,
            "global::System.DateTimeOffset"
        );

        public static AuditPropertyInfo ModifiedBy(string keyType) => new(
            AuditDefaults.IHasModifierTypeFullQualifiedMetadataName,
            AuditDefaults.ModifiedBy,
            keyType
        );

        public static readonly AuditPropertyInfo ModifiedAt = new(
            AuditDefaults.IHasModificationTimeTypeFullQualifiedMetadataName,
            AuditDefaults.ModifiedAt,
            "global::System.Nullable<global::System.DateTimeOffset>"
        );

        public static readonly AuditPropertyInfo IsDeleted = new(
            AuditDefaults.ISoftDeleteTypeFullQualifiedMetadataName,
            AuditDefaults.IsDeleted,
            "global::System.Boolean"
        );

        public static AuditPropertyInfo DeletedBy(string keyType) => new(
            AuditDefaults.IHasDeleterTypeFullQualifiedMetadataName,
            AuditDefaults.DeletedBy,
            keyType
        );

        public static readonly AuditPropertyInfo DeletedAt = new(
            AuditDefaults.IHasDeletionTimeTypeFullQualifiedMetadataName,
            AuditDefaults.DeletedAt,
            "global::System.Nullable<global::System.DateTimeOffset>"
        );
    }
}
