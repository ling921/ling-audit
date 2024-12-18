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

        var node = root.FindNode(diagnosticSpan);

        if (node is TypeSyntax typeNode &&
            node is not TypeParameterSyntax)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Make type nullable",
                    createChangedDocument: c => MakeNullableAsync(context.Document, typeNode, c),
                    equivalenceKey: "MakeNullable"),
                diagnostic);
        }
    }

    private static async Task<Document> MakeNullableAsync(Document document, TypeSyntax typeSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        var typeInfo = semanticModel.GetTypeInfo(typeSyntax, cancellationToken);
        if (typeInfo.Type?.IsValueType != true)
        {
            return document;
        }

        var nullableType = SyntaxFactory.NullableType(
            typeSyntax.WithoutTrivia())
            .WithTriviaFrom(typeSyntax);

        var newRoot = root.ReplaceNode(typeSyntax, nullableType);

        return document.WithSyntaxRoot(newRoot);
    }
}
