using VYaml.Annotations;

namespace DocFXMustache.Models.Yaml;

[YamlObject]
public readonly partial struct Return
{
    public string Type { get; }
    public string? Description { get; }

    public Return(string type, string? description)
    {
        Type = type;
        Description = description;
    }
}