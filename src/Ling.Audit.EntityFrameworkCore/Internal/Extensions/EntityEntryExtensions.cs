using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ling.Audit.EntityFrameworkCore.Internal.Extensions;

/// <summary>
/// Extension methods for <see cref="EntityEntry"/>.
/// </summary>
internal static class EntityEntryExtensions
{
    /// <summary>
    /// Gets the primary key value(s) of the entity as a string.
    /// </summary>
    /// <param name="entityEntry">The entity entry.</param>
    /// <returns>A string representation of the primary key(s) in format "Key1=Value1,Key2=Value2".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityEntry"/> is null.</exception>
    internal static string GetPrimaryKey(this EntityEntry entityEntry)
    {
        ArgumentNullException.ThrowIfNull(entityEntry);

        var key = entityEntry.Metadata.FindPrimaryKey();
        return key is null
            ? string.Empty
            : string.Join(",", key.Properties.Select(p => $"{p.Name}={p.PropertyInfo?.GetValue(entityEntry.Entity)}"));
    }
}
