using System.Text.Json;
using FluentAssertions;

namespace LogSerialization.Tests;

[Collection("LogSerializer")]
public class SerializeTests
{
  public SerializeTests()
  {
    LogSerializer.Configure(_ => { });
  }

  [Fact]
  public void Serialize_WithNullProperty_ReturnsNullJson()
  {
    // Arrange
    var obj = new NullableProperty { Baz = null };

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"Baz\": null\r\n}");
  }

  [Fact]
  public void Serialize_WithSensitiveDataAttribute_ReturnsMaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"Smith\"\r\n}");
  }

  [Fact]
  public void Serialize_WithPassedSensitiveDataNameOnly_ReturnsMaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    var options = new LogSerializerOptions()
    {
      SensitiveDataProperties = { new(null, "LastName") }
    };

    // Act
    var result = LogSerializer.Serialize(obj, options);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"*****\"\r\n}");
  }


  [Fact]
  public void Serialize_WithConfiguredSensitiveDataNameOnly_ReturnsMaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new(null, "LastName")));

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"*****\"\r\n}");
  }

  [Fact]
  public void Serialize_WithConfiguredSensitiveDataNameAndType_ReturnsMaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new("TestPerson", "LastName")));

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"*****\"\r\n}");
  }

  [Fact]
  public void Serialize_WithConfiguredSensitiveDataFullType_ReturnsMaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new("LogSerialization.Tests.TestPerson", "LastName")));

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"*****\"\r\n}");
  }

  [Fact]
  public void Serialize_WithConfiguredSensitiveDataNameNotMatching_ReturnsUnmaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new(null, "FullName")));

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"Smith\"\r\n}");
  }

  [Fact]
  public void Serialize_WithConfiguredSensitiveDataTypeNotMatching_ReturnsUnmaskedString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new("TestObject", "LastName")));

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"FirstName\": \"*****\",\r\n  \"LastName\": \"Smith\"\r\n}");
  }

  [Fact]
  public void Serialize_WithCustomOptions_ReturnsSnakeCaseString()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.JsonSerializerOptions = new()
    {
      WriteIndented = false,
      PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    });

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\"first_name\":\"*****\",\"last_name\":\"Smith\"}");
  }

  [Fact]
  public void Serialize_WithSensitiveNoDefaultConstructorProperty_ReturnsNull()
  {
    // Arrange
    var obj = new TestNoDefaultConstructor { NoDefaultConstructor = new NoDefaultConstructor("foo") };

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"NoDefaultConstructor\": null\r\n}");
  }

  [Fact]
  public void Serialize_WithSensitiveDefaultConstructorProperty_ReturnsDefaultObject()
  {
    // Arrange
    var obj = new TestDefaultConstructor { DefaultConstructor = new DefaultConstructor { Bar = "test" } };

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"DefaultConstructor\": {\r\n    \"Bar\": null\r\n  }\r\n}");
  }

  [Fact]
  public void Serialize_WithSensitiveStructProperty_ReturnsDefaultStruct()
  {
    // Arrange
    var obj = new TestStruct { DateTime = DateTime.UtcNow };

    // Act
    var result = LogSerializer.Serialize(obj);

    // Assert
    result.Should().Be("{\r\n  \"DateTime\": \"0001-01-01T00:00:00\"\r\n}");
  }
}