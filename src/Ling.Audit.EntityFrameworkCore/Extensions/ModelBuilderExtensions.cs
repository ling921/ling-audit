using Ling.Audit.EntityFrameworkCore.Internal;
using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Ling.Audit.EntityFrameworkCore.Internal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
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
    /// <param name="anonymousOperations">
    /// Allowed anonymous operations to the table this entity is mapped to.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static EntityTypeBuilder IsAuditable(this EntityTypeBuilder entityTypeBuilder, EntityOperationType anonymousOperations = EntityOperationType.None)
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);

        entityTypeBuilder.Metadata.SetAnnotation(Constants.AuditableAnnotationName, true);
        entityTypeBuilder.Metadata.SetAuditMetadata(new AuditMetadata { AnonymousOperations = anonymousOperations });

        return entityTypeBuilder;
    }

    /// <inheritdoc cref="IsAuditable(EntityTypeBuilder, EntityOperationType)"/>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    public static EntityTypeBuilder<TEntity> IsAuditable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, EntityOperationType anonymousOperations = EntityOperationType.None)
        where TEntity : class
    {
        EntityTypeBuilder builder = entityTypeBuilder;

        builder.IsAuditable(anonymousOperations);

        return entityTypeBuilder;
    }

    /// <summary>
    /// Configures whether to apply auditing to the column.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="enabled">Whether to audit the column, defaults to <see langword="false"/>.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder IsAuditable(this PropertyBuilder propertyBuilder, bool enabled = false)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        propertyBuilder.Metadata.SetAnnotation(Constants.AuditableAnnotationName, enabled);
        return propertyBuilder;
    }

    /// <inheritdoc cref="IsAuditable(PropertyBuilder, bool)"/>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    public static PropertyBuilder<TProperty> IsAuditable<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, bool enabled = false)
    {
        PropertyBuilder builder = propertyBuilder;

        builder.IsAuditable(enabled);

        return propertyBuilder;
    }

    /// <summary>
    /// Configures the table names for audit log entities.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    /// <param name="entityChangeLogTableName">The table name for entity change logs.</param>
    /// <param name="fieldChangeLogTableName">The table name for field change logs.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    /// <exception cref="InvalidOperationException">Thrown when audit log entity types are not found in the model.</exception>
    /// <exception cref="ArgumentException">Thrown when table names is null or whitespaces.</exception>
    public static ModelBuilder ConfigureAuditLogTableNames(
        this ModelBuilder builder,
        string entityChangeLogTableName,
        string fieldChangeLogTableName)
    {
        ThrowHelper.ThrowIfNull(builder);
        ThrowHelper.ThrowIfNullOrWhiteSpace(entityChangeLogTableName);
        ThrowHelper.ThrowIfNullOrWhiteSpace(fieldChangeLogTableName);

        var entityLogType = builder.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.IsGenericType && e.ClrType.GetGenericTypeDefinition() == typeof(AuditEntityChangeLog<>))
            ?.ClrType
            ?? throw new InvalidOperationException("'AuditEntityChangeLog<TKey>' entity type not found. Please ensure you have called 'UseAudit()' when configuring your DbContext.");

        builder.Entity(entityLogType)
            .ToTable(entityChangeLogTableName);

        var fieldLogType = builder.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType == typeof(AuditFieldChangeLog))
            ?.ClrType
            ?? throw new InvalidOperationException(
                "'AuditFieldChangeLog' entity type not found. Please ensure you have called 'UseAudit()' when configuring your DbContext.");

        builder.Entity(fieldLogType)
            .ToTable(fieldChangeLogTableName);

        return builder;
    }

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
    /// <param name="options">Audit options to configure the behavior of audited entities.</param>
    /// <param name="logger">Logger to log the configuration process.</param>
    /// <returns>The identity type of user for current <see cref="DbContext"/>.</returns>
    internal static Type? ConfigureAuditableEntities(this ModelBuilder builder, AuditOptions options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        Type? userIdType = options.UserIdType;
        logger.LogInformation("Starting to configure auditable entities. Initial user ID type: {UserIdType}",
            userIdType?.GetFriendlyName() ?? "null");

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var metadata = new AuditMetadata();
            var entityClrType = entityType.ClrType;
            var entityName = entityClrType.GetFriendlyName();

            logger.LogDebug("Configuring audit properties for entity: {EntityName}", entityName);

            // Configure creation audit properties
            if (entityType.HasProperty(Constants.CreatedAt, out var createdAtType))
            {
                ValidatePropertyType(entityClrType, Constants.CreatedAt, createdAtType, typeof(DateTimeOffset));
                ConfigureProperty(builder, entityClrType, createdAtType, Constants.CreatedAt, 991, options.Comments.CreatedAt);
                metadata.HasCreatedAt = true;
            }

            if (entityType.HasProperty(Constants.CreatedBy, out var createdByType))
            {
                ValidateAndUpdateUserIdType(entityClrType, Constants.CreatedBy, createdByType, ref userIdType);
                ConfigureProperty(builder, entityClrType, createdByType, Constants.CreatedBy, 992, options.Comments.CreatedBy);
                metadata.HasCreatedBy = true;
                metadata.UserIdType = createdByType;
            }

            // Configure modification audit properties
            if (entityType.HasProperty(Constants.ModifiedAt, out var modifiedAtType))
            {
                ValidatePropertyType(entityClrType, Constants.ModifiedAt, modifiedAtType, typeof(DateTimeOffset?));
                ConfigureProperty(builder, entityClrType, modifiedAtType, Constants.ModifiedAt, 994, options.Comments.ModifiedAt);
                metadata.HasModifiedAt = true;
            }

            if (entityType.HasProperty(Constants.ModifiedBy, out var modifiedByType))
            {
                ValidateAndUpdateUserIdType(entityClrType, Constants.ModifiedBy, modifiedByType, ref userIdType);
                ConfigureProperty(builder, entityClrType, modifiedByType, Constants.ModifiedBy, 995, options.Comments.ModifiedBy);
                metadata.HasModifiedBy = true;
                metadata.UserIdType = modifiedByType;
            }

            // Configure deletion audit properties
            if (entityType.HasProperty(Constants.IsDeleted, out var isDeletedType))
            {
                ValidatePropertyType(entityClrType, Constants.IsDeleted, isDeletedType, typeof(bool));
                ConfigureProperty(builder, entityClrType, isDeletedType, Constants.IsDeleted, 997, options.Comments.IsDeleted);
                metadata.HasIsDeleted = true;
            }

            if (entityType.HasProperty(Constants.DeletedAt, out var deletedAtType))
            {
                ValidatePropertyType(entityClrType, Constants.DeletedAt, deletedAtType, typeof(DateTimeOffset?));
                ConfigureProperty(builder, entityClrType, deletedAtType, Constants.DeletedAt, 998, options.Comments.DeletedAt);
                metadata.HasDeletedAt = true;
            }

            if (entityType.HasProperty(Constants.DeletedBy, out var deletedByType))
            {
                ValidateAndUpdateUserIdType(entityClrType, Constants.DeletedBy, deletedByType, ref userIdType);
                ConfigureProperty(builder, entityClrType, deletedByType, Constants.DeletedBy, 999, options.Comments.DeletedBy);
                metadata.HasDeletedBy = true;
                metadata.UserIdType = deletedByType;
            }

            entityType.SetAuditMetadata(metadata);

            logger.LogDebug("Configured audit metadata for {EntityName}: CreatedAt={HasCreatedAt}, CreatedBy={HasCreatedBy}, ModifiedAt={HasModifiedAt}, ModifiedBy={HasModifiedBy}, IsDeleted={HasIsDeleted}, DeletedAt={HasDeletedAt}, DeletedBy={HasDeletedBy}",
                entityName,
                metadata.HasCreatedAt,
                metadata.HasCreatedBy,
                metadata.HasModifiedAt,
                metadata.HasModifiedBy,
                metadata.HasIsDeleted,
                metadata.HasDeletedAt,
                metadata.HasDeletedBy);

#if NET6_0
            if (IsDesignTime)
#else
            if (EF.IsDesignTime)
#endif
            {
                logger.LogDebug("Running in design time, removing audit metadata for entity {EntityName}", entityName);
                entityType.RemoveAnnotation(Constants.MetadataAnnotationName);
            }
        }

        logger.LogInformation("Completed configuring auditable entities. Final user ID type: {UserIdType}",
            userIdType?.GetFriendlyName() ?? "null");

        return userIdType;
    }

    /// <summary>
    /// Sets the audit metadata for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type to set the audit metadata for.</param>
    /// <param name="metadata">The audit metadata to set.</param>
    internal static void SetAuditMetadata(this IMutableEntityType entityType, AuditMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(metadata);

        if (entityType.FindAnnotation(Constants.MetadataAnnotationName)?.Value is AuditMetadata original)
        {
            entityType.SetAnnotation(Constants.MetadataAnnotationName, original | metadata);
        }
        else
        {
            entityType.SetAnnotation(Constants.MetadataAnnotationName, metadata);
        }
    }

    /// <summary>
    /// Gets the audit metadata for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type to get the audit metadata for.</param>
    /// <returns>The audit metadata for the specified entity type.</returns>
    internal static AuditMetadata GetAuditMetadata(this IReadOnlyAnnotatable entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return entityType.FindAnnotation(Constants.MetadataAnnotationName)?.Value is AuditMetadata metadata
            ? metadata
            : new AuditMetadata();
    }

    /// <summary>
    /// Gets whether auditing is included for the specified annotatable.
    /// </summary>
    /// <param name="annotatable">The annotatable to check.</param>
    /// <returns><see langword="true"/> if auditing is included; otherwise, <see langword="false"/>.</returns>
    internal static bool GetAuditInclude(this IReadOnlyAnnotatable annotatable)
    {
        ArgumentNullException.ThrowIfNull(annotatable);

        var value = annotatable.FindAnnotation(Constants.AuditableAnnotationName)?.Value;
        return annotatable switch
        {
            IEntityType => value is true,   // Entity auditable defaults to false
            IProperty => value is false,    // Entity property or field auditable defaults to true
            _ => throw new InvalidOperationException()
        };
    }

    #endregion Internal Methods

    #region Private Methods

    /// <summary>
    /// Checks if the entity type has the specified property.
    /// </summary>
    /// <param name="entityType">The entity type to check.</param>
    /// <param name="propertyName">The name of the property to check for.</param>
    /// <returns><see langword="true"/> if the entity type has the specified property; otherwise, <see langword="false"/>.</returns>
    private static bool HasProperty(this IMutableEntityType entityType, string propertyName)
    {
        return entityType.FindProperty(propertyName) is not null;
    }

    /// <summary>
    /// Checks if the entity type has the specified property and gets its type.
    /// </summary>
    /// <param name="entityType">The entity type to check.</param>
    /// <param name="propertyName">The name of the property to check for.</param>
    /// <param name="type">When this method returns, contains the type of the property, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the entity type has the specified property; otherwise, <see langword="false"/>.</returns>
    private static bool HasProperty(this IMutableEntityType entityType, string propertyName, [NotNullWhen(true)] out Type? type)
    {
        type = entityType.FindProperty(propertyName)?.ClrType;
        return type is not null;
    }

    /// <summary>
    /// Configures the specified property for the specified entity type.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    /// <param name="entityType">The entity type to configure the property for.</param>
    /// <param name="propertyType">The type of the property to configure.</param>
    /// <param name="propertyName">The name of the property to configure.</param>
    /// <param name="columnOrder">The order of the column in the table.</param>
    /// <param name="comment">The comment for the property.</param>
    private static void ConfigureProperty(
        ModelBuilder builder,
        Type entityType,
        Type propertyType,
        string propertyName,
        int columnOrder,
        string comment)
    {
        builder.Entity(entityType)
            .Property(propertyType, propertyName)
            .HasColumnOrder(columnOrder)
            .HasComment(comment);
    }

    /// <summary>
    /// Validates the type of the specified property.
    /// </summary>
    /// <param name="entityClrType">The CLR type of the entity.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <param name="propertyType">The type of the property to validate.</param>
    /// <param name="targetType">The expected type of the property.</param>
    /// <exception cref="InvalidOperationException">Thrown when the property type does not match the expected type.</exception>
    private static void ValidatePropertyType(Type entityClrType, string propertyName, Type propertyType, Type targetType)
    {
        if (!propertyType.IsAssignableTo(targetType))
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' in entity '{entityClrType.GetFriendlyName()}' must be of type {targetType.Name}");
        }
    }

    /// <summary>
    /// Validates and updates the user ID type.
    /// </summary>
    /// <param name="entityClrType">The CLR type of the entity.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <param name="propertyType">The type of the property to validate.</param>
    /// <param name="userIdType">The user ID type to update.</param>
    /// <exception cref="InvalidOperationException">Thrown when the user ID type does not match the expected type.</exception>
    private static void ValidateAndUpdateUserIdType(Type entityClrType, string propertyName, Type propertyType, ref Type? userIdType)
    {
        if (userIdType is null)
        {
            userIdType = propertyType;
        }
        else if (userIdType != propertyType)
        {
            throw new InvalidOperationException(
                $"User ID type mismatch in entity '{entityClrType.GetFriendlyName()}.{propertyName}'. Expected '{userIdType.GetFriendlyName()}' but found '{propertyType.GetFriendlyName()}'.");
        }
    }

    #endregion Private Methods
}
