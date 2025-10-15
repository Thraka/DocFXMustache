using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFXMustache.Models;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Services;

/// <summary>
/// Service responsible for Pass 1: Discovery of all UIDs and building mappings for file organization
/// </summary>
public class DiscoveryService
{
    private readonly MetadataParsingService _parsingService;

    public DiscoveryService(MetadataParsingService parsingService)
    {
        _parsingService = parsingService ?? throw new ArgumentNullException(nameof(parsingService));
    }

    /// <summary>
    /// Performs Pass 1: Discovers all UIDs and builds comprehensive mappings
    /// </summary>
    /// <param name="inputDirectory">Directory containing DocFX YAML files</param>
    /// <param name="groupingStrategy">Strategy for organizing output files</param>
    /// <returns>UidMappings containing all discovered information</returns>
    public async Task<UidMappings> BuildUidMappingsAsync(string inputDirectory, string groupingStrategy)
    {
        if (string.IsNullOrEmpty(inputDirectory))
            throw new ArgumentException("Input directory cannot be null or empty", nameof(inputDirectory));

        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException($"Input directory not found: {inputDirectory}");

        var mappings = new UidMappings();
        var rootObjects = await _parsingService.ParseDirectoryAsync(inputDirectory);

        foreach (var root in rootObjects)
        {
            if (!_parsingService.ValidateMetadata(root))
            {
                Console.WriteLine($"Warning: Skipping invalid metadata");
                continue;
            }

            ProcessRootObject(root, mappings, groupingStrategy);
        }

        Console.WriteLine($"Discovery completed: {mappings.TotalUids} UIDs found across {mappings.Assemblies.Count()} assemblies and {mappings.Namespaces.Count()} namespaces");
        
        return mappings;
    }

    /// <summary>
    /// Processes a single Root object and extracts all UIDs and mappings
    /// </summary>
    private void ProcessRootObject(Root root, UidMappings mappings, string groupingStrategy)
    {
        foreach (var item in root.Items)
        {
            if (string.IsNullOrEmpty(item.Uid))
                continue;

            // Store the item for later reference
            mappings.UidToItem[item.Uid] = item;

            // Determine output path based on grouping strategy
            var outputPath = DetermineOutputPath(item, groupingStrategy);
            mappings.UidToFilePath[item.Uid] = outputPath;

            // Track assemblies
            if (item.Assemblies != null)
            {
                foreach (var assembly in item.Assemblies)
                {
                    if (!string.IsNullOrEmpty(assembly))
                    {
                        mappings.AssemblyMappings.TryAdd(assembly, GetAssemblyOutputDirectory(assembly, groupingStrategy));
                    }
                }
            }

            // Track namespaces
            if (!string.IsNullOrEmpty(item.Namespace))
            {
                mappings.NamespaceMappings.TryAdd(item.Namespace, GetNamespaceOutputDirectory(item.Namespace, groupingStrategy));
            }
        }
    }

    /// <summary>
    /// Determines the output file path for an item based on the grouping strategy
    /// </summary>
    public string DetermineOutputPath(Item item, string groupingStrategy)
    {
        var fileName = GetSafeFileName(item.Name ?? item.Uid ?? "unknown");

        return groupingStrategy.ToLowerInvariant() switch
        {
            "flat" => $"{fileName}.md",
            "namespace" => $"{GetNamespaceDirectory(item.Namespace)}/{fileName}.md",
            "assembly-namespace" => $"{GetAssemblyDirectory(item.Assemblies)}/{GetNamespaceDirectory(item.Namespace)}/{fileName}.md",
            "assembly-flat" => $"{GetAssemblyDirectory(item.Assemblies)}/{fileName}.md",
            _ => throw new ArgumentException($"Unknown grouping strategy: {groupingStrategy}")
        };
    }

    /// <summary>
    /// Extracts UIDs from metadata for a single Root object
    /// </summary>
    public Dictionary<string, string> ExtractUidsFromMetadata(Root metadata)
    {
        var uids = new Dictionary<string, string>();
        
        if (metadata?.Items == null)
            return uids;

        foreach (var item in metadata.Items)
        {
            if (!string.IsNullOrEmpty(item.Uid))
            {
                uids[item.Uid] = item.Name ?? item.Uid;
            }
        }

        return uids;
    }

    private string GetAssemblyDirectory(string[]? assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            return "unknown-assembly";

        var assembly = assemblies[0]; // Use first assembly if multiple
        return GetSafeDirectoryName(assembly);
    }

    private string GetAssemblyOutputDirectory(string assembly, string groupingStrategy)
    {
        return groupingStrategy.ToLowerInvariant() switch
        {
            "assembly-namespace" or "assembly-flat" => GetSafeDirectoryName(assembly),
            _ => string.Empty
        };
    }

    private string GetNamespaceDirectory(string? @namespace)
    {
        if (string.IsNullOrEmpty(@namespace))
            return "global";

        return GetSafeDirectoryName(@namespace);
    }

    private string GetNamespaceOutputDirectory(string @namespace, string groupingStrategy)
    {
        return groupingStrategy.ToLowerInvariant() switch
        {
            "namespace" or "assembly-namespace" => GetSafeDirectoryName(@namespace),
            _ => string.Empty
        };
    }

    private static string GetSafeFileName(string name)
    {
        // Remove or replace characters that are invalid in file names
        var invalidChars = Path.GetInvalidFileNameChars();
        var result = name;
        
        foreach (var c in invalidChars)
        {
            result = result.Replace(c, '-');
        }

        // Also handle some common problematic characters
        result = result.Replace('<', '-')
                      .Replace('>', '-')
                      .Replace('`', '-')
                      .Replace('(', '-')
                      .Replace(')', '-')
                      .Replace('.', '-');

        // Convert to lowercase for consistency with directory names
        result = result.ToLowerInvariant();

        return result;
    }

    private static string GetSafeDirectoryName(string name)
    {
        // Similar to GetSafeFileName but for directories
        var invalidChars = Path.GetInvalidPathChars();
        var result = name;
        
        foreach (var c in invalidChars)
        {
            result = result.Replace(c, '_');
        }

        // Replace dots with dashes for better directory structure
        result = result.Replace('.', '-').ToLowerInvariant();

        return result;
    }
}