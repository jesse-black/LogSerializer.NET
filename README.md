# LogSerializer .NET
LogSerializer is a .NET package for serializing objects to JSON with sensitive
data (such as access tokens or personally identifiable information) masked out
so that it can be used for logging and telemetry purposes.

## Installation
Install the package via NuGet:

```bash
dotnet add package LogSerializer
```

## Configuration
Configure the default options with the `LogSerializer.Configure` method:
```csharp
LogSerializer.Configure(o => {

  o.MaskText = "<REDACTED>",
  o.SensitiveDataProperties.Add(new("Person", "LastName")),
  o.JsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
  }
})
```
Or construct a new `LogSerializerOptions` and pass it to the methods:
```csharp
var options = new LogSerializerOptions()
{
  MaskText = "<REDACTED>",
  SensitiveDataProperties = { new("Person", "LastName") },
  JsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
  }
};
LogSerializer.Serialize(obj, options);
```
Sensitive data can be marked with annotations:
```csharp
public class TokenResponse 
{
  [SensitiveData]
  public string Token { get; set; }
}
```
Or configured in `LogSerializerOptions.SensitiveDataProperties`. The list of
type name and property name can accept null type name to match the property on
any type:
```csharp
options.SensitiveDataProperties.Add(new(null, "LastName"))
```
Or the short type name:
```csharp
options.SensitiveDataProperties.Add(new("Person", "LastName"))
```
Or the fully qualified type name:
```csharp
options.SensitiveDataProperties.Add(new("Foo.Bar.Person", "LastName"))
```

## Usage
Use `LogSerializer.Serialize` to serialize objects to JSON for logging:
```csharp
logger.LogInformation("Processed message: {message}", LogSerializer.Serialize(message));
```
Use `LogSerializer.Destructure` to create a dictionary out of an object's
properties where the key is the property name and the value is the serialized
JSON of the property value. This is useful with telemetry logging libraries that
track custom telemetry properties as key value dictionaries, such as
[TelemetryClient.TrackEvent](https://learn.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.telemetryclient.trackevent?view=azure-dotnet)
in Application Insights:
```csharp
telemetry.TrackEvent("ProcessMessage", LogSerializer.Destructure(message));
```

## License
This project is licensed under the Apache License Version 2.0 - see the
[LICENSE](https://github.com/jesse-black/LogSerializer.NET/blob/main/LICENSE) file for
details.