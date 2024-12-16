using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ling.Audit.SourceGenerators;

partial class AuditPropertyGenerator
{
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };

    private static (ClassDeclarationSyntax Declaration, List<AuditPropertyInfo> Properties)? GetTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (context.Node as ClassDeclarationSyntax)!;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);
        var allInterfaces = classSymbol.AllInterfaces;
        var propertiesToGenerate = new List<AuditPropertyInfo>();

        var existingProperties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(p => p.Name)
            .ToList();

        foreach (var @interface in allInterfaces)
        {
            var originalDefinition = @interface.OriginalDefinition;

            if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreator) &&
                !existingProperties.Contains(AuditDefaults.CreatedBy))
            {
                var keyType = GetPropertyTypeForTKey(@interface.TypeArguments[0]);
                propertiesToGenerate.Add(AuditPropertyInfo.CreatedBy(keyType));
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreationTime) &&
                !existingProperties.Contains(AuditDefaults.CreatedAt))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.CreatedAt);
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasLastModifier) &&
                !existingProperties.Contains(AuditDefaults.ModifiedBy))
            {
                var keyType = GetPropertyTypeForTKey(@interface.TypeArguments[0]);
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
                !existingProperties.Contains(AuditDefaults.DeletedBy))
            {
                var keyType = GetPropertyTypeForTKey(@interface.TypeArguments[0]);
                propertiesToGenerate.Add(AuditPropertyInfo.DeletedBy(keyType));
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeletionTime) &&
                !existingProperties.Contains(AuditDefaults.DeletedAt))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.DeletedAt);
            }
        }

        return propertiesToGenerate.Count > 0
            ? (classDeclaration, propertiesToGenerate)
            : null;
    }

    private static string GetPropertyTypeForTKey(ITypeSymbol keyType)
    {
        var format = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None
        );

        var baseType = keyType is INamedTypeSymbol namedType &&
                      namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            ? namedType.TypeArguments[0]
            : keyType;

        var baseTypeString = baseType.ToDisplayString(format);

        return baseType.IsValueType
            ? $"global::System.Nullable<{baseTypeString}>"
            : $"{baseTypeString}?";
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
