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
internal class UseNonPropertyInterfaceCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
    [
        DiagnosticDescriptors.UseNonPropertyInterfaceId
    ];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root?.FindNode(diagnosticSpan) is not BaseTypeSyntax baseType)
        {
            return;
        }

        // Get the type declaration that contains this interface
        var typeDecl = baseType.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (typeDecl == null ||
            typeDecl is not ClassDeclarationSyntax and not RecordDeclarationSyntax ||
            !typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        if (!diagnostic.Properties.TryGetValue("TargetInterface", out var targetInterface) ||
            targetInterface is not string { Length: > 0 })
        {
            return;
        }

        if (!diagnostic.Properties.TryGetValue("RemoveInterface", out var removeInterface) ||
            removeInterface != "true")
        {
            var title = $"Use '{targetInterface}<TKey>' interface";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceInterfaceAsync(context.Document, baseType, targetInterface, c),
                    equivalenceKey: title),
                diagnostic);
        }
        else
        {
            var originalInterface = targetInterface switch
            {
                "IHasCreator" => "IHasCreationUser",
                "IHasModifier" => "IHasModificationUser",
                "IHasDeleter" => "IHasDeletionUser",
                _ => targetInterface
            };
            var title = $"Remove '{originalInterface}<TKey>' interface because '{targetInterface}<TKey>' exists";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RemoveInterfaceAsync(context.Document, baseType, c),
                    equivalenceKey: title),
                diagnostic);
        }
    }

    private async Task<Document> ReplaceInterfaceAsync(Document document, BaseTypeSyntax baseType, string targetInterface, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        var newInterface = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.GenericName(
                SyntaxFactory.Identifier(targetInterface),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(baseType.DescendantNodes()
                    .OfType<GenericNameSyntax>()
                    .SelectMany(g => g.TypeArgumentList.Arguments)))
                )
            ).WithTriviaFrom(baseType);

        var newRoot = root.ReplaceNode(baseType, newInterface);
        return document.WithSyntaxRoot(newRoot);
    }

    private async Task<Document> RemoveInterfaceAsync(Document document, BaseTypeSyntax baseType, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        var newRoot = root.RemoveNode(baseType, SyntaxRemoveOptions.KeepEndOfLine)!;
        return document.WithSyntaxRoot(newRoot);
    }
}