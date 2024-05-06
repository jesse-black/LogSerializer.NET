using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace LogSerialization;

/// <summary>
/// Represents the options for serializing objects for logging.
/// </summary>
public class LogSerializerOptions
{
  private JsonSerializerOptions jsonSerializerOptions;

  /// <summary>
  /// Initializes a new instance of the <see cref="LogSerializerOptions"/> class.
  /// </summary>
  public LogSerializerOptions()
  {
    jsonSerializerOptions = InitJsonSerializerOptions(new()
    {
      WriteIndented = true,
      Converters = { new JsonStringEnumConverter() }
    });
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="LogSerializerOptions"/> class with the specified options.
  /// </summary>
  /// <param name="options">The options to copy.</param>
  public LogSerializerOptions(LogSerializerOptions options)
  {
    MaskText = options.MaskText;
    SensitiveDataProperties.AddRange(options.SensitiveDataProperties);
    jsonSerializerOptions = InitJsonSerializerOptions(new JsonSerializerOptions(options.JsonSerializerOptions));
  }

  /// <summary>
  /// Gets or sets the text used to mask sensitive data in logs.
  /// </summary>
  public string MaskText { get; set; } = "*****";

  /// <summary>
  /// Gets the list of sensitive data properties.
  /// </summary>
  public List<SensitiveDataProperty> SensitiveDataProperties { get; } = new();

  /// <summary>
  /// Gets or sets the JSON serializer options. Note that a copy will be made of
  /// the options when setting this property.
  /// </summary>
  public JsonSerializerOptions JsonSerializerOptions
  {
    get { return jsonSerializerOptions; }
    set { jsonSerializerOptions = InitJsonSerializerOptions(new JsonSerializerOptions(value)); }
  }

  private JsonSerializerOptions InitJsonSerializerOptions(JsonSerializerOptions options)
  {
    if (options.TypeInfoResolver is not DefaultJsonTypeInfoResolver typeInfoResolver)
    {
      options.TypeInfoResolver = typeInfoResolver = new DefaultJsonTypeInfoResolver();
    }
    typeInfoResolver.Modifiers.Add(jsonTypeInfo => LogSerializer.ReplaceSensitiveDataModifier(jsonTypeInfo, this));
    return options;
  }
}

/// <summary>
/// Identifies a property as sensitive data that should be masked out in logs.
/// </summary>
/// <param name="TypeName">The type name of the property. This can be the full
/// name or only the type name. Using <c>null</c> here will match the property
/// name on all types.</param>
/// <param name="PropertyName">The name of the property.</param>
public record SensitiveDataProperty(string? TypeName, string PropertyName);