using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace LogSerializer;

public class LogSerializerOptions
{
  private JsonSerializerOptions jsonSerializerOptions;
  public LogSerializerOptions()
  {
    jsonSerializerOptions = InitializeJsonSerializerOptions(new()
    {
      WriteIndented = true,
      Converters = { new JsonStringEnumConverter() }
    });
  }
  public LogSerializerOptions(LogSerializerOptions options)
  {
    MaskText = options.MaskText;
    SensitiveDataProperties.AddRange(options.SensitiveDataProperties);
    jsonSerializerOptions = InitializeJsonSerializerOptions(new JsonSerializerOptions(options.JsonSerializerOptions));
  }
  public string MaskText { get; set; } = "*****";
  public List<Property> SensitiveDataProperties { get; } = new();
  public JsonSerializerOptions JsonSerializerOptions
  {
    get { return jsonSerializerOptions; }
    set { jsonSerializerOptions = InitializeJsonSerializerOptions(value); }
  }
  private JsonSerializerOptions InitializeJsonSerializerOptions(JsonSerializerOptions options)
  {
    if (options.TypeInfoResolver is not DefaultJsonTypeInfoResolver typeInfoResolver)
    {
      options.TypeInfoResolver = typeInfoResolver = new DefaultJsonTypeInfoResolver();
    }
    typeInfoResolver.Modifiers.Add(jsonTypeInfo => LogSerializer.ReplaceSensitiveDataModifier(jsonTypeInfo, this));
    return options;
  }
}

public record Property(string? TypeName, string PropertyName);