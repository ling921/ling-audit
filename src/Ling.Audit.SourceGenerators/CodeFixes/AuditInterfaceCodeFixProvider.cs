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

        var diagnostic = context.Diagnostics[0];
        if (diagnostic.Properties.GetValueOrDefault("Suggestion") is { Length: > 0 } suggestion &&
            diagnostic.Properties.GetValueOrDefault("Indexes") is { Length: > 0 } indexesText &&
            GetIndexes(indexesText) is { Length: > 1 } indexes)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Use {suggestion}<TKey> interface",
                    createChangedDocument: c => ApplyChanges(context.Document, typeDeclaration, suggestion, indexes, c),
                    equivalenceKey: suggestion),
                diagnostic);
        }
    }

    private async Task<Document> ApplyChanges(
        Document document,
        TypeDeclarationSyntax typeDecl,
        string targetInterface,
        int[] indexes,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var baseList = typeDecl.BaseList;
        if (baseList is null)
        {
            return document;
        }

        var newBaseList = ModifyBaseList(baseList, targetInterface, indexes);

        // Create new type declaration
        var newTypeDecl = typeDecl.WithBaseList(newBaseList);

        // Replace the root node
        var newRoot = root.ReplaceNode(typeDecl, newTypeDecl);
        return document.WithSyntaxRoot(newRoot);
    }

    private static BaseListSyntax ModifyBaseList(BaseListSyntax baseList, string replacement, int[] indexes)
    {
        var baseTypes = baseList.Types.ToList();
        var separators = baseList.Types.GetSeparators().ToList();
        var originalBaseTypeCount = baseTypes.Count;

        BaseTypeSyntax targetBaseType = null!;
        TypeArgumentListSyntax typeArgumentList = null!;

        for (var j = indexes.Length - 1; j >= 0; j--)
        {
            var index = indexes[j];
            if (index >= originalBaseTypeCount || index < 0)
            {
                return baseList;
            }

            // Get type arguments
            if (baseTypes[index].Type is GenericNameSyntax genericName)
            {
                typeArgumentList = genericName.TypeArgumentList;
            }
            else if (baseTypes[index].Type is QualifiedNameSyntax { Right: GenericNameSyntax genericName1 })
            {
                typeArgumentList = genericName1.TypeArgumentList;
            }

            if (j > 0)
            {
                // Keep TrailingTrivia for last base type
                if (index == baseTypes.Count - 1)
                {
                    baseTypes[index - 1] = baseTypes[index - 1].WithTrailingTrivia(baseTypes[index].GetTrailingTrivia());
                }

                baseTypes.RemoveAt(index);

                // Remove separator
                if (index == separators.Count)
                {
                    separators.RemoveAt(index - 1);
                }
                else
                {
                    separators.RemoveAt(index);
                }
            }
            else
            {
                targetBaseType = baseTypes[index];
            }
        }

        var replaceGenericType = SyntaxFactory.GenericName(SyntaxFactory.Identifier(replacement), typeArgumentList);
        TypeSyntax typeSyntax = targetBaseType.Type is QualifiedNameSyntax qualifiedName
            ? qualifiedName.WithRight(replaceGenericType)
            : replaceGenericType;

        baseTypes[indexes[0]] = SyntaxFactory.SimpleBaseType(typeSyntax).WithTriviaFrom(targetBaseType);

        var typeList = SyntaxFactory.SeparatedList(baseTypes, separators);
        return baseList.Update(baseList.ColonToken, typeList);
    }

    private static int[]? GetIndexes(string indexes)
    {
        var arr = indexes.Split(',');

        var arr2 = new int[arr.Length];

        for (int i = 0; i < arr.Length; i++)
        {
            if (int.TryParse(arr[i], out var index))
            {
                arr2[i] = index;
            }
            else
            {
                return null;
            }
        }

        return arr2;
    }
}
