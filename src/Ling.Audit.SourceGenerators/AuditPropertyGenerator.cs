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
                .Select(group => GetGenerationContext(group.First()))
                .Where(ctx => ctx.ShouldGenerate));

        context.RegisterSourceOutput(declarations, Execute);
    }

    private static void Execute(
        SourceProductionContext context,
        AuditGenerationContext generationContext)
    {
        var (declaration, normalizedName, properties) = generationContext;

        var generatedCode = GetGeneratedCode(declaration, properties);
        context.AddSource(
            $"{normalizedName}.g.cs",
            generatedCode);
    }
}
