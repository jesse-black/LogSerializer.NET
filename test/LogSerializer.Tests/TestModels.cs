namespace LogSerialization.Tests;

public class TestPerson
{
  [SensitiveData]
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
}

public class TestNoDefaultConstructor
{
  [SensitiveData]
  public required NoDefaultConstructor NoDefaultConstructor { get; set; }
}

public class NoDefaultConstructor
{
  public NoDefaultConstructor(string foo) { Foo = foo; }
  public string Foo { get; set; }
}

public class TestDefaultConstructor
{
  [SensitiveData]
  public required DefaultConstructor DefaultConstructor { get; set; }
}

public class DefaultConstructor
{
  public required string Bar { get; set; }
}

public class TestStruct
{
  [SensitiveData]
  public DateTime DateTime { get; set; }
}

public class TestNestedObject
{
  public required DefaultConstructor NestedObject { get; set; }
}

public class NullableProperty
{
  public string? Baz { get; set; }
}