using Ling.Audit.SourceGenerators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

namespace Ling.Audit.SourceGenerators.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
internal class AuditInterfaceCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [
        DiagnosticDescriptors.UseAuditedInterfaceId
    ];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        var node = root.FindNode(context.Span);

        var typeDeclaration = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDeclaration == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        if (!diagnostic.Properties.TryGetValue("TargetInterface", out var targetInterface) ||
            targetInterface is not string { Length: > 0 })
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use {targetInterface}<TKey> interface",
                createChangedDocument: c => UseInterfaceAsync(context.Document, typeDeclaration, targetInterface, c),
                equivalenceKey: targetInterface),
            diagnostic);
    }

    private async Task<Document> UseInterfaceAsync(Document document, TypeDeclarationSyntax typeDecl, string targetInterface, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);
        var baseList = typeDecl.BaseList;
        if (baseList == null)
        {
            return document;
        }

        // Get interfaces to be removed based on target interface
        var interfacesToRemove = targetInterface switch
        {
            "ICreationAudited" => baseList.Types.Where(t =>
                IsTargetInterface(t, semanticModel, symbols.IHasCreator, symbols.IHasCreationTime)).ToList(),
            "IModificationAudited" => baseList.Types.Where(t =>
                IsTargetInterface(t, semanticModel, symbols.IHasLastModifier, symbols.IHasLastModificationTime)).ToList(),
            "IDeletionAudited" => baseList.Types.Where(t =>
                IsTargetInterface(t, semanticModel, symbols.ISoftDelete, symbols.IHasDeleter, symbols.IHasDeletionTime)).ToList(),
            "IFullAudited" => baseList.Types.Where(t =>
                IsTargetInterface(t, semanticModel, symbols.ICreationAudited, symbols.IModificationAudited, symbols.IDeletionAudited)).ToList(),
            _ => []
        };

        if (!interfacesToRemove.Any())
        {
            return document;
        }

        // Get the position of the first interface to be removed
        var firstInterface = interfacesToRemove.First();
        var lastInterface = interfacesToRemove.Last();
        var insertIndex = baseList.Types.IndexOf(firstInterface);

        // Get generic type parameter from any interface that has one
        var keyType = GetKeyType(interfacesToRemove.FirstOrDefault(i => i.Type is GenericNameSyntax), semanticModel);

        // Preserve original formatting for each interface
        var newTypes = new List<BaseTypeSyntax>();
        var originalTypes = baseList.Types.ToList();
        var originalSeparators = baseList.Types.GetSeparators().ToList();
        var newSeparators = new List<SyntaxToken>();

        for (var i = 0; i < originalTypes.Count; i++)
        {
            if (i == insertIndex)
            {
                // Create new interface with original formatting
                var newInterface = SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(keyType != null ? $"{targetInterface}<{keyType}>" : targetInterface))
                    .WithLeadingTrivia(originalTypes[i].GetLeadingTrivia())
                    .WithTrailingTrivia(lastInterface.GetTrailingTrivia());

                newTypes.Add(newInterface);
                i += interfacesToRemove.Count - 1;

                // Add separator if not the last element
                if (i < originalTypes.Count - 1)
                {
                    newSeparators.Add(originalSeparators[Math.Min(i, originalSeparators.Count - 1)]);
                }
            }
            else if (!interfacesToRemove.Contains(originalTypes[i]))
            {
                newTypes.Add(originalTypes[i]);

                // Add separator if not the last element
                if (newTypes.Count < originalTypes.Count - interfacesToRemove.Count + 1
                    && i < originalSeparators.Count)
                {
                    newSeparators.Add(originalSeparators[i]);
                }
            }
        }

        var newBaseList = baseList.WithTypes(SyntaxFactory.SeparatedList(newTypes, newSeparators));

        // Create new type declaration
        var newTypeDecl = typeDecl.WithBaseList(newBaseList);

        // Replace the root node
        var newRoot = root.ReplaceNode(typeDecl, newTypeDecl);
        return document.WithSyntaxRoot(newRoot);
    }

    private static string? GetKeyType(BaseTypeSyntax? baseType, SemanticModel semanticModel)
    {
        if (baseType?.Type is GenericNameSyntax genericName)
        {
            return genericName.TypeArgumentList.Arguments.First().ToString();
        }
        return null;
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
