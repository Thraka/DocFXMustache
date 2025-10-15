using VYaml.Annotations;

namespace DocFXMustache.Models.Yaml;

[YamlObject]
public readonly partial record struct ExceptionDoc
{
    public string Type { get; }
    public string CommentId { get; }
    public string? Description { get; }

    public ExceptionDoc(string type, string commentId, string? description)
    {
        Type = type;
        CommentId = commentId;
        Description = description;
    }
}