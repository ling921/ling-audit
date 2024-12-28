using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ling.Audit.SourceGenerators;

partial class AuditPropertyGenerator
{
    private static string GetGeneratedCode(TypeDeclarationSyntax typeDeclaration, EquatableArray<AuditPropertyInfo> properties)
    {
        var (namespaceName, containingTypes) = GetTypeContext(typeDeclaration);

        var cb = new CodeBuilder();

        cb.AppendLine("""
            // <auto-generated/>

            #pragma warning disable
            #nullable enable annotations

            """);

        cb.AppendLine($"namespace {namespaceName}")
            .OpenBrace();

        var i = 0;
        for (; i < containingTypes.Count - 1; i++)
        {
            cb.AppendFormatLine("partial {0}", containingTypes[i])
                .OpenBrace();
        }

        cb.AppendFormatLine("[global::System.CodeDom.Compiler.GeneratedCode(\"Ling.Audit.SourceGenerators\", \"{0}\")]", AuditDefaults.Version)
            .AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]")
            .AppendFormatLine("partial {0}", containingTypes[i])
            .OpenBrace();

        var index = 0;
        var isSealed = typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword));
        var virtualModifier = isSealed ? "" : "virtual ";

        foreach (var property in properties)
        {
            var inheritDocClass = property.PropertyName switch
            {
                AuditDefaults.CreatedBy => AuditDefaults.IHasCreatorTypeFullQualifiedMetadataName.Replace("`1", "{TKey}"),
                AuditDefaults.CreatedAt => AuditDefaults.IHasCreationTimeTypeFullQualifiedMetadataName,
                AuditDefaults.ModifiedBy => AuditDefaults.IHasModifierTypeFullQualifiedMetadataName.Replace("`1", "{TKey}"),
                AuditDefaults.ModifiedAt => AuditDefaults.IHasModificationTimeTypeFullQualifiedMetadataName,
                AuditDefaults.IsDeleted => AuditDefaults.ISoftDeleteTypeFullQualifiedMetadataName,
                AuditDefaults.DeletedBy => AuditDefaults.IHasDeleterTypeFullQualifiedMetadataName.Replace("`1", "{TKey}"),
                AuditDefaults.DeletedAt => AuditDefaults.IHasDeletionTimeTypeFullQualifiedMetadataName,
                _ => string.Empty,
            };

            cb.AppendFormatLine("/// <inheritdoc cref=\"global::{0}.{1}\"/>", inheritDocClass, property.PropertyName)
                .AppendFormatLine("public {0}{1} {2} {{ get; set; }}", virtualModifier, property.PropertyType, property.PropertyName);

            if (++index < properties.Count)
            {
                cb.AppendLine();
            }
        }

        cb.CloseAllBrace();

        return cb.ToString();
    }

    private class TypeDeclaration
    {
        public string Keyword { get; }
        public string Name { get; }
        public string? Parameters { get; }

        public TypeDeclaration(TypeDeclarationSyntax typeDeclaration)
        {
            Keyword = typeDeclaration.Keyword.Text;
            Name = typeDeclaration.Identifier.Text;
            Parameters = typeDeclaration.TypeParameterList?.ToString() ?? string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", Keyword, Name, Parameters);
        }
    }

    private static (string Namespace, List<TypeDeclaration> ContainingTypes) GetTypeContext(TypeDeclarationSyntax typeDeclaration)
    {
        var types = new List<TypeDeclaration>();
        SyntaxNode? parent = typeDeclaration;
        var namespaceName = "global::System";

        while (parent != null)
        {
            switch (parent)
            {
                case ClassDeclarationSyntax classDecl:
                    types.Insert(0, new TypeDeclaration(classDecl));
                    break;

                case StructDeclarationSyntax structDecl:
                    types.Insert(0, new TypeDeclaration(structDecl));
                    break;

                case RecordDeclarationSyntax recordDecl:
                    types.Insert(0, new TypeDeclaration(recordDecl));
                    break;

                case BaseNamespaceDeclarationSyntax namespaceDecl:
                    namespaceName = namespaceDecl.Name.ToString();
                    break;
            }

            parent = parent.Parent;
        }

        return (namespaceName, types);
    }
}
