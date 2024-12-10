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
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(
            classDeclarations,
            static (spc, source) => Execute(spc, source!.Value.Declaration, source!.Value.Properties));
    }

    /// <summary>
    /// Execute the source generation.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="properties">The properties.</param>
    private static void Execute(
        SourceProductionContext context,
        ClassDeclarationSyntax classDeclaration,
        List<AuditPropertyInfo> properties)
    {
        var generatedCode = GetGeneratedCode(classDeclaration, properties);
        context.AddSource($"{classDeclaration.Identifier}.g.cs", generatedCode);
    }
}
