using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using System.Reflection;

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

    /// <summary>
    /// Convention to process <see cref="AuditableAttribute"/> on entity types.
    /// </summary>
    private sealed class AuditIncludeAttributeConvention :
#if NET8_0_OR_GREATER
        TypeAttributeConventionBase<AuditableAttribute>
#else
    EntityTypeAttributeConventionBase<AuditableAttribute>
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
            AuditableAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            entityTypeBuilder.Metadata.SetAnnotation(Constants.AuditableAnnotationName, true);
        }
    }

    /// <summary>
    /// Convention to process <see cref="NotAuditedAttribute"/> on properties and fields.
    /// </summary>
    private sealed class AuditIgnoreAttributeConvention : PropertyAttributeConventionBase<NotAuditedAttribute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditIgnoreAttributeConvention"/> class.
        /// </summary>
        /// <param name="dependencies">The dependencies required for this convention.</param>
        public AuditIgnoreAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc/>
        protected override void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            NotAuditedAttribute attribute,
            MemberInfo clrMember,
            IConventionContext context)
        {
            propertyBuilder.Metadata.SetAnnotation(Constants.AuditableAnnotationName, false);
        }
    }
}
