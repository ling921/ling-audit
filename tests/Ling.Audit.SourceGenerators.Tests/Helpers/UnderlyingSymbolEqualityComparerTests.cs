using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Ling.Audit.SourceGenerators.Tests.Helpers;

public class UnderlyingSymbolEqualityComparerTests
{
    private static async Task<ITypeSymbol> GetTypeSymbolAsync(string source, string typeName)
    {
        var compilation = await CreateCompilationAsync(source);
        return compilation.GetTypeByMetadataName(typeName)!;
    }

    private static async Task<Compilation> CreateCompilationAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ITypeSymbol).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create("TestAssembly", [syntaxTree], references);
        return await Task.FromResult(compilation);
    }

    [Fact]
    public async Task Equals_WithSameUnderlyingType_ReturnsTrue()
    {
        const string source = """
                using System;

                public class TestClass
                {
                    public int? NullableInt { get; set; }
                    public int NonNullableInt { get; set; }
                }
            """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var nullableIntSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt").Type;
        var nonNullableIntSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NonNullableInt").Type;

        var comparer = new UnderlyingSymbolEqualityComparer();
        Assert.True(comparer.Equals(nullableIntSymbol, nonNullableIntSymbol));
    }

    [Fact]
    public async Task Equals_WithDifferentUnderlyingType_ReturnsFalse()
    {
        const string source = """
                using System;

                public class TestClass
                {
                    public int? NullableInt { get; set; }
                    public string ReferenceType { get; set; }
                }
            """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var nullableIntSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt").Type;
        var referenceTypeSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ReferenceType").Type;

        var comparer = new UnderlyingSymbolEqualityComparer();
        Assert.False(comparer.Equals(nullableIntSymbol, referenceTypeSymbol));
    }

    [Fact]
    public async Task GetHashCode_WithSameUnderlyingType_ReturnsSameHashCode()
    {
        const string source = """
                using System;

                public class TestClass
                {
                    public int? NullableInt { get; set; }
                    public int NonNullableInt { get; set; }
                }
            """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var nullableIntSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt").Type;
        var nonNullableIntSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NonNullableInt").Type;

        var comparer = new UnderlyingSymbolEqualityComparer();
        Assert.Equal(comparer.GetHashCode(nullableIntSymbol), comparer.GetHashCode(nonNullableIntSymbol));
    }

    [Fact]
    public async Task GetHashCode_WithDifferentUnderlyingType_ReturnsDifferentHashCode()
    {
        const string source = """
                using System;

                public class TestClass
                {
                    public int? NullableInt { get; set; }
                    public string ReferenceType { get; set; }
                }
            """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var nullableIntSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt").Type;
        var referenceTypeSymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ReferenceType").Type;

        var comparer = new UnderlyingSymbolEqualityComparer();
        Assert.NotEqual(comparer.GetHashCode(nullableIntSymbol), comparer.GetHashCode(referenceTypeSymbol));
    }
}
