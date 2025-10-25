using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DocFXMustache.Models;

/// <summary>
/// Configuration for template processing behavior and template file mappings.
/// Loaded from template.json in the template directory.
/// </summary>
public sealed class TemplateConfiguration
{
    /// <summary>
    /// Name of the template set
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "default";

    /// <summary>
    /// Description of the template set
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Template version
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Default output format for this template set.
    /// Can be overridden by CLI --format option.
    /// </summary>
    [JsonPropertyName("outputFormat")]
    public string OutputFormat { get; set; } = "md";

    /// <summary>
    /// Default file grouping strategy for this template set.
    /// Can be overridden by CLI --grouping option.
    /// </summary>
    [JsonPropertyName("fileGrouping")]
    public string FileGrouping { get; set; } = "flat";

    /// <summary>
    /// Default filename case handling for this template set.
    /// Can be overridden by CLI --case option.
    /// </summary>
    [JsonPropertyName("filenameCase")]
    public string FilenameCase { get; set; } = "lowercase";

    /// <summary>
    /// When true, combine all members on the parent type's page with anchor links.
    /// When false, create separate files for each member.
    /// Default: true
    /// </summary>
    [JsonPropertyName("combineMembers")]
    public bool CombineMembers { get; set; } = true;

    /// <summary>
    /// Generate namespace and assembly index files.
    /// Default: true
    /// </summary>
    [JsonPropertyName("generateIndexFiles")]
    public bool GenerateIndexFiles { get; set; } = true;

    /// <summary>
    /// Include inherited members in type documentation.
    /// Default: false
    /// </summary>
    [JsonPropertyName("includeInheritedMembers")]
    public bool IncludeInheritedMembers { get; set; } = false;

    /// <summary>
    /// Mappings of item types to template file names
    /// </summary>
    [JsonPropertyName("templates")]
    public TemplateFileMappings Templates { get; set; } = new();
}

/// <summary>
/// Mappings of API item types to their corresponding template files
/// </summary>
public sealed class TemplateFileMappings
{
    [JsonPropertyName("class")]
    public string Class { get; set; } = "class.mustache";

    [JsonPropertyName("interface")]
    public string Interface { get; set; } = "interface.mustache";

    [JsonPropertyName("enum")]
    public string Enum { get; set; } = "enum.mustache";

    [JsonPropertyName("struct")]
    public string Struct { get; set; } = "struct.mustache";

    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = "namespace.mustache";

    [JsonPropertyName("assembly")]
    public string Assembly { get; set; } = "assembly.mustache";

    [JsonPropertyName("link")]
    public string Link { get; set; } = "link.mustache";

    [JsonPropertyName("member")]
    public string Member { get; set; } = "member.mustache";

    [JsonPropertyName("delegate")]
    public string? Delegate { get; set; } = "delegate.mustache";

    /// <summary>
    /// Get the template file name for a given item type.
    /// Falls back to class.mustache if not found.
    /// </summary>
    public string GetTemplateForType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Class => Class,
            ItemType.Interface => Interface,
            ItemType.Enum => Enum,
            ItemType.Struct => Struct,
            ItemType.Namespace => Namespace,
            ItemType.Delegate => Delegate ?? Class,
            _ => Class // Default fallback
        };
    }
}
