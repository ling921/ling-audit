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
                !existingProperties.Contains(AuditDefaults.CreatedBy))
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
                !existingProperties.Contains(AuditDefaults.ModifiedBy))
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
                !existingProperties.Contains(AuditDefaults.DeletedBy))
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

        if (propertiesToGenerate.Count > 0)
        {
            Test(classDeclaration, semanticModel);
        }

        return propertiesToGenerate.Count > 0
            ? (classDeclaration, propertiesToGenerate)
            : null;
    }

    private static void Test(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var baseList = classDeclaration.BaseList!;
        //var firstType = baseList.Types.First().Type!;
        //if (firstType is GenericNameSyntax genericName)
        //{
        //    var typeArguments = genericName.TypeArgumentList.Arguments;
        //    foreach (var argument in typeArguments)
        //    {
        //        var attributes = argument.GetAttributes();
        //        if (attributes.Length > 0)
        //        {
        //            var attribute = attributes[0];
        //        }
        //    }
        //}
        //var firstTypeSymbol = semanticModel.GetTypeInfo(firstType).Type!;
        //var xxx = firstTypeSymbol.ContainingType;
        //if (firstTypeSymbol is INamedTypeSymbol { IsGenericType: true } typeSymbol)
        //{
        //    var arguments = typeSymbol.TypeArguments;
        //    foreach (var argument in arguments)
        //    {
        //        var attributes = argument.GetAttributes();
        //        if (attributes.Length > 0)
        //        {
        //            var attribute = attributes[0];
        //        }
        //    }

        //    var ori = typeSymbol.OriginalDefinition;
        //    var arguments2 = typeSymbol.TypeArguments;
        //    foreach (var argument in arguments2)
        //    {
        //        var attributes = argument.GetAttributes();
        //        if (attributes.Length > 0)
        //        {
        //            var attribute = attributes[0];
        //        }
        //    }
        //    var parameters2 = typeSymbol.TypeParameters;
        //    foreach (var parameter in parameters2)
        //    {
        //        var attributes = parameter.GetAttributes();
        //        if (attributes.Length > 0)
        //        {
        //            var attribute = attributes[0];
        //        }
        //    }
        //}
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
