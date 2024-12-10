using Ling.Audit.SourceGenerators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AuditTypeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DiagnosticDescriptors.ValueType,
        DiagnosticDescriptors.PartialType,
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration
        );
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var symbols = new AuditSymbols(semanticModel.Compilation);
        var allInterfaces = typeSymbol.AllInterfaces;
        var existingProperties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(p => p.Name)
            .ToList();

        var needsGeneration = allInterfaces.Any(i =>
        {
            var originalDefinition = i.OriginalDefinition;
            return
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreator) && !existingProperties.Contains(AuditDefaults.CreatedBy)) ||
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreationTime) && !existingProperties.Contains(AuditDefaults.CreatedAt)) ||
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasLastModifier) && !existingProperties.Contains(AuditDefaults.ModifiedBy)) ||
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasLastModificationTime) && !existingProperties.Contains(AuditDefaults.ModifiedAt)) ||
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.ISoftDelete) && !existingProperties.Contains(AuditDefaults.IsDeleted)) ||
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeleter) && !existingProperties.Contains(AuditDefaults.DeletedBy)) ||
                (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeletionTime) && !existingProperties.Contains(AuditDefaults.DeletedAt));
        });

        if (needsGeneration)
        {
            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (!IsClassOrRecord(typeDeclaration))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.ValueType,
                    typeDeclaration.Identifier.GetLocation(),
                    typeName);
                context.ReportDiagnostic(diagnostic);
            }
            else if (!typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.PartialType,
                    typeDeclaration.Identifier.GetLocation(),
                    typeName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsClassOrRecord(TypeDeclarationSyntax typeDeclaration) =>
        typeDeclaration.IsKind(SyntaxKind.ClassDeclaration) ||
        typeDeclaration.IsKind(SyntaxKind.RecordDeclaration);
}
