using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore.Internal;

/// <summary>
/// Convention to process <see cref="AuditIncludeAttribute"/> on entity types.
/// </summary>
internal sealed class AuditIncludeAttributeConvention :
#if NET8_0_OR_GREATER
    TypeAttributeConventionBase<AuditIncludeAttribute>
#else
    EntityTypeAttributeConventionBase<AuditIncludeAttribute>
#endif
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditIncludeAttributeConvention"/> class.
    /// </summary>
    /// <param name="dependencies">The dependencies required for this convention.</param>
    public AuditIncludeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc/>
    protected override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        AuditIncludeAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        entityTypeBuilder.Metadata.SetAnnotation(Constants.AuditableAnnotationName, true);
    }
}
