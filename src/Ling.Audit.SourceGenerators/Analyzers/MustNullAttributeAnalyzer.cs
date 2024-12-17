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
            SyntaxKind.VariableDeclaration,
            SyntaxKind.FieldDeclaration);
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
            case VariableDeclarationSyntax variableDeclaration:
                AnalyzeVariableDeclaration(context, variableDeclaration);
                break;
            case PropertyDeclarationSyntax propertyDeclaration:
                AnalyzePropertyDeclaration(context, propertyDeclaration);
                break;
            case FieldDeclarationSyntax fieldDeclaration:
                AnalyzeFieldDeclaration(context, fieldDeclaration);
                break;
        }
    }

    private static void AnalyzeTypeParameterDeclaration(SyntaxNodeAnalysisContext context, TypeParameterSyntax typeParameter)
    {
        if (context.SemanticModel.GetDeclaredSymbol(typeParameter) is not ITypeParameterSymbol typeParameterSymbol)
            return;

        var mustNullableAttribute = context.Compilation.GetTypeByMetadataName(AuditDefaults.MustNullAttributeFullQualifiedMetadataName);
        if (!HasAttributeSymbol(typeParameterSymbol, mustNullableAttribute))
            return;

        if (typeParameterSymbol.HasValueTypeConstraint && typeParameterSymbol.HasNotNullConstraint)
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
        if (baseType.Type is GenericNameSyntax genericName &&
            context.SemanticModel.GetSymbolInfo(genericName).Symbol is INamedTypeSymbol typeSymbol)
        {
            CheckGenericTypeArguments(context, genericName, typeSymbol);
        }
    }

    private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context, VariableDeclarationSyntax variableDeclaration)
    {
        if (variableDeclaration.Type is GenericNameSyntax genericName &&
            context.SemanticModel.GetSymbolInfo(genericName).Symbol is INamedTypeSymbol typeSymbol)
        {
            CheckGenericTypeArguments(context, genericName, typeSymbol);
        }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.Type is GenericNameSyntax genericName &&
            context.SemanticModel.GetSymbolInfo(genericName).Symbol is INamedTypeSymbol typeSymbol)
        {
            CheckGenericTypeArguments(context, genericName, typeSymbol);
        }
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context, FieldDeclarationSyntax fieldDeclaration)
    {
        if (fieldDeclaration.Declaration is VariableDeclarationSyntax variableDeclaration)
        {
            AnalyzeVariableDeclaration(context, variableDeclaration);
        }
    }

    private static void CheckGenericTypeArguments(SyntaxNodeAnalysisContext context, GenericNameSyntax genericName, INamedTypeSymbol typeSymbol)
    {
        var mustNullableAttribute = context.Compilation.GetTypeByMetadataName(AuditDefaults.MustNullAttributeFullQualifiedMetadataName);

        var index = 0;
        foreach (var (parameter, argument) in typeSymbol.TypeParameters.Zip(typeSymbol.TypeArguments, (p, a) => (p, a)))
        {
            if (!HasAttributeSymbol(parameter, mustNullableAttribute))
            {
                continue;
            }

            if (argument.IsValueType && !argument.IsNullable())
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.TypeParameterMustBeNullable,
                    genericName.TypeArgumentList.Arguments[index].GetLocation(),
                    parameter.Name);
                context.ReportDiagnostic(diagnostic);
            }
            index++;
        }
    }

    private static bool HasAttributeSymbol(ITypeParameterSymbol symbol, INamedTypeSymbol? attributeType)
    {
        return attributeType is not null &&
            symbol.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));
    }
}
