using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore.Internal;

/// <summary>
/// Plugin to add audit-related conventions to EF Core's convention set.
/// </summary>
internal sealed class AuditConventionSetPlugin : IConventionSetPlugin
{
    private readonly ProviderConventionSetBuilderDependencies _dependencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditConventionSetPlugin"/> class.
    /// </summary>
    /// <param name="dependencies">The dependencies required for this plugin.</param>
    public AuditConventionSetPlugin(ProviderConventionSetBuilderDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <inheritdoc/>
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.EntityTypeAddedConventions.Add(new AuditIncludeAttributeConvention(_dependencies));
        conventionSet.PropertyAddedConventions.Add(new AuditIgnoreAttributeConvention(_dependencies));
        return conventionSet;
    }
}
