using Ling.Audit.SourceGenerators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AuditInterfaceAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        DiagnosticDescriptors.UseCreationAudited,
        DiagnosticDescriptors.UseModificationAudited,
        DiagnosticDescriptors.UseDeletionAudited,
        DiagnosticDescriptors.UseFullAudited
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

        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol ||
            typeDeclaration.BaseList?.Types is not { } baseTypes)
        {
            return;
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);

        if (HasInterface(typeSymbol, symbols.IHasCreator) &&
            HasInterface(typeSymbol, symbols.IHasCreationTime) &&
            !HasInterface(typeSymbol, symbols.ICreationAudited))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.IHasCreator, symbols.IHasCreationTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseCreationAudited,
                    lastInterface.GetLocation(),
                    properties: ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "ICreationAudited"));
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (HasInterface(typeSymbol, symbols.IHasLastModifier) &&
            HasInterface(typeSymbol, symbols.IHasLastModificationTime) &&
            !HasInterface(typeSymbol, symbols.IModificationAudited))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.IHasLastModifier, symbols.IHasLastModificationTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseModificationAudited,
                    lastInterface.GetLocation(),
                    properties: ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IModificationAudited"));
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (HasInterface(typeSymbol, symbols.ISoftDelete) &&
            HasInterface(typeSymbol, symbols.IHasDeleter) &&
            HasInterface(typeSymbol, symbols.IHasDeletionTime) &&
            !HasInterface(typeSymbol, symbols.IDeletionAudited))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.ISoftDelete, symbols.IHasDeleter, symbols.IHasDeletionTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseDeletionAudited,
                    lastInterface.GetLocation(),
                    properties: ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IDeletionAudited"));
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (HasInterface(typeSymbol, symbols.ICreationAudited) &&
            HasInterface(typeSymbol, symbols.IModificationAudited) &&
            HasInterface(typeSymbol, symbols.IDeletionAudited) &&
            !HasInterface(typeSymbol, symbols.IFullAudited))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.ICreationAudited, symbols.IModificationAudited, symbols.IDeletionAudited));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseFullAudited,
                    lastInterface.GetLocation(),
                    properties: ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IFullAudited"));
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool HasInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol? interfaceSymbol)
    {
        return interfaceSymbol != null &&
            typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol));
    }

    private static bool IsTargetInterface(BaseTypeSyntax baseType, SemanticModel semanticModel, params INamedTypeSymbol?[] targetSymbols)
    {
        if (semanticModel.GetTypeInfo(baseType.Type).Type is not INamedTypeSymbol typeSymbol)
        {
            return false;
        }

        return targetSymbols.Any(target =>
            target != null &&
            SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, target));
    }
}
