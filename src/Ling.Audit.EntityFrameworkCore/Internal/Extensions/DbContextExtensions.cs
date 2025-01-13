using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;

namespace Ling.Audit.EntityFrameworkCore.Internal.Extensions;

internal static class DbContextExtensions
{
    internal static AuditOptions GetAuditOptions(this DbContext context)
    {
        return context.GetService<IOptionsSnapshot<AuditOptions>>().Value;
    }

    /// <summary>
    /// Gets the primary key value(s) of the entity as a string.
    /// </summary>
    /// <param name="entityEntry">The entity entry.</param>
    /// <returns>A string representation of the primary key(s) in format "Key1=Value1,Key2=Value2".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityEntry"/> is null.</exception>
    internal static string GetPrimaryKey(this EntityEntry entityEntry)
    {
        ThrowHelper.ThrowIfNull(entityEntry);

        return entityEntry.Metadata.FindPrimaryKey()?.GetPrimaryKey(entityEntry.Entity) ?? string.Empty;
    }

    internal static string GetPrimaryKey(this IKey key, object entity)
    {
        ThrowHelper.ThrowIfNull(key);
        ThrowHelper.ThrowIfNull(entity);

        return string.Join(";", key.Properties.Select(p => $"{p.Name}={p.PropertyInfo?.GetValue(entity)}"));
    }
}
