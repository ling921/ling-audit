using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators;

internal sealed class AuditSymbols(Compilation compilation)
{
    public INamedTypeSymbol? IHasCreator => compilation.GetTypeByMetadataName(AuditDefaults.IHasCreatorTypeFullQualifiedName);
    public INamedTypeSymbol? IHasCreationTime => compilation.GetTypeByMetadataName(AuditDefaults.IHasCreationTimeTypeFullQualifiedName);
    public INamedTypeSymbol? ICreationAudited => compilation.GetTypeByMetadataName(AuditDefaults.ICreationAuditedTypeFullQualifiedName);
    public INamedTypeSymbol? IHasLastModifier => compilation.GetTypeByMetadataName(AuditDefaults.IHasLastModifierTypeFullQualifiedName);
    public INamedTypeSymbol? IHasLastModificationTime => compilation.GetTypeByMetadataName(AuditDefaults.IHasLastModificationTimeTypeFullQualifiedName);
    public INamedTypeSymbol? IModificationAudited => compilation.GetTypeByMetadataName(AuditDefaults.IModificationAuditedTypeFullQualifiedName);
    public INamedTypeSymbol? ISoftDelete => compilation.GetTypeByMetadataName(AuditDefaults.ISoftDeleteTypeFullQualifiedName);
    public INamedTypeSymbol? IHasDeleter => compilation.GetTypeByMetadataName(AuditDefaults.IHasDeleterTypeFullQualifiedName);
    public INamedTypeSymbol? IHasDeletionTime => compilation.GetTypeByMetadataName(AuditDefaults.IHasDeletionTimeTypeFullQualifiedName);
    public INamedTypeSymbol? IDeletionAudited => compilation.GetTypeByMetadataName(AuditDefaults.IDeletionAuditedTypeFullQualifiedName);
    public INamedTypeSymbol? IFullAudited => compilation.GetTypeByMetadataName(AuditDefaults.IFullAuditedTypeFullQualifiedName);
}
