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
            typeDeclaration.BaseList?.Types is not { Count: > 0 } baseTypes)
        {
            return;
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);
        var directlyImplementedTypes = baseTypes
            .Select(t => semanticModel.GetTypeInfo(t.Type, context.CancellationToken).Type)
            .OfType<INamedTypeSymbol>()
            //.Where(t => t.TypeKind == TypeKind.Interface)
            .ToList();

        bool IsImplementedDirectly(INamedTypeSymbol interfaceSymbol, out int index, out INamedTypeSymbol? implementedInterface)
        {
            for (var i = 0; i < directlyImplementedTypes.Count; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(directlyImplementedTypes[i].OriginalDefinition, interfaceSymbol))
                {
                    index = i;
                    implementedInterface = directlyImplementedTypes[i];
                    return true;
                }
            }

            index = -1;
            implementedInterface = null;
            return false;
        }

        if (IsImplementedDirectly(symbols.IHasCreator, out var c1, out _) &&
            IsImplementedDirectly(symbols.IHasCreationTime, out var c2, out _) &&
            !IsImplementedDirectly(symbols.ICreationAudited, out _, out _))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.IHasCreator, symbols.IHasCreationTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    descriptor: DiagnosticDescriptors.UseAuditedInterface,
                    location: lastInterface.GetLocation(),
                    properties: BuildProperties("ICreationAudited", c1, c2),
                    "ICreationAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (IsImplementedDirectly(symbols.IHasModifier, out var m1, out _) &&
            IsImplementedDirectly(symbols.IHasModificationTime, out var m2, out _) &&
            !IsImplementedDirectly(symbols.IModificationAudited, out _, out _))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.IHasModifier, symbols.IHasModificationTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    descriptor: DiagnosticDescriptors.UseAuditedInterface,
                    location: lastInterface.GetLocation(),
                    properties: BuildProperties("IModificationAudited", m1, m2),
                    "IModificationAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (IsImplementedDirectly(symbols.ISoftDelete, out var d1, out _) &&
            IsImplementedDirectly(symbols.IHasDeleter, out var d2, out _) &&
            IsImplementedDirectly(symbols.IHasDeletionTime, out var d3, out _) &&
            !IsImplementedDirectly(symbols.IDeletionAudited, out _, out _))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.ISoftDelete, symbols.IHasDeleter, symbols.IHasDeletionTime));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    descriptor: DiagnosticDescriptors.UseAuditedInterface,
                    location: lastInterface.GetLocation(),
                    properties: BuildProperties("IDeletionAudited", d1, d2, d3),
                    "IDeletionAudited<TKey>");
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (IsImplementedDirectly(symbols.ICreationAudited, out var f1, out var creationAudited) &&
            IsImplementedDirectly(symbols.IModificationAudited, out var f2, out var modificationAudited) &&
            IsImplementedDirectly(symbols.IDeletionAudited, out var f3, out var deletionAudited) &&
            !IsImplementedDirectly(symbols.IFullAudited, out _, out _) &&
            HasConsistentGenericParameters(creationAudited, modificationAudited, deletionAudited))
        {
            var lastInterface = baseTypes
                .LastOrDefault(t => IsTargetInterface(t, semanticModel,
                    symbols.ICreationAudited, symbols.IModificationAudited, symbols.IDeletionAudited));

            if (lastInterface != null)
            {
                var diagnostic = Diagnostic.Create(
                    descriptor: DiagnosticDescriptors.UseAuditedInterface,
                    location: lastInterface.GetLocation(),
                    properties: BuildProperties("IFullAudited", f1, f2, f3),
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

    private static ImmutableDictionary<string, string?> BuildProperties(string suggestion, params int[] indexes)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string?>();
        builder.Add("Suggestion", suggestion);
        builder.Add("Indexes", string.Join(",", indexes.OrderBy(i => i)));
        return builder.ToImmutable();
    }
}
