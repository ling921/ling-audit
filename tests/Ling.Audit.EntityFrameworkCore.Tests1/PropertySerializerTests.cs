namespace Ling.Audit.EntityFrameworkCore.Tests;

public class PropertySerializerTests
{
    private readonly IPropertySerializer _serializer;

    public PropertySerializerTests()
    {
        _serializer = new DefaultPropertySerializer();
    }

    [Fact]
    public void Serialize_ShouldHandlePrimitiveTypes()
    {
        // Arrange
        var value = 42;

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void Serialize_ShouldHandleStrings()
    {
        // Arrange
        var value = "Test String";

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        result.Should().Be("Test String");
    }

    [Fact]
    public void Serialize_ShouldHandleNullValues()
    {
        // Arrange
        string? value = null;

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Serialize_ShouldHandleComplexObjects()
    {
        // Arrange
        var value = new TestComplexObject
        {
            Id = 1,
            Name = "Test",
            Nested = new TestNestedObject { Value = "Nested" }
        };

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Id");
        result.Should().Contain("Name");
        result.Should().Contain("Nested");
    }

    [Fact]
    public void Serialize_ShouldHandleCollections()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3 };

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("1");
        result.Should().Contain("2");
        result.Should().Contain("3");
    }

    [Fact]
    public void Serialize_ShouldHandleEnums()
    {
        // Arrange
        var value = TestEnum.Value2;

        // Act
        var result = _serializer.Serialize(value);

        // Assert
        result.Should().Be("Value2");
    }

    private class TestComplexObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public TestNestedObject Nested { get; set; } = null!;
    }

    private class TestNestedObject
    {
        public string Value { get; set; } = null!;
    }

    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}
