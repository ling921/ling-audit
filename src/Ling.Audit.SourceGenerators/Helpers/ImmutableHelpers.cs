using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Helpers;

internal static class ImmutableHelpers
{
    public static int FindIndex<T>(this ImmutableArray<T> source, Func<T, bool> predicate)
    {
        for (var i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
            {
                return i;
            }
        }
        return -1;
    }
}
