namespace LogSerializer;

/// <summary>
/// Specifies that a property contains sensitive data that should be masked out when logging.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SensitiveDataAttribute : Attribute
{
}
