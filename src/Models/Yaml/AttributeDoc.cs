using VYaml.Annotations;

namespace DocFXMustache.Models.Yaml;

[YamlObject]
public readonly partial record struct AttributeDoc
{
    public string Type { get; }
    [YamlMember("ctor")]
    public string Constructor { get; }
    public TypeWithValue[] Arguments { get; }

    public AttributeDoc(string type, string constructor, TypeWithValue[] arguments)
    {
        Type = type;
        Constructor = constructor;
        Arguments = arguments;
    }
}