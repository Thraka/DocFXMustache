using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFXMustache.Services;
using DocFXMustache.Tests.Helpers;

namespace DocFXMustache.Tests.Unit.Services;

public class DiscoveryServiceTests : IDisposable
{
    private readonly MetadataParsingService _parsingService;
    private readonly DiscoveryService _discoveryService;

    public DiscoveryServiceTests()
    {
        _parsingService = new MetadataParsingService();
        _discoveryService = new DiscoveryService(_parsingService);
    }

    #region BuildUidMappingsAsync Tests

    [Fact]
    public async Task BuildUidMappingsAsync_WithRealSadConsoleData_ReturnsMappings()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "flat");

        // Assert
        Assert.NotNull(mappings);
        Assert.NotEmpty(mappings.UidToFilePath);
        Assert.NotEmpty(mappings.UidToItem);
        // Real SadConsole data should have many more UIDs than simple test data
        Assert.True(mappings.TotalUids > 100, $"Expected many UIDs from SadConsole data, got {mappings.TotalUids}");
    }

    [Fact]
    public async Task BuildUidMappingsAsync_FindsSadConsoleClasses()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "flat");

        // Assert
        Assert.Contains("SadConsole.ColoredGlyph", mappings.UidToItem.Keys);
        Assert.Contains("SadConsole.ColoredGlyphBase", mappings.UidToItem.Keys);
        Assert.Contains("SadConsole.Mirror", mappings.UidToItem.Keys);
        Assert.Contains("SadConsole.ScreenSurface", mappings.UidToItem.Keys);
        Assert.Contains("SadConsole.ScreenObject", mappings.UidToItem.Keys);
        Assert.Contains("SadConsole.IScreenSurface", mappings.UidToItem.Keys);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_WithFlatGrouping_BuildsFlatPaths()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "flat");

        // Assert
        foreach (var path in mappings.UidToFilePath.Values)
        {
            // Flat grouping should not have directory separators
            var relativePath = path;
            Assert.DoesNotContain(Path.DirectorySeparatorChar.ToString(), relativePath.TrimEnd(".md".ToCharArray()));
        }
    }

    [Fact]
    public async Task BuildUidMappingsAsync_WithNamespaceGrouping_CreatesNamespaceDirs()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "namespace");

        // Assert
        var pathsWithDirs = mappings.UidToFilePath.Values
            .Where(p => p.Contains("/") || p.Contains(Path.DirectorySeparatorChar.ToString())).ToList();
        Assert.NotEmpty(pathsWithDirs);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_WithAssemblyFlatGrouping_CreatesAssemblyDirs()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "assembly-flat");

        // Assert
        Assert.NotEmpty(mappings.AssemblyMappings);
        var pathsWithAssembly = mappings.UidToFilePath.Values
            .Where(p => p.Contains("/") || p.Contains(Path.DirectorySeparatorChar.ToString())).ToList();
        Assert.NotEmpty(pathsWithAssembly);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_WithAssemblyNamespaceGrouping_CreatesHierarchy()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "assembly-namespace");

        // Assert
        Assert.NotEmpty(mappings.AssemblyMappings);
        Assert.NotEmpty(mappings.NamespaceMappings);
        var pathsWithHierarchy = mappings.UidToFilePath.Values
            .Where(p => (p.Count(c => c == '/') + p.Count(c => c == Path.DirectorySeparatorChar)) >= 2).ToList();
        Assert.NotEmpty(pathsWithHierarchy);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_PopulatesAssemblies()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "assembly-flat");

        // Assert
        Assert.NotEmpty(mappings.Assemblies);
        Assert.Contains("TestAssembly", mappings.Assemblies);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_PopulatesNamespaces()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "namespace");

        // Assert
        Assert.NotEmpty(mappings.Namespaces);
        Assert.Contains("TestNamespace", mappings.Namespaces);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_CalculatesTotalUids()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await _discoveryService.BuildUidMappingsAsync(fixturesDir, "flat");

        // Assert
        Assert.True(mappings.TotalUids > 0);
        Assert.Equal(mappings.UidToItem.Count, mappings.TotalUids);
    }

    [Fact]
    public async Task BuildUidMappingsAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            _discoveryService.BuildUidMappingsAsync("/path/does/not/exist", "flat"));
    }

    [Fact]
    public async Task BuildUidMappingsAsync_WithInvalidGrouping_ThrowsArgumentException()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _discoveryService.BuildUidMappingsAsync(fixturesDir, "invalid-strategy"));
    }

    #endregion

    #region ExtractUidsFromMetadata Tests

    [Fact]
    public async Task ExtractUidsFromMetadata_WithValidMetadata_ReturnsUidDictionary()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SimpleClass.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);

        // Act
        var uids = _discoveryService.ExtractUidsFromMetadata(metadata);

        // Assert
        Assert.NotEmpty(uids);
        Assert.Contains("DocFXMustache.Models.TypeDocumentation", uids.Keys);
    }

    [Fact]
    public async Task ExtractUidsFromMetadata_WithMultipleItems_ExtractsAllUids()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("ClassWithMembers.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);

        // Act
        var uids = _discoveryService.ExtractUidsFromMetadata(metadata);

        // Assert
        Assert.Equal(3, uids.Count);
        Assert.Contains("TestNamespace.TestClass", uids.Keys);
        Assert.Contains("TestNamespace.TestClass.TestMethod", uids.Keys);
    }

    [Fact]
    public void ExtractUidsFromMetadata_WithNullMetadata_ReturnsEmptyDictionary()
    {
        // Act
        var uids = _discoveryService.ExtractUidsFromMetadata(null!);

        // Assert
        Assert.Empty(uids);
    }

    #endregion

    #region DetermineOutputPath Tests

    [Fact]
    public async Task DetermineOutputPath_WithSadConsoleClass_AppliesFlatStrategy()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.ColoredGlyph.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act
        var outputPath = _discoveryService.DetermineOutputPath(item, "flat");

        // Assert
        Assert.DoesNotContain(Path.DirectorySeparatorChar.ToString(), outputPath);
        Assert.EndsWith(".md", outputPath);
        Assert.Contains("coloredglyph", outputPath.ToLowerInvariant());
    }

    [Fact]
    public async Task DetermineOutputPath_WithSadConsoleClass_AppliesNamespaceStrategy()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.Mirror.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act
        var outputPath = _discoveryService.DetermineOutputPath(item, "namespace");

        // Assert
        Assert.True(outputPath.Contains("/") || outputPath.Contains(Path.DirectorySeparatorChar.ToString()),
            $"Expected path to contain directory separator, got: {outputPath}");
        Assert.Contains("sadconsole", outputPath.ToLowerInvariant());
        Assert.EndsWith(".md", outputPath);
    }

    [Fact]
    public async Task DetermineOutputPath_WithComplexMetadata_GeneratesCorrectPaths()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.Console.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act
        var flatPath = _discoveryService.DetermineOutputPath(item, "flat");
        var namespacePath = _discoveryService.DetermineOutputPath(item, "namespace");

        // Assert
        Assert.DoesNotContain(Path.DirectorySeparatorChar.ToString(), flatPath);
        Assert.True(namespacePath.Contains("/") || namespacePath.Contains(Path.DirectorySeparatorChar.ToString()));
        Assert.EndsWith(".md", flatPath);
        Assert.EndsWith(".md", namespacePath);
    }

    [Fact]
    public async Task DetermineOutputPath_WithInvalidStrategy_ThrowsArgumentException()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SimpleClass.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _discoveryService.DetermineOutputPath(item, "invalid-strategy"));
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}
