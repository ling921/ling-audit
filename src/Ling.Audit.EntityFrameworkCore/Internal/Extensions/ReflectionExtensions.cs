using System.Text;

namespace Ling.Audit.EntityFrameworkCore.Internal.Extensions;

/// <summary>
/// Extension methods for reflection operations.
/// </summary>
internal static class ReflectionExtensions
{
    /// <summary>
    /// Gets a friendly name for the type, handling generic type definitions.
    /// </summary>
    /// <param name="type">The type to get the friendly name for.</param>
    /// <returns>A readable string representation of the type name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    internal static string GetFriendlyName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var builder = new StringBuilder();
        var nameParts = type.Name.Split('`');
        builder.Append(nameParts[0]);

        var genericArgs = type.GetGenericArguments();
        if (genericArgs.Length > 0)
        {
            builder.Append('<');
            builder.AppendJoin(",", genericArgs.Select(t => t.GetFriendlyName()));
            builder.Append('>');
        }

        return builder.ToString();
    }
}
