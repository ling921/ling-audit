using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ling.Audit.SourceGenerators;

/// <summary>
/// Source generator for audit properties.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class AuditPropertyGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => ctx)
            .Collect()
            .SelectMany((ctxs, _) => ctxs
                .GroupBy(ctx => ctx.Node.SyntaxTree)
                .Select(group => GetTargetForGeneration(group.First()))
                .Where(result => result.Properties.Length > 0));

        context.RegisterSourceOutput(declarations, Execute);
    }

    private static void Execute(
        SourceProductionContext context,
        (TypeDeclarationSyntax Declaration, EquatableArray<AuditPropertyInfo> Properties) declarationInfo)
    {
        var (declaration, properties) = declarationInfo;

        var generatedCode = GetGeneratedCode(declaration, properties);
        context.AddSource(
            $"{declaration.Identifier}_{declaration.SyntaxTree.FilePath.GetHashCode():X8}.g.cs",
            generatedCode);
    }
}
