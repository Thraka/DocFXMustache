using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFXMustache.Models;
using DocFXMustache.Models.Yaml;
using Microsoft.Extensions.Logging;

namespace DocFXMustache.Services;

/// <summary>
/// Service responsible for Pass 1: Discovery of all UIDs and building mappings for file organization
/// </summary>
public class DiscoveryService
{
    private readonly MetadataParsingService _parsingService;
    private readonly ILogger<DiscoveryService> _logger;

    public DiscoveryService(MetadataParsingService parsingService, ILogger<DiscoveryService> logger)
    {
        _parsingService = parsingService ?? throw new ArgumentNullException(nameof(parsingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs Pass 1: Discovers all UIDs and builds comprehensive mappings
    /// </summary>
    /// <param name="inputDirectory">Directory containing DocFX YAML files</param>
    /// <param name="groupingStrategy">Strategy for organizing output files</param>
    /// <param name="caseControl">Filename case control: uppercase, lowercase, or mixed</param>
    /// <param name="outputFormat">Output file format (md or mdx)</param>
    /// <returns>UidMappings containing all discovered information</returns>
    public async Task<UidMappings> BuildUidMappingsAsync(string inputDirectory, string groupingStrategy, string caseControl = "lowercase", string outputFormat = "md")
    {
        if (string.IsNullOrEmpty(inputDirectory))
            throw new ArgumentException("Input directory cannot be null or empty", nameof(inputDirectory));

        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException($"Input directory not found: {inputDirectory}");

        _logger.LogInformation("Starting UID discovery in {Directory} with {GroupingStrategy} grouping and {CaseControl} case", 
            inputDirectory, groupingStrategy, caseControl);

        var mappings = new UidMappings();
        var rootObjects = await _parsingService.ParseDirectoryAsync(inputDirectory);

        var processedCount = 0;
        var skippedCount = 0;

        foreach (var root in rootObjects)
        {
            if (!_parsingService.ValidateMetadata(root))
            {
                _logger.LogWarning("Skipping invalid metadata");
                skippedCount++;
                continue;
            }

            ProcessRootObject(root, mappings, groupingStrategy, caseControl, outputFormat);
            processedCount++;
        }

        _logger.LogInformation(
            "Discovery completed: {TotalUids} UIDs found across {AssemblyCount} assemblies and {NamespaceCount} namespaces ({ProcessedCount} processed, {SkippedCount} skipped)",
            mappings.TotalUids, 
            mappings.Assemblies.Count(), 
            mappings.Namespaces.Count(),
            processedCount,
            skippedCount);
        
        Console.WriteLine($"Discovery completed: {mappings.TotalUids} UIDs found across {mappings.Assemblies.Count()} assemblies and {mappings.Namespaces.Count()} namespaces");
        
        return mappings;
    }

    /// <summary>
    /// Processes a single Root object and extracts all UIDs and mappings
    /// </summary>
    private void ProcessRootObject(Root root, UidMappings mappings, string groupingStrategy, string caseControl, string outputFormat)
    {
        var itemsProcessed = 0;
        
        foreach (var item in root.Items)
        {
            if (string.IsNullOrEmpty(item.Uid))
            {
                _logger.LogDebug("Skipping item without UID");
                continue;
            }

            // Store the item for later reference
            mappings.UidToItem[item.Uid] = item;

            // Determine output path based on grouping strategy
            var outputPath = DetermineOutputPath(item, groupingStrategy, caseControl, outputFormat);
            mappings.UidToFilePath[item.Uid] = outputPath;
            
            _logger.LogDebug("Mapped UID {Uid} to {OutputPath}", item.Uid, outputPath);

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
            
            itemsProcessed++;
        }
        
        _logger.LogDebug("Processed {ItemCount} items from root object", itemsProcessed);
    }

    /// <summary>
    /// Determines the output file path for an item based on the grouping strategy
    /// </summary>
    public string DetermineOutputPath(Item item, string groupingStrategy, string caseControl = "lowercase", string outputFormat = "md")
    {
        // For flat grouping, use the fully qualified UID; otherwise use just the type name
        var fileName = groupingStrategy.ToLowerInvariant() == "flat"
            ? GetSafeFileName(item.Uid ?? item.Name ?? "unknown")
            : GetSafeFileName(item.Name ?? item.Uid ?? "unknown");

        // Apply case transformation to filename
        fileName = ApplyCaseTransformation(fileName, caseControl);

        return groupingStrategy.ToLowerInvariant() switch
        {
            "flat" => $"{fileName}.{outputFormat}",
            "namespace" => $"{GetNamespaceDirectory(item.Namespace)}/{fileName}.{outputFormat}",
            "assembly-namespace" => $"{GetAssemblyDirectory(item.Assemblies)}/{GetNamespaceDirectory(item.Namespace)}/{fileName}.{outputFormat}",
            "assembly-flat" => $"{GetAssemblyDirectory(item.Assemblies)}/{fileName}.{outputFormat}",
            _ => throw new ArgumentException($"Unknown grouping strategy: {groupingStrategy}")
        };
    }

    /// <summary>
    /// Applies case transformation to a filename based on the case control setting
    /// </summary>
    private static string ApplyCaseTransformation(string fileName, string caseControl)
    {
        return caseControl.ToLowerInvariant() switch
        {
            "uppercase" => fileName.ToUpperInvariant(),
            "lowercase" => fileName.ToLowerInvariant(),
            "mixed" => fileName, // Keep original case
            _ => fileName.ToLowerInvariant() // Default to lowercase
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
                      .Replace(')', '-');

        // Handle DocFX generic parameter notation (e.g., List`1, Dictionary`2)
        // These should be converted to -1, -2 format (following Microsoft Learn pattern)
        // The backtick has already been replaced with dash above, so now we have "List-1"
        // which is correct for the Microsoft Learn pattern

        // Do NOT convert to lowercase here - let ApplyCaseTransformation handle it
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