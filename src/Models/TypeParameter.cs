namespace DocFXMustache.Models;

/// <summary>
/// Represents a type parameter (generic parameter) documentation.
/// </summary>
public readonly record struct TypeParameter(string Name, string? Description);