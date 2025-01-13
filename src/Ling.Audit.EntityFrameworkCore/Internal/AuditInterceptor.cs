using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Ling.Audit.EntityFrameworkCore.Internal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Ling.Audit.EntityFrameworkCore.Internal;

internal sealed class AuditInterceptor<TUserId> : SaveChangesInterceptor, IDisposable
{
    private IReadOnlyList<AuditEntityEntry>? _entries;

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
        var entries = new List<AuditEntityEntry>();
        var now = DateTimeOffset.Now;
        var isUserIdDefaultValue = EqualityComparer<TUserId>.Default.Equals(userProvider.Id, default);

        foreach (var entityEntry in context.ChangeTracker.Entries())
        {
            var entityType = entityEntry.Metadata.ClrType;
            var metadata = entityEntry.Metadata.GetAuditMetadata();
            var eventType = AuditEventType.None;
            var userId = userProvider.Id;

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
                            !metadata.AllowsAnonymousOperation(EntityOperationType.Delete) &&
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
                            !metadata.AllowsAnonymousOperation(EntityOperationType.Update) &&
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
                            !metadata.AllowsAnonymousOperation(EntityOperationType.Create) &&
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

        if (!AppContext.TryGetSwitch(AuditDefaults.DisableAuditingSwitch, out var disabled) || !disabled)
        {
            _entries = entries;
        }
    }

    internal int InternalSavedChanges(SaveChangesCompletedEventData eventData)
    {
        var context = eventData.Context;

        if (context is null || (AppContext.TryGetSwitch(AuditDefaults.DisableAuditingSwitch, out var disabled) && disabled)) return 0;

        var options = context.GetAuditOptions();
        var logger = context.GetService<ILoggerFactory>().CreateLogger(GetType());
        if (_entries is null)
        {
            logger.LogWarning("Unable to get entry information when saved changes.");
            throw new InvalidOperationException("Unable to get entry information before saving changes.");
        }

        var userProvider = context.GetService<IAuditContextProvider<TUserId>>();
        var serializer = context.GetService<IPropertySerializer>();

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
                IPAddress = userProvider.IPAddress,
                ClientName = userProvider.ClientName,
                Details = i.Properties.ConvertAll(j => new AuditFieldChangeLog
                {
                    FieldName = i.EntityType + '.' + j.Name,
                    OriginalValue = serializer.Serialize(j.OriginalValue, j.ValueType),
                    NewValue = serializer.Serialize(j.NewValue, j.ValueType),
                    ValueType = j.ValueType.Name,
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

    private static bool TryGetAuditEntry(EntityEntry entityEntry, [NotNullWhen(true)] out AuditEntityEntry? auditEntry)
    {
        var entityInclude = entityEntry.Metadata.GetAuditInclude();
        if (!entityInclude)
        {
            auditEntry = null;
            return false;
        }

        auditEntry = new AuditEntityEntry(entityEntry);

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
                    Name = propertyEntry.Metadata.Name,
                    ValueType = propertyEntry.Metadata.ClrType,
                    OriginalValue = entityEntry.State is EntityState.Added ? null : propertyEntry.OriginalValue,
                    NewValue = entityEntry.State is EntityState.Deleted ? null : propertyEntry.CurrentValue
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

    /// <inheritdoc />
    public void Dispose()
    {
        _entries = null;
    }
}
