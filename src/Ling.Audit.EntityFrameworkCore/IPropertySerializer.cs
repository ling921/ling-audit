using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Interface for serializing and deserializing property values.
/// </summary>
public interface IPropertySerializer
{
    /// <summary>
    /// Converts an object into its string representation based on the specified type.
    /// </summary>
    /// <param name="value">The object to be converted into a string format.</param>
    /// <param name="valueType">Specifies the type of the object being serialized to ensure correct conversion.</param>
    /// <returns>A string representation of the object, or <see langword="null"/> if the conversion fails.</returns>
    string? Serialize(object? value, Type valueType);

    /// <summary>
    /// Converts a string representation into an object of a specified type. It handles potential null values
    /// gracefully.
    /// </summary>
    /// <param name="value">The string representation of the object to be converted.</param>
    /// <param name="valueType">Specifies the type of the object that the string will be converted into.</param>
    /// <returns>Returns the deserialized object or <see langword="null"/> if the input string is <see langword="null"/>.</returns>
    object? Deserialize(string? value, Type valueType);
}

internal sealed class DefaultPropertySerializer : IPropertySerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();
    private static readonly HashSet<Type> AdditionalSimpleTypes = new()
    {
        typeof(string),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(decimal)
    };
    private readonly JsonSerializerOptions _options;

    public DefaultPropertySerializer(JsonSerializerOptions? options)
    {
        _options = options ?? DefaultOptions;
    }

    private static JsonSerializerOptions CreateDefaultOptions()
    {
#if NET9_0_OR_GREATER
        var options = JsonSerializerOptions.Web;
#else
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
#endif
        return options;
    }

    [RequiresUnreferencedCode("The type being serialized may not be preserved in the trimmed output.")]
    public string? Serialize(object? value, Type valueType)
    {
        if (valueType == null)
        {
            throw new ArgumentNullException(nameof(valueType));
        }

        if (value == null)
        {
            return null;
        }

        try
        {
            var typeToCheck = Nullable.GetUnderlyingType(valueType) ?? valueType;
            return IsSimpleType(typeToCheck) ? value.ToString() : JsonSerializer.Serialize(value, valueType, _options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to serialize value of type {valueType.FullName}", ex);
        }
    }

    [RequiresUnreferencedCode("The type being deserialized may not be preserved in the trimmed output.")]
    public object? Deserialize(string? value, Type valueType)
    {
        if (valueType == null)
        {
            throw new ArgumentNullException(nameof(valueType));
        }

        if (value == null)
        {
            return Nullable.GetUnderlyingType(valueType) is null
                ? Activator.CreateInstance(valueType)
                : null;
        }

        try
        {
            var typeToCheck = Nullable.GetUnderlyingType(valueType) ?? valueType;
            return IsSimpleType(typeToCheck) ? Convert.ChangeType(value, typeToCheck) : JsonSerializer.Deserialize(value, valueType, _options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize value to type {valueType.FullName}", ex);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || AdditionalSimpleTypes.Contains(type) || type.IsEnum;
    }
}
