using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace LogSerializer;

public static class LogSerializer
{
  private static LogSerializerOptions defaultOptions = new LogSerializerOptions();

  public static string Serialize<T>(T obj, LogSerializerOptions? options = null)
  {
    options ??= defaultOptions;
    return JsonSerializer.Serialize(obj, options.JsonSerializerOptions);
  }

  public static Dictionary<string, string?> Destructure<T>(T obj, LogSerializerOptions? options = null)
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
            });
  }

  public static void ConfigureDefaults(Action<LogSerializerOptions> configure)
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