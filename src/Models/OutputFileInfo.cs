namespace DocFXMustache.Models;

/// <summary>
/// Represents output file information for a UID, including file path and optional anchor
/// </summary>
public class OutputFileInfo
{
    /// <summary>
    /// The full file path where the UID is documented
    /// </summary>
    public required string FilePath { get; set; }
    
    /// <summary>
    /// Optional anchor for member links (e.g., "foreground" for property on parent type page)
    /// </summary>
    public string? Anchor { get; set; }
}
