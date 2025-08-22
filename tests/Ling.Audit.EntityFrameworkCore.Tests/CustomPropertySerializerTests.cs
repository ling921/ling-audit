using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class CustomPropertySerializerTests
{
    [Fact]
    public void UseSerializer_Should_ReplaceDefaultSerializer()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseInMemoryDatabase(databaseName: $"CustomSerializerTest_{Guid.NewGuid()}")
            .UseSerializer<CustomSerializer>()
            .Options;

        // Act
        using var context = new TestContext(options);
        var serializer = context.GetService<IPropertySerializer>();

        // Assert
        serializer.Should().NotBeNull();
        serializer.Should().BeOfType<CustomSerializer>();
    }

    [Fact]
    public void CustomSerializer_Should_SerializeAsExpected()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseInMemoryDatabase(databaseName: $"CustomSerializerTest_{Guid.NewGuid()}")
            .UseSerializer<CustomSerializer>()
            .Options;

        // Act
        using var context = new TestContext(options);
        var serializer = context.GetService<IPropertySerializer>();
        var testObj = new TestClass { Id = 1, Name = "Test" };
        var serialized = serializer.Serialize(testObj, typeof(TestClass));

        // Assert
        serialized.Should().NotBeNull();
        serialized.Should().Contain("id"); // 驼峰命名
        serialized.Should().Contain("name"); // 驼峰命名
    }

    [Fact]
    public void CustomSerializer_Should_DeserializeAsExpected()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseInMemoryDatabase(databaseName: $"CustomSerializerTest_{Guid.NewGuid()}")
            .UseSerializer<CustomSerializer>()
            .Options;

        // Act
        using var context = new TestContext(options);
        var serializer = context.GetService<IPropertySerializer>();
        var json = "{\"id\":1,\"name\":\"Test\"}";
        var deserialized = serializer.Deserialize(json, typeof(TestClass)) as TestClass;

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(1);
        deserialized.Name.Should().Be("Test");
    }

    private class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class CustomSerializer : IPropertySerializer
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public string? Serialize(object? value, Type valueType)
        {
            if (value == null) return null;
            return JsonSerializer.Serialize(value, valueType, _options);
        }

        public object? Deserialize(string? value, Type valueType)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return JsonSerializer.Deserialize(value, valueType, _options);
        }
    }
}
