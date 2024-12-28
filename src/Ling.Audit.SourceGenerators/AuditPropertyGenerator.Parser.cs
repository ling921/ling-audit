using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Ling.Audit.SourceGenerators;

partial class AuditPropertyGenerator
{
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is TypeDeclarationSyntax { BaseList.Types.Count: > 0 } typeDeclaration &&
            (typeDeclaration is ClassDeclarationSyntax or RecordDeclarationSyntax) &&
            typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    private static AuditGenerationContext GetGenerationContext(GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return new(typeDeclaration, string.Empty, EquatableArray<AuditPropertyInfo>.Empty);
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
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable | SymbolDisplayMiscellaneousOptions.IncludeNotNullableReferenceTypeModifier
        );

        foreach (var @interface in allInterfaces)
        {
            var originalDefinition = @interface.OriginalDefinition;

            if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreator) &&
                !existingProperties.Contains(AuditDefaults.CreatedBy) &&
                @interface.TypeArguments[0].CanAssignNull())
            {
                var keyType = @interface.TypeArguments[0].ToDisplayString(format);
                if (@interface.TypeArguments[0].IsReferenceType) keyType += "?";
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
                if (@interface.TypeArguments[0].IsReferenceType) keyType += "?";
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
                if (@interface.TypeArguments[0].IsReferenceType) keyType += "?";
                propertiesToGenerate.Add(AuditPropertyInfo.DeletedBy(keyType));
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeletionTime) &&
                !existingProperties.Contains(AuditDefaults.DeletedAt))
            {
                propertiesToGenerate.Add(AuditPropertyInfo.DeletedAt);
            }
        }

        return new(typeDeclaration, GetNormalizedTypeName(classSymbol), new(propertiesToGenerate));
    }

    private static string GetNormalizedTypeName(INamedTypeSymbol namedTypeSymbol)
    {
        var sb = new StringBuilder();
        
        if (!string.IsNullOrEmpty(namedTypeSymbol.ContainingNamespace?.ToString()))
        {
            sb.Append(namedTypeSymbol.ContainingNamespace).Append('.');
        }

        BuildTypeFullName(namedTypeSymbol, sb);
        return sb.ToString();
    }

    private static void BuildTypeFullName(INamedTypeSymbol symbol, StringBuilder sb)
    {
        if (symbol.ContainingType is not null)
        {
            BuildTypeFullName(symbol.ContainingType, sb);
            sb.Append('.');
        }

        sb.Append(symbol.Name);

        if (symbol.TypeParameters.Length > 0)
        {
            sb.Append('_').Append(symbol.TypeParameters.Length);
        }
    }
}
