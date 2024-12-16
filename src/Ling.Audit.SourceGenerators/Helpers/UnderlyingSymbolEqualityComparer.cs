using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators.Helpers;

/// <summary>
/// Compares two <see cref="ITypeSymbol"/> instances by their underlying type.
/// </summary>
internal class UnderlyingSymbolEqualityComparer : IEqualityComparer<ITypeSymbol>
{
    /// <inheritdoc/>
    public bool Equals(ITypeSymbol? x, ITypeSymbol? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        var x1 = x.NullableAnnotation == NullableAnnotation.Annotated
            ? ((INamedTypeSymbol)x).TypeArguments[0]
            : x;
        var y1 = y.NullableAnnotation == NullableAnnotation.Annotated
            ? ((INamedTypeSymbol)y).TypeArguments[0]
            : y;

        return SymbolEqualityComparer.Default.Equals(x1, y1);
    }

    /// <inheritdoc/>
    public int GetHashCode(ITypeSymbol obj)
    {
        var underlyingType = obj.NullableAnnotation == NullableAnnotation.Annotated
            ? ((INamedTypeSymbol)obj).TypeArguments[0]
            : obj;
        return SymbolEqualityComparer.Default.GetHashCode(underlyingType);
    }
}
