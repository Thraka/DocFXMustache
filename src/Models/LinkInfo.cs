namespace DocFXMustache.Models;

/// <summary>
/// Represents a resolved link for use in Mustache template rendering.
/// This is the model passed to link.mustache template.
/// </summary>
public class LinkInfo
{
    /// <summary>
    /// The UID being linked to
    /// </summary>
    public required string Uid { get; set; }
    
    /// <summary>
    /// The display name for the link (extracted from UID or metadata)
    /// </summary>
    public required string DisplayName { get; set; }
    
    /// <summary>
    /// The relative path from current file to target (includes anchor if applicable)
    /// </summary>
    public required string RelativePath { get; set; }
    
    /// <summary>
    /// Whether this is an external reference (e.g., System.String)
    /// </summary>
    public bool IsExternal { get; set; }
}
