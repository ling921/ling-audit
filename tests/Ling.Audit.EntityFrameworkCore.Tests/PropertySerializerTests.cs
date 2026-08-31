using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class PropertySerializerTests
{
    private readonly IPropertySerializer _serializer;

    public PropertySerializerTests()
    {
        _serializer = new DefaultPropertySerializer(null);
    }

    [Fact]
    public void Serialize_PrimitiveTypes_Should_Return_StringRepresentation()
    {
        // Arrange & Act & Assert
        _serializer.Serialize(42, typeof(int)).Should().Be("42");
        _serializer.Serialize(true, typeof(bool)).Should().Be("True");
        _serializer.Serialize(3.14, typeof(double)).Should().Be("3.14");
    }

    [Fact]
    public void Serialize_Null_Should_Return_Null()
    {
        // Arrange & Act
        var result = _serializer.Serialize(null, typeof(string));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Serialize_String_Should_Return_SameString()
    {
        // Arrange
        var value = "Test String";

        // Act
        var result = _serializer.Serialize(value, typeof(string));

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Serialize_DateTime_Should_Return_StringRepresentation()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1);

        // Act
        var result = _serializer.Serialize(date, typeof(DateTime));

        // Assert
        result.Should().Be(date.ToString());
    }

    [Fact]
    public void Serialize_Guid_Should_Return_StringRepresentation()
    {
        // Arrange
        var guid = Guid.Parse("A52E6E92-8F6C-45EB-8B96-F6DDACC0C8F7");

        // Act
        var result = _serializer.Serialize(guid, typeof(Guid));

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Serialize_Enum_Should_Return_StringRepresentation()
    {
        // Arrange
        var value = TestEnum.Value2;

        // Act
        var result = _serializer.Serialize(value, typeof(TestEnum));

        // Assert
        result.Should().Be("Value2");
    }

    [Fact]
    public void Serialize_ComplexType_Should_ReturnJsonString()
    {
        // Arrange
        var complexObject = new TestComplexObject
        {
            Id = 1,
            Name = "Test"
        };

        // Act
        var result = _serializer.Serialize(complexObject, typeof(TestComplexObject));

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("\"Id\":1");
        result.Should().Contain("\"Name\":\"Test\"");
    }

    [Fact]
    public void Deserialize_PrimitiveTypes_Should_ReturnCorrectValue()
    {
        // Arrange & Act & Assert
        _serializer.Deserialize("42", typeof(int)).Should().Be(42);
        _serializer.Deserialize("True", typeof(bool)).Should().Be(true);
        _serializer.Deserialize("3.14", typeof(double)).Should().Be(3.14);
    }

    [Fact]
    public void Deserialize_Null_Should_ReturnDefaultValue()
    {
        // Arrange & Act & Assert
        _serializer.Deserialize(null, typeof(int)).Should().Be(0);
        _serializer.Deserialize(null, typeof(string)).Should().Be(string.Empty);
        _serializer.Deserialize(null, typeof(int?)).Should().BeNull();
    }

    [Fact]
    public void Deserialize_ComplexType_Should_ReturnCorrectObject()
    {
        // Arrange
        var json = "{\"Id\":1,\"Name\":\"Test\"}";

        // Act
        var result = _serializer.Deserialize(json, typeof(TestComplexObject)) as TestComplexObject;

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void CustomSerializer_Should_BeUsable()
    {
        // Arrange
        var serializer = new CustomPropertySerializer();
        var value = new TestComplexObject { Id = 1, Name = "Test" };

        // Act
        var serialized = serializer.Serialize(value, typeof(TestComplexObject));
        var deserialized = serializer.Deserialize(serialized, typeof(TestComplexObject)) as TestComplexObject;

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(1);
        deserialized.Name.Should().Be("Test");
    }

    private class TestComplexObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    private class CustomPropertySerializer : IPropertySerializer
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
