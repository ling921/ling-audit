using Ling.Audit.SourceGenerators.Diagnostics;
using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class AuditInterfaceAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        DiagnosticDescriptors.UseAuditedInterface
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
        var directlyImplementedInterfaces = baseTypes
            .Select(t => semanticModel.GetTypeInfo(t.Type, context.CancellationToken).Type)
            .OfType<INamedTypeSymbol>()
            .Where(t => t.TypeKind == TypeKind.Interface)
            .ToList();

        bool HasDirectlyImplemented(INamedTypeSymbol interfaceSymbol, out INamedTypeSymbol? implementedInterface)
        {
            implementedInterface = directlyImplementedInterfaces.FirstOrDefault(i =>
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol));
            return implementedInterface != null;
        }

        if (HasDirectlyImplemented(symbols.IHasCreator, out _) &&
            HasDirectlyImplemented(symbols.IHasCreationTime, out _) &&
            !HasDirectlyImplemented(symbols.ICreationAudited, out _))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.IHasCreator, symbols.IHasCreationTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseAuditedInterface,
                    lastInterface.GetLocation(),
                    ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "ICreationAudited"),
                    "ICreationAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (HasDirectlyImplemented(symbols.IHasLastModifier, out _) &&
            HasDirectlyImplemented(symbols.IHasLastModificationTime, out _) &&
            !HasDirectlyImplemented(symbols.IModificationAudited, out _))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.IHasLastModifier, symbols.IHasLastModificationTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseAuditedInterface,
                    lastInterface.GetLocation(),
                    ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IModificationAudited"),
                    "IModificationAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (HasDirectlyImplemented(symbols.ISoftDelete, out _) &&
            HasDirectlyImplemented(symbols.IHasDeleter, out _) &&
            HasDirectlyImplemented(symbols.IHasDeletionTime, out _) &&
            !HasDirectlyImplemented(symbols.IDeletionAudited, out _))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.ISoftDelete, symbols.IHasDeleter, symbols.IHasDeletionTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseAuditedInterface,
                    lastInterface.GetLocation(),
                    ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IDeletionAudited"),
                    "IDeletionAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (HasDirectlyImplemented(symbols.ICreationAudited, out var creationAudited) &&
            HasDirectlyImplemented(symbols.IModificationAudited, out var modificationAudited) &&
            HasDirectlyImplemented(symbols.IDeletionAudited, out var deletionAudited) &&
            !HasDirectlyImplemented(symbols.IFullAudited, out _) &&
            HasConsistentGenericParameters(creationAudited, modificationAudited, deletionAudited))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.ICreationAudited, symbols.IModificationAudited, symbols.IDeletionAudited));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UseAuditedInterface,
                    lastInterface.GetLocation(),
                    ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IFullAudited"),
                    "IFullAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }
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

    private static bool HasConsistentGenericParameters(
        INamedTypeSymbol? creationAudited,
        INamedTypeSymbol? modificationAudited,
        INamedTypeSymbol? deletionAudited)
    {
        if (creationAudited == null || modificationAudited == null || deletionAudited == null)
            return false;

        var creationKey = creationAudited.TypeArguments.FirstOrDefault();
        var modificationKey = modificationAudited.TypeArguments.FirstOrDefault();
        var deletionKey = deletionAudited.TypeArguments.FirstOrDefault();

        if (creationKey == null || modificationKey == null || deletionKey == null)
            return false;

        var comparer = new UnderlyingSymbolEqualityComparer();
        return comparer.Equals(creationKey, modificationKey) &&
               comparer.Equals(modificationKey, deletionKey);
    }
}
