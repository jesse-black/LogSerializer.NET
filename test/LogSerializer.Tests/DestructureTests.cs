using FluentAssertions;

namespace LogSerialization.Tests;

[Collection("LogSerializer")]
public class DestructureTests
{
  public DestructureTests()
  {
    LogSerializer.Configure(_ => { });
  }

  [Fact]
  public void Destructure_WithNullProperty_ReturnsNoEntry()
  {
    // Arrange
    var obj = new NullableProperty { Baz = null };

    // Act
    var result = LogSerializer.Destructure(obj);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string>());
  }

  [Fact]
  public void Destructure_WithSensitiveDataAttribute_ReturnsMaskedValue()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };

    // Act
    var result = LogSerializer.Destructure(obj);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string> {
      { "FirstName", "*****" },
      { "LastName", "Smith" }
    });
  }

  [Fact]
  public void Destructure_WithSensitiveDataPassed_ReturnsMaskedValue()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    var options = new LogSerializerOptions()
    {
      SensitiveDataProperties = { new(null, "LastName") }
    };

    // Act
    var result = LogSerializer.Destructure(obj, options);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string> {
      { "FirstName", "*****" },
      { "LastName", "*****" }
    });
  }

  [Fact]
  public void Destructure_WithSensitiveDataConfigured_ReturnsMaskedValue()
  {
    // Arrange
    var obj = new TestPerson { FirstName = "John", LastName = "Smith" };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new(null, "LastName")));

    // Act
    var result = LogSerializer.Destructure(obj);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string> {
      { "FirstName", "*****" },
      { "LastName", "*****" }
    });
  }

  [Fact]
  public void Destructure_NestedObject_ReturnsJsonValue()
  {
    // Arrange
    var obj = new TestNestedObject { NestedObject = new DefaultConstructor { Bar = "Foo" } };

    // Act
    var result = LogSerializer.Destructure(obj);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string> {
      { "NestedObject", "{\r\n  \"Bar\": \"Foo\"\r\n}" }
    });
  }

  [Fact]
  public void Destructure_NestedObjectWithSensitiveData_ReturnsMaskedJsonValue()
  {
    // Arrange
    var obj = new TestNestedObject { NestedObject = new DefaultConstructor { Bar = "Foo" } };
    LogSerializer.Configure(options => options.SensitiveDataProperties.Add(new(null, "Bar")));

    // Act
    var result = LogSerializer.Destructure(obj);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string> {
      { "NestedObject", "{\r\n  \"Bar\": \"*****\"\r\n}" }
    });
  }

  [Fact]
  public void Destructure_SensitiveDataNestedObject_ReturnsDefaultObjectJsonValue()
  {
    // Arrange
    var obj = new TestDefaultConstructor { DefaultConstructor = new DefaultConstructor { Bar = "Foo" } };

    // Act
    var result = LogSerializer.Destructure(obj);

    // Assert
    result.Should().BeEquivalentTo(new Dictionary<string, string> {
      { "DefaultConstructor", "{\r\n  \"Bar\": null\r\n}" }
    });
  }
}