using Microsoft.CodeAnalysis;

namespace Ling.Audit.SourceGenerators.Helpers;

public static class ITypeSymbolHelpers
{
    /// <summary>
    /// Checks if the type is a generic type symbol of <see cref="Nullable{T}"/>
    /// </summary>
    /// <param name="type">The type symbol to check</param>
    /// <returns><see langword="true"/> if the type is <see cref="Nullable{T}"/>, otherwise <see langword="false"/></returns>
    public static bool IsNullable(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T };
    }

    /// <summary>
    /// Checks if the type can be assigned <see langword="null"/>
    /// <para>
    /// The type can be assigned <see langword="null"/> if it is a reference type symbol
    /// or if it is a generic type symbol of <see cref="Nullable{T}"/>
    /// </para>
    /// </summary>
    /// <param name="type">The type symbol to check</param>
    /// <returns><see langword="true"/> if the type can be assigned <see langword="null"/>, otherwise <see langword="false"/></returns>
    public static bool CanAssignNull(this ITypeSymbol type)
    {
        return type.IsReferenceType || type.IsNullable();
    }
}