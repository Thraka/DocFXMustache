namespace DocFXMustache.Models;

public readonly record struct TypeReferenceDocumentation
{
    public string Uid { get; }
    public string Name { get; }
    public Link Link { get; }

    public TypeReferenceDocumentation(string uid, string name, Link link)
    {
        Uid = uid;
        Name = name;
        Link = link;
    }

    // Convenience constructor with just uid and link (name derived from uid)
    public TypeReferenceDocumentation(string name, Link link) : this(ExtractUid(link), name, link)
    {
    }

    private static string ExtractUid(Link link)
    {
        // For external links, extract UID from the URL if possible
        // For now, just return empty - this will be enhanced later
        return string.Empty;
    }
}