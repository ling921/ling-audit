using Ling.Audit.SourceGenerators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class UseNonPropertyInterfaceAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DiagnosticDescriptors.UseNonPropertyInterface
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.BaseList);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var baseList = (BaseListSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var userSymbols = GetUserSymbols(context.Compilation);

        var baseTypes = baseList.Types
            .Select(type => (BaseType: type, TypeSymbol: semanticModel.GetTypeInfo(type.Type, context.CancellationToken).Type))
            .Where(i => i.TypeSymbol is INamedTypeSymbol { TypeKind: TypeKind.Interface })
            .Select(i => (i.BaseType, (INamedTypeSymbol)i.TypeSymbol!))
            .ToList();

        var flags = new (bool, Location?, ImmutableDictionary<string, string?>?)[6];

        foreach (var (baseType, typeSymbol) in baseTypes)
        {
            var originalDefinition = typeSymbol.OriginalDefinition;

            var matched = Array.FindIndex(userSymbols, i => SymbolEqualityComparer.Default.Equals(originalDefinition, i));
            switch (matched)
            {
                case 0:
                case 2:
                case 4:
                    flags[matched] = (true, null, null);
                    break;
                case 1:
                    flags[1] = (true, baseType.GetLocation(), ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IHasCreator"));
                    break;
                case 3:
                    flags[3] = (true, baseType.GetLocation(), ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IHasModifier"));
                    break;
                case 5:
                    flags[5] = (true, baseType.GetLocation(), ImmutableDictionary<string, string?>.Empty.Add("TargetInterface", "IHasDeleter"));
                    break;
            }
        }

        if (flags[1] is (true, Location location1, ImmutableDictionary<string, string?> properties1))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.UseNonPropertyInterface,
                location1,
                properties1.Add("RemoveInterface", flags[0] is (true, _, _) ? "true" : "false"),
                "IHasCreator<TKey>");
            context.ReportDiagnostic(diagnostic);
        }
        if (flags[3] is (true, Location location2, ImmutableDictionary<string, string?> properties2))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.UseNonPropertyInterface,
                location2,
                properties2.Add("RemoveInterface", flags[2] is (true, _, _) ? "true" : "false"),
                "IHasModifier<TKey>");
            context.ReportDiagnostic(diagnostic);
        }
        else if (flags[5] is (true, Location location3, ImmutableDictionary<string, string?> properties3))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.UseNonPropertyInterface,
                location3,
                properties3.Add("RemoveInterface", flags[4] is (true, _, _) ? "true" : "false"),
                "IHasDeleter<TKey>");
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static INamedTypeSymbol[] GetUserSymbols(Compilation compilation) =>
    [
        compilation.GetTypeByMetadataName(AuditDefaults.IHasCreatorTypeFullQualifiedMetadataName)!,
        compilation.GetTypeByMetadataName(AuditDefaults.IHasCreatorTypeFullQualifiedMetadataName.Replace("Creator", "CreationUser"))!,
        compilation.GetTypeByMetadataName(AuditDefaults.IHasModifierTypeFullQualifiedMetadataName)!,
        compilation.GetTypeByMetadataName(AuditDefaults.IHasModifierTypeFullQualifiedMetadataName.Replace("Modifier", "ModificationUser"))!,
        compilation.GetTypeByMetadataName(AuditDefaults.IHasDeleterTypeFullQualifiedMetadataName)!,
        compilation.GetTypeByMetadataName(AuditDefaults.IHasDeleterTypeFullQualifiedMetadataName.Replace("Deleter", "DeletionUser"))!
    ];
}