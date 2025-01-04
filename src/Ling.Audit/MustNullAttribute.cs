namespace Ling.Audit;

/// <summary>
/// Marks a generic parameter as nullable.
/// </summary>
[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
internal sealed class MustNullAttribute : Attribute;
