namespace DocFXMustache.Models;

public readonly record struct TypeReferenceDocumentation
{
    public string Name { get; }
    public Link Link { get; }

    public TypeReferenceDocumentation(string name, Link link)
    {
        Name = name; // We'll add formatting in Phase 3
        Link = link;
    }
}