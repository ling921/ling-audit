using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using System.Reflection;

namespace Ling.Audit.EntityFrameworkCore.Internal;

/// <summary>
/// Convention to process <see cref="AuditIgnoreAttribute"/> on properties and fields.
/// </summary>
internal sealed class AuditIgnoreAttributeConvention : PropertyAttributeConventionBase<AuditIgnoreAttribute>
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
        AuditIgnoreAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        propertyBuilder.Metadata.SetAnnotation(Constants.AuditableAnnotationName, false);
    }
}
