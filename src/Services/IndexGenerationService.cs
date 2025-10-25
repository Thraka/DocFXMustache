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
/// Service responsible for generating index files: assembly overviews, namespace indexes, and table of contents
/// </summary>
public class IndexGenerationService
{
    private readonly TemplateProcessingService _templateProcessingService;
    private readonly ILogger<IndexGenerationService> _logger;

    public IndexGenerationService(TemplateProcessingService templateProcessingService, ILogger<IndexGenerationService> logger)
    {
        _templateProcessingService = templateProcessingService ?? throw new ArgumentNullException(nameof(templateProcessingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates all index files including assembly overviews, namespace indexes, and table of contents
    /// </summary>
    /// <param name="uidMappings">UID mappings containing discovery information</param>
    /// <param name="outputDirectory">Base output directory</param>
    /// <param name="groupingStrategy">File grouping strategy being used</param>
    /// <returns>List of generated index file paths</returns>
    public async Task<List<string>> GenerateAllIndexFilesAsync(UidMappings uidMappings, string outputDirectory, string groupingStrategy)
    {
        if (uidMappings == null)
            throw new ArgumentNullException(nameof(uidMappings));
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty", nameof(outputDirectory));

        _logger.LogInformation("Starting index file generation in {OutputDirectory} with {GroupingStrategy} strategy", 
            outputDirectory, groupingStrategy);

        var generatedFiles = new List<string>();

        try
        {
            // Generate table of contents (always at root level)
            var tocPath = await GenerateTableOfContentsAsync(uidMappings, outputDirectory, groupingStrategy);
            if (!string.IsNullOrEmpty(tocPath))
            {
                generatedFiles.Add(tocPath);
            }

            // Generate assembly overview pages
            var assemblyIndexPaths = await GenerateAssemblyIndexesAsync(uidMappings, outputDirectory, groupingStrategy);
            generatedFiles.AddRange(assemblyIndexPaths);

            // Generate namespace index files
            var namespaceIndexPaths = await GenerateNamespaceIndexesAsync(uidMappings, outputDirectory, groupingStrategy);
            generatedFiles.AddRange(namespaceIndexPaths);

            _logger.LogInformation("Index file generation completed: {FileCount} files generated", generatedFiles.Count);
            return generatedFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during index file generation");
            throw;
        }
    }

    /// <summary>
    /// Generates a table of contents file listing all assemblies and namespaces
    /// </summary>
    public async Task<string> GenerateTableOfContentsAsync(UidMappings uidMappings, string outputDirectory, string groupingStrategy)
    {
        if (uidMappings == null)
            throw new ArgumentNullException(nameof(uidMappings));
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty", nameof(outputDirectory));

        _logger.LogDebug("Generating table of contents");

        var tocData = new TableOfContentsData
        {
            Title = "API Documentation",
            GeneratedDate = DateTime.UtcNow,
            GroupingStrategy = groupingStrategy,
            Assemblies = uidMappings.Assemblies.Select(assembly => new AssemblyTocEntry
            {
                Name = assembly,
                Path = GetAssemblyIndexPath(assembly, groupingStrategy),
                Namespaces = GetNamespacesForAssembly(uidMappings, assembly)
                    .Select(ns => new NamespaceTocEntry
                    {
                        Name = ns,
                        Path = GetNamespaceIndexPath(ns, assembly, groupingStrategy),
                        TypeCount = GetTypeCountForNamespace(uidMappings, ns)
                    }).ToList()
            }).ToList(),
            TotalTypes = uidMappings.TotalUids,
            TotalAssemblies = uidMappings.Assemblies.Count(),
            TotalNamespaces = uidMappings.Namespaces.Count()
        };

        var renderedContent = _templateProcessingService.RenderTableOfContents(tocData);
        var outputPath = Path.Combine(outputDirectory, "README.md");

        await WriteFileAsync(outputPath, renderedContent);
        _logger.LogDebug("Generated table of contents at {OutputPath}", outputPath);

        return outputPath;
    }

    /// <summary>
    /// Generates assembly overview pages for all assemblies
    /// </summary>
    public async Task<List<string>> GenerateAssemblyIndexesAsync(UidMappings uidMappings, string outputDirectory, string groupingStrategy)
    {
        var generatedFiles = new List<string>();

        foreach (var assembly in uidMappings.Assemblies)
        {
            try
            {
                var assemblyIndexPath = await GenerateAssemblyIndexAsync(uidMappings, outputDirectory, assembly, groupingStrategy);
                if (!string.IsNullOrEmpty(assemblyIndexPath))
                {
                    generatedFiles.Add(assemblyIndexPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating assembly index for {Assembly}", assembly);
            }
        }

        _logger.LogDebug("Generated {Count} assembly index files", generatedFiles.Count);
        return generatedFiles;
    }

    /// <summary>
    /// Generates a single assembly overview page
    /// </summary>
    public async Task<string> GenerateAssemblyIndexAsync(UidMappings uidMappings, string outputDirectory, string assembly, string groupingStrategy)
    {
        _logger.LogDebug("Generating assembly index for {Assembly}", assembly);

        var assemblyData = new AssemblyIndexData
        {
            Name = assembly,
            GeneratedDate = DateTime.UtcNow,
            Namespaces = GetNamespacesForAssembly(uidMappings, assembly)
                .Select(ns => new NamespaceIndexEntry
                {
                    Name = ns,
                    Path = GetNamespaceIndexPath(ns, assembly, groupingStrategy),
                    Types = GetTypesForNamespace(uidMappings, ns)
                        .Select(type => new TypeIndexEntry
                        {
                            Name = GetTypeDisplayName(type.Value),
                            FullName = type.Key,
                            Kind = GetTypeKind(type.Value),
                            Path = GetRelativePathForType(uidMappings, type.Key, assembly, groupingStrategy)
                        }).ToList()
                }).ToList(),
            TotalNamespaces = GetNamespacesForAssembly(uidMappings, assembly).Count(),
            TotalTypes = GetTypesForAssembly(uidMappings, assembly).Count()
        };

        var renderedContent = _templateProcessingService.RenderAssemblyIndex(assemblyData);
        var outputPath = GetAssemblyIndexFilePath(outputDirectory, assembly, groupingStrategy);

        await WriteFileAsync(outputPath, renderedContent);
        _logger.LogDebug("Generated assembly index at {OutputPath}", outputPath);

        return outputPath;
    }

    /// <summary>
    /// Generates namespace index files for all namespaces
    /// </summary>
    public async Task<List<string>> GenerateNamespaceIndexesAsync(UidMappings uidMappings, string outputDirectory, string groupingStrategy)
    {
        var generatedFiles = new List<string>();

        foreach (var @namespace in uidMappings.Namespaces)
        {
            try
            {
                var namespaceIndexPath = await GenerateNamespaceIndexAsync(uidMappings, outputDirectory, @namespace, groupingStrategy);
                if (!string.IsNullOrEmpty(namespaceIndexPath))
                {
                    generatedFiles.Add(namespaceIndexPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating namespace index for {Namespace}", @namespace);
            }
        }

        _logger.LogDebug("Generated {Count} namespace index files", generatedFiles.Count);
        return generatedFiles;
    }

    /// <summary>
    /// Generates a single namespace index page
    /// </summary>
    public async Task<string> GenerateNamespaceIndexAsync(UidMappings uidMappings, string outputDirectory, string @namespace, string groupingStrategy)
    {
        _logger.LogDebug("Generating namespace index for {Namespace}", @namespace);

        var namespaceData = new NamespaceIndexData
        {
            Name = @namespace,
            GeneratedDate = DateTime.UtcNow,
            Types = GetTypesForNamespace(uidMappings, @namespace)
                .GroupBy(type => GetTypeKind(type.Value))
                .Select(group => new TypeGroupIndexEntry
                {
                    Kind = group.Key,
                    Types = group.Select(type => new TypeIndexEntry
                    {
                        Name = GetTypeDisplayName(type.Value),
                        FullName = type.Key,
                        Kind = group.Key,
                        Path = GetRelativePathForType(uidMappings, type.Key, null, groupingStrategy),
                        Summary = GetTypeSummary(type.Value)
                    }).OrderBy(t => t.Name).ToList()
                }).OrderBy(g => GetTypeKindOrder(g.Kind)).ToList(),
            TotalTypes = GetTypeCountForNamespace(uidMappings, @namespace)
        };

        var renderedContent = _templateProcessingService.RenderNamespaceIndex(namespaceData);
        var outputPath = GetNamespaceIndexFilePath(outputDirectory, @namespace, groupingStrategy);

        await WriteFileAsync(outputPath, renderedContent);
        _logger.LogDebug("Generated namespace index at {OutputPath}", outputPath);

        return outputPath;
    }

    #region Helper Methods

    private IEnumerable<string> GetNamespacesForAssembly(UidMappings uidMappings, string assembly)
    {
        return uidMappings.UidToItem.Values
            .Where(item => item.Assemblies != null && item.Assemblies.Contains(assembly))
            .Where(item => !string.IsNullOrEmpty(item.Namespace))
            .Select(item => item.Namespace!)
            .Distinct()
            .OrderBy(ns => ns);
    }

    private IEnumerable<KeyValuePair<string, Item>> GetTypesForAssembly(UidMappings uidMappings, string assembly)
    {
        return uidMappings.UidToItem
            .Where(kvp => kvp.Value.Assemblies != null && kvp.Value.Assemblies.Contains(assembly))
            .Where(kvp => IsTopLevelType(kvp.Value.Type));
    }

    private IEnumerable<KeyValuePair<string, Item>> GetTypesForNamespace(UidMappings uidMappings, string @namespace)
    {
        return uidMappings.UidToItem
            .Where(kvp => kvp.Value.Namespace == @namespace)
            .Where(kvp => IsTopLevelType(kvp.Value.Type));
    }

    private int GetTypeCountForNamespace(UidMappings uidMappings, string @namespace)
    {
        return GetTypesForNamespace(uidMappings, @namespace).Count();
    }

    private static bool IsTopLevelType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Class or
            ItemType.Interface or
            ItemType.Struct or
            ItemType.Enum or
            ItemType.Delegate => true,
            _ => false
        };
    }

    private static string GetTypeKind(Item item)
    {
        return item.Type switch
        {
            ItemType.Class => "Classes",
            ItemType.Interface => "Interfaces",
            ItemType.Struct => "Structs",
            ItemType.Enum => "Enums",
            ItemType.Delegate => "Delegates",
            _ => "Other"
        };
    }

    private static int GetTypeKindOrder(string kind)
    {
        return kind switch
        {
            "Classes" => 1,
            "Interfaces" => 2,
            "Structs" => 3,
            "Enums" => 4,
            "Delegates" => 5,
            _ => 99
        };
    }

    private static string GetTypeDisplayName(Item item)
    {
        return item.Name ?? item.Uid ?? "Unknown";
    }

    private static string GetTypeSummary(Item item)
    {
        return item.Summary ?? string.Empty;
    }

    private string GetRelativePathForType(UidMappings uidMappings, string uid, string? fromAssembly, string groupingStrategy)
    {
        if (!uidMappings.UidToFilePath.TryGetValue(uid, out var typePath))
        {
            return "#";
        }

        // Return relative path - the actual calculation depends on where the index file is located
        return typePath;
    }

    private static string GetAssemblyIndexPath(string assembly, string groupingStrategy)
    {
        return groupingStrategy.ToLowerInvariant() switch
        {
            "assembly-namespace" or "assembly-flat" => $"{GetSafeDirectoryName(assembly)}/README.md",
            _ => $"assemblies/{GetSafeDirectoryName(assembly)}.md"
        };
    }

    private static string GetNamespaceIndexPath(string @namespace, string? assembly, string groupingStrategy)
    {
        var safeNamespace = GetSafeDirectoryName(@namespace);
        
        return groupingStrategy.ToLowerInvariant() switch
        {
            "namespace" => $"{safeNamespace}/README.md",
            "assembly-namespace" when !string.IsNullOrEmpty(assembly) => $"{GetSafeDirectoryName(assembly)}/{safeNamespace}/README.md",
            _ => $"namespaces/{safeNamespace}.md"
        };
    }

    private static string GetAssemblyIndexFilePath(string outputDirectory, string assembly, string groupingStrategy)
    {
        var relativePath = GetAssemblyIndexPath(assembly, groupingStrategy);
        return Path.Combine(outputDirectory, relativePath);
    }

    private static string GetNamespaceIndexFilePath(string outputDirectory, string @namespace, string groupingStrategy)
    {
        var relativePath = GetNamespaceIndexPath(@namespace, null, groupingStrategy);
        return Path.Combine(outputDirectory, relativePath);
    }

    private static string GetSafeDirectoryName(string name)
    {
        var invalidChars = Path.GetInvalidPathChars();
        var result = name;
        
        foreach (var c in invalidChars)
        {
            result = result.Replace(c, '_');
        }

        return result.Replace('.', '-').ToLowerInvariant();
    }

    private async Task WriteFileAsync(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content);
    }

    #endregion
}

#region Data Models for Index Generation

/// <summary>
/// Data model for table of contents generation
/// </summary>
public class TableOfContentsData
{
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public string GroupingStrategy { get; set; } = string.Empty;
    public List<AssemblyTocEntry> Assemblies { get; set; } = new();
    public int TotalTypes { get; set; }
    public int TotalAssemblies { get; set; }
    public int TotalNamespaces { get; set; }
}

/// <summary>
/// Assembly entry for table of contents
/// </summary>
public class AssemblyTocEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<NamespaceTocEntry> Namespaces { get; set; } = new();
}

/// <summary>
/// Namespace entry for table of contents
/// </summary>
public class NamespaceTocEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int TypeCount { get; set; }
}

/// <summary>
/// Data model for assembly index generation
/// </summary>
public class AssemblyIndexData
{
    public string Name { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public List<NamespaceIndexEntry> Namespaces { get; set; } = new();
    public int TotalNamespaces { get; set; }
    public int TotalTypes { get; set; }
}

/// <summary>
/// Namespace entry for assembly index
/// </summary>
public class NamespaceIndexEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<TypeIndexEntry> Types { get; set; } = new();
}

/// <summary>
/// Data model for namespace index generation
/// </summary>
public class NamespaceIndexData
{
    public string Name { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public List<TypeGroupIndexEntry> Types { get; set; } = new();
    public int TotalTypes { get; set; }
}

/// <summary>
/// Type group entry for namespace index (groups by type kind)
/// </summary>
public class TypeGroupIndexEntry
{
    public string Kind { get; set; } = string.Empty;
    public List<TypeIndexEntry> Types { get; set; } = new();
}

/// <summary>
/// Type entry for index generation
/// </summary>
public class TypeIndexEntry
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

#endregion