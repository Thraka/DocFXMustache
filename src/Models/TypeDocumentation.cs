using System;
using System.Collections.Generic;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Models;

public sealed class TypeDocumentation
{
    private readonly List<TypeDocumentation> constructors = new();
    private readonly List<TypeDocumentation> fields = new();
    private readonly List<TypeDocumentation> properties = new();
    private readonly List<TypeDocumentation> methods = new();
    private readonly List<TypeDocumentation> events = new();

    public string Uid { get; }
    public string Name { get; }
    public string FullName { get; }
    public ItemType Type { get; }
    public string? Summary { get; }
    public Link Link { get; }
    public string? Syntax { get; }
    public string? Remarks { get; }

    public TypeReferenceDocumentation[] Inheritance { get; }
    public TypeReferenceDocumentation[] Implements { get; }
    public ParameterDocumentation[] Parameters { get; }
    public ReturnDocumentation? Returns { get; }
    public TypeParameter[] TypeParameters { get; }
    public ExceptionDocumentation[] Exceptions { get; }
    public AttributeDoc[] Attributes { get; }

    public IReadOnlyList<TypeDocumentation> Constructors => constructors;
    public IReadOnlyList<TypeDocumentation> Fields => fields;
    public IReadOnlyList<TypeDocumentation> Properties => properties;
    public IReadOnlyList<TypeDocumentation> Methods => methods;
    public IReadOnlyList<TypeDocumentation> Events => events;

    public TypeDocumentation(
        string uid,
        string name,
        string fullName,
        ItemType type,
        string? summary,
        Link link,
        string? syntax = null,
        string? remarks = null,
        TypeReferenceDocumentation[]? inheritance = null,
        TypeReferenceDocumentation[]? implements = null,
        ParameterDocumentation[]? parameters = null,
        ReturnDocumentation? returns = null,
        TypeParameter[]? typeParameters = null,
        ExceptionDocumentation[]? exceptions = null,
        AttributeDoc[]? attributes = null)
    {
        Uid = uid ?? throw new ArgumentNullException(nameof(uid));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        Type = type;
        Summary = summary;
        Link = link;
        Syntax = syntax;
        Remarks = remarks;
        Inheritance = inheritance ?? Array.Empty<TypeReferenceDocumentation>();
        Implements = implements ?? Array.Empty<TypeReferenceDocumentation>();
        Parameters = parameters ?? Array.Empty<ParameterDocumentation>();
        Returns = returns;
        TypeParameters = typeParameters ?? Array.Empty<TypeParameter>();
        Exceptions = exceptions ?? Array.Empty<ExceptionDocumentation>();
        Attributes = attributes ?? Array.Empty<AttributeDoc>();
    }

    public void AddConstructor(TypeDocumentation constructor)
    {
        constructors.Add(constructor);
    }

    public void AddField(TypeDocumentation field)
    {
        fields.Add(field);
    }

    public void AddProperty(TypeDocumentation property)
    {
        properties.Add(property);
    }

    public void AddMethod(TypeDocumentation method)
    {
        methods.Add(method);
    }

    public void AddEvent(TypeDocumentation eventDoc)
    {
        events.Add(eventDoc);
    }
}