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
internal class MakeNullableCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
    [
        DiagnosticDescriptors.TypeParameterMustBeNullableId
    ];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root.FindNode(diagnosticSpan) is not TypeParameterSyntax node)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make type parameter nullable",
                createChangedDocument: c => MakeNullableAsync(context.Document, node, c),
                equivalenceKey: "MakeNullable"),
            diagnostic);
    }

    private static async Task<Document> MakeNullableAsync(Document document, TypeParameterSyntax typeParameterSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        if (semanticModel.GetDeclaredSymbol(typeParameterSyntax, cancellationToken) is not ITypeParameterSymbol typeParameterSymbol || !typeParameterSymbol.HasValueTypeConstraint)
        {
            return document;
        }

        if (typeParameterSyntax.Parent is not TypeParameterListSyntax)
        {
            return document;
        }

        var newTypeParameter = SyntaxFactory.TypeParameter(typeParameterSyntax.Identifier.WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia("?")));
        var newRoot = root.ReplaceNode(typeParameterSyntax, newTypeParameter);

        return document.WithSyntaxRoot(newRoot);
    }
}
