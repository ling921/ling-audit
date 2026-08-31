using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Ling.Audit.EntityFrameworkCore.Internal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ling.Audit.EntityFrameworkCore.Internal;

internal sealed class AuditSaveChangesInterceptor<TUserId> : SaveChangesInterceptor
{
    private readonly ConditionalWeakTable<DbContext, SaveState> _states = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        InternalSavingChangesAsync(eventData, default).ConfigureAwait(false).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await InternalSavingChangesAsync(eventData, cancellationToken).ConfigureAwait(false);
        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        InternalSavedChanges(eventData, async: false, default).ConfigureAwait(false).GetAwaiter().GetResult();
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await InternalSavedChanges(eventData, async: true, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is { } context)
        {
            _states.Remove(context);
        }

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            _states.Remove(context);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    internal async Task InternalSavingChangesAsync(
        DbContextEventData eventData,
        CancellationToken cancellationToken)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return;
        }

        if (_states.TryGetValue(context, out var activeState) && activeState.IsSavingAuditLogs)
        {
            return;
        }

        _states.Remove(context);
        var state = new SaveState();
        _states.Add(context, state);

        try
        {
            var userProvider = context.GetService<IAuditUserProvider<TUserId>>();
            var handler = context.GetService<IAuditAnonymousHandler>();
            var timeProvider = context.GetService<IAuditTimeProvider>();
            var options = context.GetAuditOptions();
            var now = timeProvider.GetUtcNow();
            var userId = userProvider.Id;
            var isAnonymous = EqualityComparer<TUserId>.Default.Equals(userId, default);
            var entries = new List<AuditEntityEntry>();

            foreach (var entityEntry in context.ChangeTracker.Entries())
            {
                var entityType = entityEntry.Metadata.ClrType;
                var metadata = entityEntry.Metadata.GetAuditMetadata();
                var eventType = AuditEventType.None;

                switch (entityEntry.State)
                {
                    case EntityState.Deleted:
                        if (metadata.HasDeletedAt)
                        {
                            entityEntry.Property(Constants.DeletedAt).CurrentValue = now;
                        }

                        if (metadata.HasDeletedBy)
                        {
                            await EnsureUserAsync(
                                options, metadata, handler, isAnonymous, entityType,
                                DataOperation.Delete, entityEntry.Entity, cancellationToken).ConfigureAwait(false);
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
                            await EnsureUserAsync(
                                options, metadata, handler, isAnonymous, entityType,
                                DataOperation.Modify, entityEntry.Entity, cancellationToken).ConfigureAwait(false);
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
                            await EnsureUserAsync(
                                options, metadata, handler, isAnonymous, entityType,
                                DataOperation.Create, entityEntry.Entity, cancellationToken).ConfigureAwait(false);
                            entityEntry.Property(Constants.CreatedBy).CurrentValue = userId;
                        }

                        eventType = AuditEventType.Create;
                        break;
                }

                if (eventType is not AuditEventType.None && TryGetAuditEntry(entityEntry, out var auditEntry))
                {
                    auditEntry.EventType = eventType;
                    entries.Add(auditEntry);
                }
            }

            if (AppContext.TryGetSwitch(AuditDefaults.DisableAuditingSwitch, out var disabled) && disabled)
            {
                _states.Remove(context);
                return;
            }

            if (entries.Count > 0 &&
                options.TransactionMode is AuditTransactionMode.RequireExplicitTransaction &&
                context.Database.IsRelational() &&
                context.Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException(
                    "AuditOptions.TransactionMode requires an explicit database transaction. " +
                    "Begin a transaction before calling SaveChanges so entity changes and audit logs are atomic.");
            }

            state.Entries = entries;
            state.OccurredAt = now;
            state.UserId = userId;
            state.UserName = userProvider.Name;
            state.IPAddress = userProvider.IPAddress;
            state.ClientName = userProvider.ClientName;
        }
        catch
        {
            _states.Remove(context);
            throw;
        }
    }

    private async Task InternalSavedChanges(
        SaveChangesCompletedEventData eventData,
        bool async,
        CancellationToken cancellationToken)
    {
        var context = eventData.Context;
        if (context is null || !_states.TryGetValue(context, out var state) || state.IsSavingAuditLogs)
        {
            return;
        }

        try
        {
            var options = context.GetAuditOptions();
            var serializer = context.GetService<IPropertySerializer>();
            var logger = context.GetService<ILoggerFactory>().CreateLogger(GetType());
            var entityCount = 0;
            var fieldCount = 0;

            foreach (var entry in state.Entries)
            {
                var log = new AuditEntityChangeLog<TUserId>
                {
                    DatabaseSchema = entry.Schema,
                    TableName = entry.Table,
                    EntityKey = entry.PrimaryKey,
                    EntityTypeName = entry.EntityType,
                    EventType = entry.EventType,
                    EventTime = state.OccurredAt,
                    UserId = state.UserId,
                    UserName = state.UserName,
                    IPAddress = state.IPAddress,
                    ClientName = state.ClientName,
                    Details = entry.Properties.ConvertAll(property => new AuditFieldChangeLog
                    {
                        FieldName = entry.EntityType + '.' + property.Name,
                        OriginalValue = serializer.Serialize(property.OriginalValue, property.ValueType),
                        NewValue = serializer.Serialize(property.NewValue, property.ValueType),
                        ValueType = property.ValueType.AssemblyQualifiedName ?? property.ValueType.FullName ?? property.ValueType.Name,
                    }),
                };

                if (log.Details.Count == 0 && !options.AuditNoFieldChangeEntity)
                {
                    continue;
                }

                context.Add(log);
                entityCount++;
                fieldCount += log.Details.Count;
            }

            if (entityCount > 0)
            {
                state.IsSavingAuditLogs = true;
                if (async)
                {
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    context.SaveChanges();
                }
            }

            logger.LogInformation(
                "Added {EntityCount} entity and {FieldCount} field audit logs to {DbContextType}[{ContextId}].",
                entityCount,
                fieldCount,
                context.GetType().GetFriendlyName(),
                context.ContextId);
        }
        finally
        {
            _states.Remove(context);
        }
    }

    private static Task EnsureUserAsync(
        AuditOptions options,
        AuditMetadata metadata,
        IAuditAnonymousHandler handler,
        bool isAnonymous,
        Type entityType,
        DataOperation operation,
        object entity,
        CancellationToken cancellationToken)
    {
        return !options.AllowAnonymous && !metadata.IsAnonymousAllowed(operation) && isAnonymous
            ? handler.HandleAsync(entityType, operation, entity, cancellationToken)
            : Task.CompletedTask;
    }

    private static bool TryGetAuditEntry(
        EntityEntry entityEntry,
        [NotNullWhen(true)] out AuditEntityEntry? auditEntry)
    {
        if (!entityEntry.Metadata.IsAuditable())
        {
            auditEntry = null;
            return false;
        }

        auditEntry = new AuditEntityEntry(entityEntry);
        foreach (var propertyEntry in entityEntry.Properties)
        {
            if (propertyEntry.Metadata.IsAuditable() &&
                (entityEntry.State is EntityState.Added ||
                 !Equals(propertyEntry.OriginalValue, propertyEntry.CurrentValue)))
            {
                auditEntry.Properties.Add(new AuditPropertyEntry
                {
                    Name = propertyEntry.Metadata.Name,
                    ValueType = propertyEntry.Metadata.ClrType,
                    OriginalValue = entityEntry.State is EntityState.Added ? null : propertyEntry.OriginalValue,
                    NewValue = entityEntry.State is EntityState.Deleted ? null : propertyEntry.CurrentValue,
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

    private sealed class SaveState
    {
        public IReadOnlyList<AuditEntityEntry> Entries { get; set; } = [];
        public DateTimeOffset OccurredAt { get; set; }
        public TUserId? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IPAddress { get; set; }
        public string? ClientName { get; set; }
        public bool IsSavingAuditLogs { get; set; }
    }
}
