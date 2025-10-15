using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VYaml.Serialization;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Services;

/// <summary>
/// Service responsible for parsing DocFX YAML metadata files into strongly-typed models
/// </summary>
public class MetadataParsingService
{
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

        try
        {
            using var stream = File.OpenRead(filePath);
            var root = await YamlSerializer.DeserializeAsync<Root>(stream);
            
            if (root == null)
                throw new InvalidOperationException($"Failed to deserialize YAML file: {filePath}");

            return root;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is FileNotFoundException))
        {
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
        var results = new List<Root>();

        foreach (var filePath in yamlFiles)
        {
            try
            {
                var root = await ParseYamlFileAsync(filePath);
                results.Add(root);
            }
            catch (Exception ex)
            {
                // Log the error but continue processing other files
                Console.WriteLine($"Warning: Failed to parse {filePath}: {ex.Message}");
            }
        }

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
            return false;

        // Check that we have items
        if (metadata.Items == null || !metadata.Items.Any())
            return false;

        // Validate each item has required properties
        foreach (var item in metadata.Items)
        {
            if (string.IsNullOrEmpty(item.Uid))
                return false;
                
            if (string.IsNullOrEmpty(item.Name))
                return false;
        }

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

        return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
    }
}