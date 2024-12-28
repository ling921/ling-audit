using Ling.Audit.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Ling.Audit.SourceGenerators.Tests.Helpers;

public class ITypeSymbolHelpersTests
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
    public async Task IsNullable_WithNullableType_ReturnsTrue()
    {
        const string source = """
            using System;

            public class TestClass
            {
                public int? NullableInt { get; set; }
            }
        """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var propertySymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt");

        Assert.True(propertySymbol.Type.IsNullable());
    }

    [Fact]
    public async Task IsNullable_WithNonNullableType_ReturnsFalse()
    {
        const string source = """
            using System;

            public class TestClass
            {
                public int NonNullableInt { get; set; }
            }
        """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var propertySymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NonNullableInt");

        Assert.False(propertySymbol.Type.IsNullable());
    }

    [Fact]
    public async Task CanAssignNull_WithReferenceType_ReturnsTrue()
    {
        const string source = """
            using System;

            public class TestClass
            {
                public string ReferenceType { get; set; }
            }
        """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var propertySymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ReferenceType");

        Assert.True(propertySymbol.Type.CanAssignNull());
    }

    [Fact]
    public async Task CanAssignNull_WithNullableType_ReturnsTrue()
    {
        const string source = """
            using System;

            public class TestClass
            {
                public int? NullableInt { get; set; }
            }
        """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var propertySymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt");

        Assert.True(propertySymbol.Type.CanAssignNull());
    }

    [Fact]
    public async Task CanAssignNull_WithNonNullableType_ReturnsFalse()
    {
        const string source = """
            using System;

            public class TestClass
            {
                public int NonNullableInt { get; set; }
            }
        """;

        var typeSymbol = await GetTypeSymbolAsync(source, "TestClass");
        var propertySymbol = typeSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NonNullableInt");

        Assert.False(propertySymbol.Type.CanAssignNull());
    }
}
