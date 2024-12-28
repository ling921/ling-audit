using Ling.Audit.SourceGenerators.Diagnostics;
using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MustNullAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DiagnosticDescriptors.TypeParameterMustBeNullable
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode,
            SyntaxKind.TypeParameter,
            SyntaxKind.SimpleBaseType,
            SyntaxKind.PropertyDeclaration,
            // Note that FieldDeclarationSyntax contains a VariableDeclarationSyntax, which has a Declaration property.
            SyntaxKind.VariableDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.Parameter,
            SyntaxKind.DelegateDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        switch (context.Node)
        {
            case TypeParameterSyntax typeParameter:
                AnalyzeTypeParameterDeclaration(context, typeParameter);
                break;

            case SimpleBaseTypeSyntax baseType:
                AnalyzeBaseType(context, baseType);
                break;

            case PropertyDeclarationSyntax propertyDeclaration:
                AnalyzePropertyDeclaration(context, propertyDeclaration);
                break;

            case VariableDeclarationSyntax variableDeclaration:
                AnalyzeVariableDeclaration(context, variableDeclaration);
                break;

            case MethodDeclarationSyntax methodDeclaration:
                AnalyzeMethodDeclaration(context, methodDeclaration);
                break;

            case ParameterSyntax parameter:
                AnalyzeParameter(context, parameter);
                break;

            case DelegateDeclarationSyntax delegateDeclaration:
                AnalyzeDelegateDeclaration(context, delegateDeclaration);
                break;
        }
    }

    private static void AnalyzeTypeParameterDeclaration(SyntaxNodeAnalysisContext context, TypeParameterSyntax typeParameter)
    {
        if (context.SemanticModel.GetDeclaredSymbol(typeParameter) is not ITypeParameterSymbol typeParameterSymbol)
        {
            return;
        }

        var mustNullAttribute = GetMustNullAttributeSymbol(context.Compilation);
        if (!HasAttributeSymbol(typeParameterSymbol, mustNullAttribute))
        {
            return;
        }

        if (typeParameterSymbol.HasValueTypeConstraint || typeParameterSymbol.HasNotNullConstraint)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.TypeParameterMustBeNullable,
                typeParameter.Identifier.GetLocation(),
                typeParameterSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeBaseType(SyntaxNodeAnalysisContext context, SimpleBaseTypeSyntax baseType)
    {
        if (baseType.Type is GenericNameSyntax genericName)
        {
            CheckGenericTypeArguments(context, genericName);
        }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.Type is GenericNameSyntax genericName)
        {
            CheckGenericTypeArguments(context, genericName);
        }
    }

    private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context, VariableDeclarationSyntax variableDeclaration)
    {
        if (variableDeclaration.Type is GenericNameSyntax genericName)
        {
            CheckGenericTypeArguments(context, genericName);
        }
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.ReturnType is GenericNameSyntax genericReturnType)
        {
            CheckGenericTypeArguments(context, genericReturnType);
        }
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context, ParameterSyntax parameter)
    {
        if (parameter.Type is GenericNameSyntax genericName)
        {
            CheckGenericTypeArguments(context, genericName);
        }
    }

    private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context, DelegateDeclarationSyntax delegateDeclaration)
    {
        if (delegateDeclaration.ReturnType is GenericNameSyntax genericReturnType)
        {
            CheckGenericTypeArguments(context, genericReturnType);
        }
    }

    private static void CheckGenericTypeArguments(SyntaxNodeAnalysisContext context, GenericNameSyntax genericName)
    {
        if (context.SemanticModel.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var mustNullAttribute = GetMustNullAttributeSymbol(context.Compilation);

        for (var i = 0; i < typeSymbol.TypeArguments.Length; i++)
        {
            var argument = typeSymbol.TypeArguments[i];
            var argumentSyntax = genericName.TypeArgumentList.Arguments[i];

            // Check if current type parameter has MustNullAttribute or its base type has
            if (HasAttributeSymbol(typeSymbol.TypeParameters[i], mustNullAttribute) ||
                (argument is INamedTypeSymbol && HasMustNullDefinitionInBaseTypes(typeSymbol, i, mustNullAttribute)))
            {
                // Check argument is nullable
                if (argument is not ITypeParameterSymbol && !argument.CanAssignNull())
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.TypeParameterMustBeNullable,
                        argumentSyntax.GetLocation(),
                        argumentSyntax.ToFullString());
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // If type argument is a generic type, check itself
            if (argument is INamedTypeSymbol namedTypeArgument &&
                namedTypeArgument.IsGenericType &&
                argumentSyntax is GenericNameSyntax nestedGenericName)
            {
                CheckGenericTypeArguments(context, nestedGenericName);
            }
        }
    }

    private static bool HasMustNullDefinitionInBaseTypes(
        INamedTypeSymbol currentType,
        int argumentIndex,
        INamedTypeSymbol mustNullAttribute)
    {
        // Find base type and interfaces
        var originalType = currentType.OriginalDefinition;
        var genericBaseTypes = new List<INamedTypeSymbol>();
        if (originalType is { BaseType.IsGenericType: true })
        {
            genericBaseTypes.Add(originalType.BaseType);
        }

        genericBaseTypes.AddRange(originalType.Interfaces.Where(t => t.IsGenericType));

        foreach (var baseType in genericBaseTypes)
        {
            // Find argument matched in base type
            for (var i = 0; i < baseType.TypeArguments.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(
                    baseType.TypeArguments[i],
                    originalType.TypeParameters[argumentIndex]))
                {
                    continue;
                }

                // Stop if base type argument is an actual type
                if (baseType.TypeArguments[i] is not ITypeParameterSymbol)
                {
                    break;
                }

                if (HasAttributeSymbol(baseType.TypeParameters[i], mustNullAttribute))
                {
                    return true;
                }

                // Recursively check base type
                if (HasMustNullDefinitionInBaseTypes(baseType.OriginalDefinition, i, mustNullAttribute))
                {
                    return true;
                }

                break;
            }
        }

        return false;
    }

    private static bool HasAttributeSymbol(ITypeParameterSymbol symbol, INamedTypeSymbol? attributeType)
    {
        return attributeType is not null &&
            symbol.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));
    }

    private static INamedTypeSymbol GetMustNullAttributeSymbol(Compilation compilation)
    {
        return compilation.GetTypeByMetadataName(AuditDefaults.MustNullAttributeFullQualifiedMetadataName)!;
    }
}
