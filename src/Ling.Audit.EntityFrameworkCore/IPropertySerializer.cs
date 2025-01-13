using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// 
/// </summary>
public interface IPropertySerializer
{
    string? Serialize(object? value, Type valueType);

    object? Deserialize(string? value, Type valueType);
}

internal sealed class DefaultPropertySerializer : IPropertySerializer
{
    private readonly JsonSerializerOptions _options;

    public DefaultPropertySerializer(JsonSerializerOptions jsonSerializerOptions)
    {
#if NET9_0_OR_GREATER
        _options = JsonSerializerOptions.Web;
#else
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
#if NET8_0
        _options.MakeReadOnly();
#endif
#endif
        _options = jsonSerializerOptions;
    }

    public string? Serialize(object? value, Type valueType)
    {
        return value is null ? null : JsonSerializer.Serialize(value, valueType, _options);
    }

    public object? Deserialize(string? value, Type valueType)
    {
        if (value is null)
        {
            return valueType.IsValueType && Nullable.GetUnderlyingType(valueType) is null
                ? Activator.CreateInstance(valueType)
                : null;
        }
        else
        {
            return JsonSerializer.Deserialize(value, valueType, _options);
        }
    }
}
