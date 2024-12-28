using Ling.Audit.SourceGenerators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class AuditTypeAnalyzer : DiagnosticAnalyzer
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

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        switch (typeDeclaration)
        {
            case ClassDeclarationSyntax classDeclaration:
                AnalyzeReferenceTypeDeclarationNode(context, classDeclaration, typeSymbol);
                break;
            case StructDeclarationSyntax structDeclaration:
                AnalyzeValueTypeDeclarationNode(context, structDeclaration, typeSymbol);
                break;
            case RecordDeclarationSyntax recordDeclaration:
                AnalyzeRecordDeclarationNode(context, recordDeclaration, typeSymbol);
                break;
        }
    }

    private static void AnalyzeReferenceTypeDeclarationNode(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
    {
        if (typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        var existingProperties = typeSymbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .Cast<IPropertySymbol>()
            .Where(p => !p.IsImplicitlyDeclared &&
                       p.Locations.Any(l => l.IsInSource))
            .Select(p => p.Name)
            .ToList();

        foreach (var propertyName in GetAuditPropertyNames(context, typeSymbol))
        {
            if (!existingProperties.Contains(propertyName))
            {
                var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.PartialType,
                    typeDeclaration.Identifier.GetLocation(),
                    typeName);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }
    }

    private static void AnalyzeValueTypeDeclarationNode(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
    {
        if (GetAuditPropertyNames(context, typeSymbol).Any())
        {
            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ValueType,
                typeDeclaration.Identifier.GetLocation(),
                typeName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeRecordDeclarationNode(SyntaxNodeAnalysisContext context, RecordDeclarationSyntax recordDeclaration, INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            AnalyzeValueTypeDeclarationNode(context, recordDeclaration, typeSymbol);
        }
        else
        {
            AnalyzeReferenceTypeDeclarationNode(context, recordDeclaration, typeSymbol);
        }
    }

    private static IEnumerable<string> GetAuditPropertyNames(SyntaxNodeAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        var symbols = new AuditSymbols(context.SemanticModel.Compilation);
        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            var originalDefinition = @interface.OriginalDefinition;

            if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreator))
            {
                yield return AuditDefaults.CreatedBy;
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasCreationTime))
            {
                yield return AuditDefaults.CreatedAt;
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasModifier))
            {
                yield return AuditDefaults.ModifiedBy;
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasModificationTime))
            {
                yield return AuditDefaults.ModifiedAt;
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.ISoftDelete))
            {
                yield return AuditDefaults.IsDeleted;
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeleter))
            {
                yield return AuditDefaults.DeletedBy;
            }
            else if (SymbolEqualityComparer.Default.Equals(originalDefinition, symbols.IHasDeletionTime))
            {
                yield return AuditDefaults.DeletedAt;
            }
        }
    }
}
