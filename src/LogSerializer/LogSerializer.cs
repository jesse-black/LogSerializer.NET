using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace LogSerialization;

/// <summary>
/// Provides methods for serializing and destructuring objects for logging, with
/// properties marked as sensitive data masked out.
/// </summary>
public static class LogSerializer
{
  private static LogSerializerOptions defaultOptions = new();

  /// <summary>
  /// Serializes an object to a JSON string using the specified options.
  /// </summary>
  /// <typeparam name="T">The type of the object to serialize.</typeparam>
  /// <param name="obj">The object to serialize.</param>
  /// <param name="options">The serialization options. If not provided, the default options will be used.</param>
  /// <returns>A JSON string representing the serialized object.</returns>
  public static string Serialize<T>(T obj, LogSerializerOptions? options = null)
  {
    options ??= defaultOptions;
    return JsonSerializer.Serialize(obj, options.JsonSerializerOptions);
  }

  /// <summary>
  /// Destructures an object into a dictionary of property names and values, using the specified options.
  /// </summary>
  /// <typeparam name="T">The type of the object to destructure.</typeparam>
  /// <param name="obj">The object to destructure.</param>
  /// <param name="options">The serialization options. If not provided, the default options will be used.</param>
  /// <returns>A dictionary containing the property names and values of the destructured object.</returns>
  public static Dictionary<string, string> Destructure<T>(T obj, LogSerializerOptions? options = null)
  {
    options ??= defaultOptions;
    return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead)
        .ToDictionary(
            p => p.Name,
            p => p.GetValue(obj) switch
            {
              null => null,
              string s => p.IsSensitiveData(options) ? options.MaskText : s,
              object o => p.IsSensitiveData(options) ? Serialize(DefaultInstance(o.GetType()), options) : Serialize(o, options)
            })
        .Where(kv => kv.Value is not null)
        .ToDictionary(kv => kv.Key, kv => kv.Value!);
  }

  /// <summary>
  /// Configures the default serialization options using the specified action.
  /// </summary>
  /// <param name="configure">An action that configures the default serialization options.</param>
  public static void Configure(Action<LogSerializerOptions> configure)
  {
    defaultOptions = new();
    configure(defaultOptions);
  }

  internal static void ReplaceSensitiveDataModifier(JsonTypeInfo typeInfo, LogSerializerOptions options)
  {
    foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
    {
      if (propertyInfo.IsSensitiveData(typeInfo, options))
      {
        var getProperty = propertyInfo.Get;
        if (getProperty is not null)
        {
          propertyInfo.Get = (obj) =>
          {
            var value = getProperty(obj);
            return value switch
            {
              null => null,
              string s => options.MaskText,
              object o => DefaultInstance(o.GetType())
            };
          };
        }
      }
    }
  }

  private static bool IsSensitiveData(this PropertyInfo propertyInfo, LogSerializerOptions options)
  {
    return options.SensitiveDataProperties
        .Any(p => p.PropertyName == propertyInfo.Name
            && (p.TypeName is null || p.TypeName == propertyInfo.DeclaringType?.Name || p.TypeName == propertyInfo.DeclaringType?.FullName))
    || propertyInfo.GetCustomAttribute(typeof(SensitiveDataAttribute), true) is not null;
  }

  private static bool IsSensitiveData(this JsonPropertyInfo propertyInfo, JsonTypeInfo typeInfo, LogSerializerOptions options)
  {
    return options.SensitiveDataProperties
        .Any(p => p.PropertyName == propertyInfo.Name
            && (p.TypeName is null || p.TypeName == typeInfo.Type.Name || p.TypeName == typeInfo.Type.FullName))
    || propertyInfo.AttributeProvider?.GetCustomAttributes(typeof(SensitiveDataAttribute), true)?.Length >= 1;
  }

  private static object? DefaultInstance(Type type)
  {
    try
    {
      return Activator.CreateInstance(type);
    }
    catch (MissingMethodException)
    {
      return null;
    }
  }
}