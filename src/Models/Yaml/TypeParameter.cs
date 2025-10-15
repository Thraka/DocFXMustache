using VYaml.Annotations;

namespace DocFXMustache.Models.Yaml;

[YamlObject]
public partial record struct TypeParameter
{
    public string Id { get; }
    public string Description { get; }

    public TypeParameter(string id, string description)
    {
        Id = id;
        Description = description;
    }
}