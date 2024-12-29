using Ling.Audit.EntityFrameworkCore.Extensions;
using Ling.Audit.EntityFrameworkCore.Internal;
using Ling.Audit.EntityFrameworkCore.Internal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Extension methods for building model.
/// </summary>
public static class ModelBuilderExtensions
{
    #region Public Methods

    /// <summary>
    /// Setup a global query filter for entities has "IsDeleted" property.
    /// </summary>
    /// <param name="builder">Entity model builder.</param>
    public static void SetupSoftDeleteQueryFilter(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;

            if (entityType.HasProperty(Constants.IsDeleted))
            {
                var typeParameter = Expression.Parameter(type, "e");
                var propertyParameter = Expression.Property(typeParameter, Constants.IsDeleted);
                var lambda = Expression.Lambda(Expression.Not(propertyParameter), typeParameter);

                builder.Entity(type).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Configures enable auditing to be applied to the table this entity is mapped to.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="allowAnonymousOperate">
    /// Allowed anonymous operations to the table this entity is mapped to.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder IsAuditable(this EntityTypeBuilder entityTypeBuilder, EntityOperationType allowAnonymousOperate = EntityOperationType.None)
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);

        entityTypeBuilder.Metadata.SetAnnotation(Constants.IncludeAnnotationName, true);
        entityTypeBuilder.Metadata.SetAuditMetadata(new AuditMetadata { AllowAnonymousOperate = allowAnonymousOperate });

        return entityTypeBuilder;
    }

    /// <inheritdoc cref="IsAuditable(EntityTypeBuilder, EntityOperationType)"/>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    public static EntityTypeBuilder<TEntity> IsAuditable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, EntityOperationType allowAnonymousOperate = EntityOperationType.None)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).IsAuditable(allowAnonymousOperate);

    /// <summary>
    /// Configures whether to apply auditing to the column.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="enabled">Whether to audit the column, defaults to <see langword="false"/>.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder IsAuditable(this PropertyBuilder propertyBuilder, bool enabled = false)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        propertyBuilder.Metadata.SetAnnotation(Constants.IncludeAnnotationName, enabled);
        return propertyBuilder;
    }

    /// <inheritdoc cref="IsAuditable(PropertyBuilder, bool)"/>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    public static PropertyBuilder<TProperty> IsAuditable<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, bool enabled = false)
        => (PropertyBuilder<TProperty>)((PropertyBuilder)propertyBuilder).IsAuditable(enabled);

    #endregion Public Methods

    #region Internal Methods

#if NET6_0

    /// <summary>
    /// Gets whether the current program is running at design time.
    /// </summary>
    internal static bool IsDesignTime => Path.GetFileName(Environment.GetCommandLineArgs().FirstOrDefault()) == "ef.dll";

#endif

    /// <summary>
    /// Configure properties of auditable entities.
    /// </summary>
    /// <param name="builder">Entity model builder.</param>
    /// <param name="options"></param>
    /// <returns>The identity type of user for current <see cref="DbContext"/>.</returns>
    internal static Type? ConfigureAuditableEntities(this ModelBuilder builder, AuditOptions options)
    {
        Type? userIdType = options.UserIdType;
        Type? entityClrType = null;
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;
            var auditMetadata = entityType.GetAuditMetadata();

            if (entityType.HasProperty(Constants.Id, out var propertyType))
            {
                builder.Entity(type)
                    .Property(propertyType, Constants.Id)
                    .HasColumnOrder(0)
                    .HasComment(options.Comments.Id);
            }

            if (entityType.HasProperty(Constants.CreatedAt, out propertyType))
            {
                builder.Entity(type)
                    .Property(propertyType, Constants.CreatedAt)
                    .HasColumnOrder(991)
                    .HasComment(options.Comments.CreatedAt);

                CheckDateTimeOffset(type, Constants.CreatedAt, propertyType);
                auditMetadata.HasCreatedAt = true;
            }
            if (entityType.HasProperty(Constants.CreatedBy, out var cUserIdType))
            {
                builder.Entity(type)
                    .Property(cUserIdType, Constants.CreatedBy)
                    .HasColumnOrder(992)
                    .HasComment(options.Comments.CreatedBy);

                CheckAndAssign(ref entityClrType, ref userIdType, type, cUserIdType);
                auditMetadata.UserIdType = cUserIdType;
                auditMetadata.HasCreatedBy = true;
            }

            if (entityType.HasProperty(Constants.ModifiedAt, out propertyType))
            {
                builder.Entity(type)
                    .Property(propertyType, Constants.ModifiedAt)
                    .HasColumnOrder(994)
                    .HasComment(options.Comments.ModifiedAt);

                CheckDateTimeOffset(type, Constants.ModifiedAt, propertyType);
                auditMetadata.HasModifiedAt = true;
            }
            if (entityType.HasProperty(Constants.ModifiedBy, out var mUserIdType))
            {
                builder.Entity(type)
                    .Property(mUserIdType, Constants.ModifiedBy)
                    .HasColumnOrder(995)
                    .HasComment(options.Comments.ModifiedBy);

                CheckAndAssign(ref entityClrType, ref userIdType, type, mUserIdType);
                auditMetadata.UserIdType = mUserIdType;
                auditMetadata.HasModifiedBy = true;
            }

            if (entityType.HasProperty(Constants.IsDeleted, out propertyType))
            {
                builder.Entity(type)
                    .Property(propertyType, Constants.IsDeleted)
                    .HasColumnOrder(997)
                    .HasComment(options.Comments.IsDeleted);

                CheckBoolean(type, Constants.IsDeleted, propertyType);
                auditMetadata.HasIsDeleted = true;
            }
            if (entityType.HasProperty(Constants.DeletedAt, out propertyType))
            {
                builder.Entity(type)
                    .Property(propertyType, Constants.DeletedAt)
                    .HasColumnOrder(998)
                    .HasComment(options.Comments.DeletedAt);

                CheckDateTimeOffset(type, Constants.DeletedAt, propertyType);
                auditMetadata.HasDeletedAt = true;
            }
            if (entityType.HasProperty(Constants.DeletedBy, out var dUserIdType))
            {
                builder.Entity(type)
                    .Property(dUserIdType, Constants.DeletedBy)
                    .HasColumnOrder(999)
                    .HasComment(options.Comments.DeletedBy);

                CheckAndAssign(ref entityClrType, ref userIdType, type, dUserIdType);
                auditMetadata.UserIdType = dUserIdType;
                auditMetadata.HasDeletedBy = true;
            }

            entityType.SetAuditMetadata(auditMetadata);

#if NET6_0
            if (IsDesignTime)
#else
            if (EF.IsDesignTime)
#endif
            {
                Debug.WriteLine("Currently running in design time.");
                entityType.RemoveAnnotation(Constants.MetadataAnnotationName);
            }
        }

        return userIdType;
    }

    internal static bool GetAuditInclude(this IReadOnlyAnnotatable annotatable)
    {
        ArgumentNullException.ThrowIfNull(annotatable);

        var value = annotatable.FindAnnotation(Constants.IncludeAnnotationName)?.Value;
        return annotatable switch
        {
            IEntityType => value is not null && (bool)value, // Entity auditable defaults to false
            IProperty => value is null || (bool)value, // Entity property or field auditable defaults to true
            _ => throw new InvalidOperationException()
        };
    }

    internal static void SetAuditMetadata(this IMutableEntityType entityType, AuditMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(metadata);

        if (entityType.FindAnnotation(Constants.MetadataAnnotationName)?.Value is AuditMetadata original)
        {
            original.Append(metadata);
            entityType.SetAnnotation(Constants.MetadataAnnotationName, original);
        }
        else
        {
            entityType.SetAnnotation(Constants.MetadataAnnotationName, metadata);
        }
    }

    internal static AuditMetadata GetAuditMetadata(this IReadOnlyAnnotatable entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return entityType.FindAnnotation(Constants.MetadataAnnotationName)?.Value is AuditMetadata metadata
            ? metadata
            : new AuditMetadata();
    }

    #endregion Internal Methods

    #region Private Methods

    private static void CheckAndAssign(ref Type? entityType, ref Type? userIdType, Type currentEntityType, Type currenTUserIdtype)
    {
        if (userIdType is null)
        {
            entityType = currentEntityType;
            userIdType = currenTUserIdtype;
        }
        else if (userIdType != currenTUserIdtype)
        {
            const string msg = "All audit entities in the same 'DbContext' cannot have different user's identity types.";
            var desc = entityType == currentEntityType
                ? string.Format(
                    "Entity type '{0}' has both type '{1}' and type '{2}' as user's identity type.",
                    currentEntityType.GetFriendlyName(),
                    userIdType.GetFriendlyName(),
                    currenTUserIdtype.GetFriendlyName())
                : string.Format(
                    "Entity type '{0}' has type '{1}' as user key type but entity '{2}' has type '{3}' as user's identity type.",
                    entityType!.GetFriendlyName(),
                    userIdType.GetFriendlyName(),
                    currentEntityType.GetFriendlyName(),
                    currenTUserIdtype.GetFriendlyName());
            throw new InvalidOperationException($"{msg} {desc}");
        }
    }

    private static void CheckDateTimeOffset(Type currentEntityType, string propertyName, Type propertyType)
    {
        if (!propertyType.IsAssignableFrom(typeof(DateTimeOffset)))
        {
            throw new InvalidOperationException(string.Format(
                "Type '{2}' of member '{1}' in entity type '{0}' is neither 'DateTimeOffset' nor 'DateTimeOffset?'.",
                currentEntityType.GetFriendlyName(),
                propertyName,
                propertyType.GetFriendlyName()));
        }
    }

    private static void CheckBoolean(Type currentEntityType, string propertyName, Type propertyType)
    {
        if (!propertyType.IsAssignableFrom(typeof(bool)))
        {
            throw new InvalidOperationException(string.Format(
                "Type '{2}' of member '{1}' in entity type '{0}' is not 'bool'.",
                currentEntityType.GetFriendlyName(),
                propertyName,
                propertyType.GetFriendlyName()));
        }
    }

    private static bool HasProperty(this IMutableEntityType entityType, string propertyName)
    {
        return entityType.FindProperty(propertyName) is not null;
    }

    private static bool HasProperty(this IMutableEntityType entityType, string propertyName, [NotNullWhen(true)] out Type? type)
    {
        type = entityType.FindProperty(propertyName)?.ClrType;
        return type is not null;
    }

    #endregion Private Methods
}
