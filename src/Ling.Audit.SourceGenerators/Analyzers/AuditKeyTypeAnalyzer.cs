using Ling.Audit.SourceGenerators.Diagnostics;
using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class AuditKeyTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DiagnosticDescriptors.KeyTypeMismatch,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);
        var auditInterfaces = typeSymbol.AllInterfaces
            .Where(i =>
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, symbols.IHasCreator) ||
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, symbols.IHasModifier) ||
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, symbols.IHasDeleter))
            .ToList();

        if (auditInterfaces.Count <= 1)
        {
            return;
        }

        var comparer = new UnderlyingSymbolEqualityComparer();
        var keyTypes = auditInterfaces
            .Select(i => i.TypeArguments[0])
            .Distinct(comparer)
            .ToList();

        if (keyTypes.Count > 1)
        {
            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var keyTypeNames = string.Join(", ", keyTypes.Select(t => t?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.KeyTypeMismatch,
                typeDeclaration.Identifier.GetLocation(),
                typeName,
                keyTypeNames);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
