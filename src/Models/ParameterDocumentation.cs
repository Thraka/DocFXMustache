namespace DocFXMustache.Models;

public readonly record struct ParameterDocumentation(string Name, TypeReferenceDocumentation Type, string? Summary);