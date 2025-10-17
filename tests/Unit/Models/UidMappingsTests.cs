using System.Collections.Generic;
using DocFXMustache.Models;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Tests.Unit.Models;

public class UidMappingsTests
{
    #region Initialization Tests

    [Fact]
    public void UidMappings_NewInstance_InitializesEmptyDictionaries()
    {
        // Act
        var mappings = new UidMappings();

        // Assert
        Assert.Empty(mappings.UidToFilePath);
        Assert.Empty(mappings.UidToItem);
        Assert.Empty(mappings.AssemblyMappings);
        Assert.Empty(mappings.NamespaceMappings);
        Assert.Equal(0, mappings.TotalUids);
    }

    #endregion

    #region Assemblies Property Tests

    [Fact]
    public void UidMappings_Assemblies_ReturnsAssemblyMappingKeys()
    {
        // Arrange
        var mappings = new UidMappings();
        mappings.AssemblyMappings.Add("Assembly1", "path1");
        mappings.AssemblyMappings.Add("Assembly2", "path2");

        // Act
        var assemblies = mappings.Assemblies;

        // Assert
        Assert.Contains("Assembly1", assemblies);
        Assert.Contains("Assembly2", assemblies);
    }

    #endregion

    #region Namespaces Property Tests

    [Fact]
    public void UidMappings_Namespaces_ReturnsNamespaceMappingKeys()
    {
        // Arrange
        var mappings = new UidMappings();
        mappings.NamespaceMappings.Add("Namespace1", "path1");
        mappings.NamespaceMappings.Add("Namespace2", "path2");

        // Act
        var namespaces = mappings.Namespaces;

        // Assert
        Assert.Contains("Namespace1", namespaces);
        Assert.Contains("Namespace2", namespaces);
    }

    #endregion

    #region Dictionary Operations Tests

    [Fact]
    public void UidMappings_CanPopulateDictionaries()
    {
        // Arrange
        var mappings = new UidMappings();

        // Act
        mappings.UidToFilePath.Add("uid1", "file1.md");
        mappings.AssemblyMappings.Add("Assembly1", "assembly_path");
        mappings.NamespaceMappings.Add("Namespace1", "namespace_path");

        // Assert
        Assert.Single(mappings.UidToFilePath);
        Assert.Single(mappings.AssemblyMappings);
        Assert.Single(mappings.NamespaceMappings);
    }

    #endregion
}
