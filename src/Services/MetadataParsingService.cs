using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VYaml.Serialization;
using DocFXMustache.Models.Yaml;
using Microsoft.Extensions.Logging;

namespace DocFXMustache.Services;

/// <summary>
/// Service responsible for parsing DocFX YAML metadata files into strongly-typed models
/// </summary>
public class MetadataParsingService
{
    private readonly ILogger<MetadataParsingService> _logger;

    /// <summary>
    /// Initializes a new instance of MetadataParsingService
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    public MetadataParsingService(ILogger<MetadataParsingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Parses a single YAML metadata file into a Root object
    /// </summary>
    /// <param name="filePath">Path to the YAML file</param>
    /// <returns>Parsed Root object containing items and references</returns>
    public async Task<Root> ParseYamlFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"YAML file not found: {filePath}");

        _logger.LogDebug("Parsing YAML file: {FilePath}", filePath);

        try
        {
            using var stream = File.OpenRead(filePath);
            var root = await YamlSerializer.DeserializeAsync<Root>(stream);
            
            if (root == null)
            {
                _logger.LogError("Failed to deserialize YAML file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to deserialize YAML file: {filePath}");
            }

            var itemCount = root.Items?.Count() ?? 0;
            _logger.LogDebug("Successfully parsed {FilePath} with {ItemCount} items", filePath, itemCount);

            return root;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is FileNotFoundException))
        {
            _logger.LogError(ex, "Error parsing YAML file: {FilePath}", filePath);
            throw new InvalidOperationException($"Error parsing YAML file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses all YAML files in a directory into Root objects
    /// </summary>
    /// <param name="directoryPath">Directory containing YAML files</param>
    /// <param name="searchPattern">File search pattern (default: "*.yml")</param>
    /// <returns>Enumerable of parsed Root objects</returns>
    public async Task<IEnumerable<Root>> ParseDirectoryAsync(string directoryPath, string searchPattern = "*.yml")
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var yamlFiles = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        _logger.LogInformation("Found {FileCount} YAML files in {Directory}", yamlFiles.Length, directoryPath);
        
        var results = new List<Root>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var filePath in yamlFiles)
        {
            try
            {
                var root = await ParseYamlFileAsync(filePath);
                results.Add(root);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse {FilePath}", filePath);
                failureCount++;
            }
        }

        _logger.LogInformation("Parsing completed: {SuccessCount} succeeded, {FailureCount} failed", 
            successCount, failureCount);

        return results;
    }

    /// <summary>
    /// Validates that a Root object has the expected structure
    /// </summary>
    /// <param name="metadata">Root object to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool ValidateMetadata(Root metadata)
    {
        if (metadata == null)
        {
            _logger.LogWarning("Metadata validation failed: metadata is null");
            return false;
        }

        // Check that we have items
        if (metadata.Items == null || !metadata.Items.Any())
        {
            _logger.LogWarning("Metadata validation failed: no items found");
            return false;
        }

        // Validate each item has required properties
        foreach (var item in metadata.Items)
        {
            if (string.IsNullOrEmpty(item.Uid))
            {
                _logger.LogWarning("Metadata validation failed: item missing UID");
                return false;
            }
                
            if (string.IsNullOrEmpty(item.Name))
            {
                _logger.LogWarning("Metadata validation failed: item {Uid} missing name", item.Uid);
                return false;
            }
        }

        _logger.LogDebug("Metadata validated successfully with {ItemCount} items", metadata.Items.Count());
        return true;
    }

    /// <summary>
    /// Gets all YAML files in a directory (for dry-run operations)
    /// </summary>
    /// <param name="directoryPath">Directory to scan</param>
    /// <param name="searchPattern">File search pattern (default: "*.yml")</param>
    /// <returns>Array of file paths</returns>
    public string[] GetYamlFiles(string directoryPath, string searchPattern = "*.yml")
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        _logger.LogDebug("Found {FileCount} YAML files in {Directory}", files.Length, directoryPath);
        
        return files;
    }
}