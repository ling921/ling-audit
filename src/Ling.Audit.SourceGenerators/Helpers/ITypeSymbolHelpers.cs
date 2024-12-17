using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators.Helpers;

public static class ITypeSymbolHelpers
{
    public static bool IsNullable(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol namedType &&
               namedType.IsGenericType &&
               namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
    }
}