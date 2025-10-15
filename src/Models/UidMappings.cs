using System.Collections.Generic;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Models;

/// <summary>
/// Stores the results from Pass 1 (Discovery) for use in Pass 2 (Generation)
/// </summary>
public class UidMappings
{
    /// <summary>
    /// Maps UID to the output file path where the documentation will be generated
    /// </summary>
    public Dictionary<string, string> UidToFilePath { get; set; } = new();
    
    /// <summary>
    /// Maps UID to the parsed Item object for quick access during generation
    /// </summary>
    public Dictionary<string, Item> UidToItem { get; set; } = new();
    
    /// <summary>
    /// Maps assembly names to their respective output directories based on grouping strategy
    /// </summary>
    public Dictionary<string, string> AssemblyMappings { get; set; } = new();
    
    /// <summary>
    /// Maps namespace names to their respective output directories based on grouping strategy
    /// </summary>
    public Dictionary<string, string> NamespaceMappings { get; set; } = new();
    
    /// <summary>
    /// Total number of UIDs discovered
    /// </summary>
    public int TotalUids => UidToItem.Count;
    
    /// <summary>
    /// Gets all unique assemblies found during discovery
    /// </summary>
    public IEnumerable<string> Assemblies => AssemblyMappings.Keys;
    
    /// <summary>
    /// Gets all unique namespaces found during discovery
    /// </summary>
    public IEnumerable<string> Namespaces => NamespaceMappings.Keys;
}