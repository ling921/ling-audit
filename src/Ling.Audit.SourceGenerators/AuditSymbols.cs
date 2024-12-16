using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators;

internal sealed class AuditSymbols(Compilation compilation)
{
    public INamedTypeSymbol IHasCreator => compilation.GetTypeByMetadataName(AuditDefaults.IHasCreatorTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IHasCreationTime => compilation.GetTypeByMetadataName(AuditDefaults.IHasCreationTimeTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol ICreationAudited => compilation.GetTypeByMetadataName(AuditDefaults.ICreationAuditedTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IHasLastModifier => compilation.GetTypeByMetadataName(AuditDefaults.IHasModifierTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IHasLastModificationTime => compilation.GetTypeByMetadataName(AuditDefaults.IHasModificationTimeTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IModificationAudited => compilation.GetTypeByMetadataName(AuditDefaults.IModificationAuditedTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol ISoftDelete => compilation.GetTypeByMetadataName(AuditDefaults.ISoftDeleteTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IHasDeleter => compilation.GetTypeByMetadataName(AuditDefaults.IHasDeleterTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IHasDeletionTime => compilation.GetTypeByMetadataName(AuditDefaults.IHasDeletionTimeTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IDeletionAudited => compilation.GetTypeByMetadataName(AuditDefaults.IDeletionAuditedTypeFullQualifiedMetadataName)!;
    public INamedTypeSymbol IFullAudited => compilation.GetTypeByMetadataName(AuditDefaults.IFullAuditedTypeFullQualifiedMetadataName)!;
}
