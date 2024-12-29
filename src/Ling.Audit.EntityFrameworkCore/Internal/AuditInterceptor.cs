using Ling.Audit.EntityFrameworkCore.Extensions;
using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Ling.Audit.EntityFrameworkCore.Internal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore.Internal;

internal sealed class AuditInterceptor<TUserId> : SaveChangesInterceptor, IDisposable
{
    private IReadOnlyList<AuditEntry>? _entries;

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        InternalSavingChanges(eventData);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var count = InternalSavedChanges(eventData);
        if (count > 0)
        {
            eventData.Context!.SaveChanges();
        }
        return result;
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        InternalSavingChanges(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result, CancellationToken
        cancellationToken = default)
    {
        var count = InternalSavedChanges(eventData);
        if (count > 0)
        {
            await eventData.Context!.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    internal void InternalSavingChanges(DbContextEventData eventData)
    {
        var context = eventData.Context;

        if (context is null) return;

        var userProvider = context.GetService<IAuditContextProvider<TUserId>>();
        var logger = context.GetService<ILoggerFactory>().CreateLogger(GetType());
        var options = context.GetAuditOptions();
        var entries = new List<AuditEntry>();
        var now = DateTimeOffset.Now;
        var isUserIdDefaultValue = EqualityComparer<TUserId>.Default.Equals(userProvider.Id, default);

        foreach (var entityEntry in context.ChangeTracker.Entries())
        {
            var entityType = entityEntry.Metadata.ClrType;
            var metadata = entityEntry.Metadata.GetAuditMetadata();
            var eventType = AuditEventType.None;
            var userId = isUserIdDefaultValue ? null : userProvider.Id.ConvertToTargetType(metadata.UserIdType);

            if (typeof(TUserId).IsSameTypeIgnoreNullableTo(metadata.UserIdType))
            {
                logger.LogError(
                    "The type of 'TUserId' configured is '{UserIdType}', but entity's user key type is '{entityType}'.",
                    typeof(TUserId),
                    metadata.UserIdType);
                throw new InvalidOperationException($"The type of 'TUserId' configured is not match entity [{entityType}].");
            }

            switch (entityEntry.State)
            {
                case EntityState.Deleted:
                    if (metadata.HasDeletedAt)
                    {
                        entityEntry.Property(Constants.DeletedAt).CurrentValue = now;
                    }

                    if (metadata.HasDeletedBy)
                    {
                        if (!options.AllowAnonymousDelete &&
                            !metadata.IsAllowedAnonymousOperate(EntityOperationType.Delete) &&
                            isUserIdDefaultValue)
                        {
                            logger.LogError("Not allowed to delete entity '{entityType}' with anonymous user.", entityType);
                            throw new InvalidOperationException($"Anonymous deletion of '{entityType.GetFriendlyName()}' is not allowed.");
                        }
                        entityEntry.Property(Constants.DeletedBy).CurrentValue = userId;
                    }

                    if (metadata.HasIsDeleted)
                    {
                        entityEntry.Property(Constants.IsDeleted).CurrentValue = true;
                        entityEntry.State = EntityState.Modified;
                        eventType = AuditEventType.SoftDelete;
                    }
                    else
                    {
                        eventType = AuditEventType.Delete;
                    }
                    break;

                case EntityState.Modified:
                    if (metadata.HasModifiedAt)
                    {
                        entityEntry.Property(Constants.ModifiedAt).CurrentValue = now;
                    }
                    if (metadata.HasModifiedBy)
                    {
                        if (!options.AllowAnonymousModify &&
                            !metadata.IsAllowedAnonymousOperate(EntityOperationType.Update) &&
                            isUserIdDefaultValue)
                        {
                            logger.LogError("Not allowed to modify entity '{entityType}' with anonymous user.", entityType);
                            throw new InvalidOperationException($"Anonymous modification of {entityType.GetFriendlyName()} is not allowed.");
                        }

                        entityEntry.Property(Constants.ModifiedBy).CurrentValue = userId;
                    }
                    eventType = GetModifiedType(entityEntry, metadata);
                    break;

                case EntityState.Added:
                    if (metadata.HasCreatedAt)
                    {
                        entityEntry.Property(Constants.CreatedAt).CurrentValue = now;
                    }
                    if (metadata.HasCreatedBy)
                    {
                        if (!options.AllowAnonymousCreate &&
                            !metadata.IsAllowedAnonymousOperate(EntityOperationType.Create) &&
                            isUserIdDefaultValue)
                        {
                            logger.LogError("Not allowed to create entity '{entityType}' with anonymous user.", entityType);
                            throw new InvalidOperationException($"Anonymous creation of {entityType.GetFriendlyName()} is not allowed.");
                        }
                        entityEntry.Property(Constants.CreatedBy).CurrentValue = userId;
                    }
                    eventType = AuditEventType.Create;
                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }

            if (eventType is not AuditEventType.None && TryGetAuditEntry(entityEntry, out var auditEntry))
            {
                auditEntry.EventType = eventType;
                entries.Add(auditEntry);
            }
        }

        if (!AppContext.TryGetSwitch(AuditDefaults.DisabledSwitchKey, out var disabled) || !disabled)
        {
            _entries = entries;
        }
    }

    internal int InternalSavedChanges(SaveChangesCompletedEventData eventData)
    {
        var context = eventData.Context;

        if (context is null || (AppContext.TryGetSwitch(AuditDefaults.DisabledSwitchKey, out var disabled) && disabled)) return 0;

        var options = context.GetAuditOptions();
        var logger = context.GetService<ILoggerFactory>().CreateLogger(GetType());
        if (_entries is null)
        {
            logger.LogWarning("Unable to get entry information when saved changes.");
            throw new InvalidOperationException("Unable to get entry information before saving changes.");
        }

        var userProvider = context.GetService<IAuditContextProvider<TUserId>>();
        var logs = _entries
            .Select(i => new AuditEntityChangeLog<TUserId>
            {
                DatabaseSchema = i.Schema,
                TableName = i.Table,
                EntityKey = i.PrimaryKey,
                EntityTypeName = i.EntityType,
                EventType = i.EventType,
                EventTime = DateTimeOffset.Now,
                UserId = userProvider.Id,
                UserName = userProvider.Name,
                Details = i.Properties.ConvertAll(j => new AuditFieldChangeLog
                {
                    FieldName = i.EntityType + '.' + j.PropertyName,
                    OriginalValue = GetStringValue(j.OriginalValue),
                    NewValue = GetStringValue(j.NewValue),
                }),
            })
            .ToList();

        int entityChangedCount = 0, fieldChangedCount = 0;
        foreach (var log in logs)
        {
            if (log.Details.Count > 0 || options.AuditNoFieldChangeEntity)
            {
                context.Add(log);
                entityChangedCount++;
                fieldChangedCount += log.Details.Count;
            }
        }

        logger.LogInformation(
            "Add {EntityCount} entity changed and {FieldCount} field changed audit logs to {DbContextType}[{ContextId}].",
            entityChangedCount,
            fieldChangedCount,
            context.GetType().GetFriendlyName(),
            context.ContextId);
        return entityChangedCount + fieldChangedCount;
    }

    private static bool TryGetAuditEntry(EntityEntry entityEntry, [NotNullWhen(true)] out AuditEntry? auditEntry)
    {
        var entityInclude = entityEntry.Metadata.GetAuditInclude();
        if (!entityInclude)
        {
            auditEntry = null;
            return false;
        }

        auditEntry = new AuditEntry(entityEntry);

        foreach (var propertyEntry in entityEntry.Properties)
        {
            var propertyInclude = propertyEntry.Metadata.GetAuditInclude();
            if (propertyInclude && !Constants.PropertyNames.Contains(propertyEntry.Metadata.Name))
            {
                if (entityEntry.State is not EntityState.Added && Equals(propertyEntry.OriginalValue, propertyEntry.CurrentValue))
                {
                    continue;
                }

                auditEntry.Properties.Add(new AuditPropertyEntry
                {
                    PropertyName = propertyEntry.Metadata.Name,
                    OriginalValue = entityEntry.State == EntityState.Added ? null : propertyEntry.OriginalValue,
                    NewValue = propertyEntry.CurrentValue
                });
            }
        }

        return true;
    }

    private static AuditEventType GetModifiedType(EntityEntry entityEntry, AuditMetadata metadata)
    {
        if (metadata.HasIsDeleted)
        {
            var originalValue = (bool)entityEntry.Property(Constants.IsDeleted).OriginalValue!;
            var newValue = (bool)entityEntry.Property(Constants.IsDeleted).CurrentValue!;
            if (originalValue != newValue)
            {
                return newValue ? AuditEventType.SoftDelete : AuditEventType.Recovery;
            }
        }
        return AuditEventType.Modify;
    }

    private static string? GetStringValue(object? value)
    {
        if (value == null) return null;

        try
        {
            if (value is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else if (value is DateTimeOffset dto)
            {
                return dto.ToString("yyyy-MM-dd HH:mm:ss zzz");
            }

            var converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(typeof(string)))
            {
                return converter.ConvertToString(value);
            }
        }
        catch { }

        return JsonSerializer.Serialize(value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _entries = null;
    }
}
