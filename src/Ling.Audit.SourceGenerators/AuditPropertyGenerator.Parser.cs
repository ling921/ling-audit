using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ling.Audit.SourceGenerators;

partial class AuditPropertyGenerator
{
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.BaseList?.Types.Count > 0;
        }
        return false;
    }

    private record AuditPropertyInfo(string InterfaceName, string PropertyName, string PropertyType);

    private static (ClassDeclarationSyntax Declaration, List<AuditPropertyInfo> Properties)? GetTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (context.Node as ClassDeclarationSyntax)!;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        var allInterfaces = classSymbol.AllInterfaces;
        var propertiesToGenerate = new List<AuditPropertyInfo>();

        var existingProperties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(p => p.Name)
            .ToList();

        foreach (var @interface in allInterfaces)
        {
            var fullName = @interface.OriginalDefinition.ToDisplayString();

            switch (fullName)
            {
                case AuditDefaults.CreatedByTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.CreatedBy))
                    {
                        var keyType = GetPropertyTypeForTKey(@interface.TypeArguments[0]);
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.CreatedBy,
                            keyType
                        ));
                    }
                    break;

                case AuditDefaults.CreatedAtTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.CreatedAt))
                    {
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.CreatedAt,
                            "global::System.DateTimeOffset"
                        ));
                    }
                    break;

                case AuditDefaults.ModifiedByTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.ModifiedBy))
                    {
                        var keyType = GetPropertyTypeForTKey(@interface.TypeArguments[0]);
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.ModifiedBy,
                            keyType
                        ));
                    }
                    break;

                case AuditDefaults.ModifiedAtTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.ModifiedAt))
                    {
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.ModifiedAt,
                            "global::System.Nullable<global::System.DateTimeOffset>"
                        ));
                    }
                    break;

                case AuditDefaults.SoftDeleteTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.IsDeleted))
                    {
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.IsDeleted,
                            "global::System.Boolean"
                        ));
                    }
                    break;

                case AuditDefaults.DeletedByTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.DeletedBy))
                    {
                        var keyType = GetPropertyTypeForTKey(@interface.TypeArguments[0]);
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.DeletedBy,
                            keyType
                        ));
                    }
                    break;

                case AuditDefaults.DeletedAtTypeDef:
                    if (!existingProperties.Contains(AuditDefaults.DeletedAt))
                    {
                        propertiesToGenerate.Add(new AuditPropertyInfo(
                            fullName,
                            AuditDefaults.DeletedAt,
                            "global::System.Nullable<global::System.DateTimeOffset>"
                        ));
                    }
                    break;
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
}
