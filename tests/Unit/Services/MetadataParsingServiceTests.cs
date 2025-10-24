using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFXMustache.Services;
using DocFXMustache.Tests.Helpers;

namespace DocFXMustache.Tests.Unit.Services;

public class MetadataParsingServiceTests : IDisposable
{
    private readonly MetadataParsingService _service;

    public MetadataParsingServiceTests()
    {
        var logger = LoggerHelper.CreateNullLogger<MetadataParsingService>();
        _service = new MetadataParsingService(logger);
    }

    #region ParseYamlFileAsync Tests

    [Fact]
    public async Task ParseYamlFileAsync_WithValidFile_ReturnsRootObject()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.Mirror.yml");

        // Act
        var result = await _service.ParseYamlFileAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task ParseYamlFileAsync_WithSadConsoleClass_ParsesCorrectly()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.ColoredGlyph.yml");

        // Act
        var result = await _service.ParseYamlFileAsync(filePath);

        // Assert
        var item = result.Items.First();
        Assert.Equal("SadConsole.ColoredGlyph", item.Uid);
        Assert.Equal("ColoredGlyph", item.Name);
        Assert.Equal("SadConsole", item.Namespace);
    }

    [Fact]
    public async Task ParseYamlFileAsync_WithLargeFile_ParsesAllItems()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.ScreenSurface.yml");

        // Act
        var result = await _service.ParseYamlFileAsync(filePath);

        // Assert
        Assert.NotNull(result.Items);
        Assert.NotEmpty(result.Items);
        Assert.Contains(result.Items, item => item.Uid == "SadConsole.ScreenSurface");
    }

    [Fact]
    public async Task ParseYamlFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = "/path/that/does/not/exist.yml";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _service.ParseYamlFileAsync(filePath));
    }

    #endregion

    #region ParseDirectoryAsync Tests

    [Fact]
    public async Task ParseDirectoryAsync_WithMultipleFiles_ParsesAllFiles()
    {
        // Arrange
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var results = await _service.ParseDirectoryAsync(fixturesDir);

        // Assert
        Assert.NotNull(results);
        var resultsList = results.ToList();
        Assert.NotEmpty(resultsList);
        // We have 3 fixture files
        Assert.True(resultsList.Count >= 3);
    }

    [Fact]
    public async Task ParseDirectoryAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => 
            _service.ParseDirectoryAsync("/path/that/does/not/exist"));
    }

    [Fact]
    public async Task ParseDirectoryAsync_WithCustomSearchPattern_OnlyParseMatchingFiles()
    {
        // Arrange
        var tempDir = TestDataHelper.CreateTempDirectory("parse_directory_test");
        try
        {
            // Copy a fixture to temp directory with .yaml extension
            var sourceFile = TestDataHelper.GetFixturePath("SimpleClass.yml");
            var targetFile = Path.Combine(tempDir, "test.yaml");
            File.Copy(sourceFile, targetFile);

            // Act - Search for .yaml files only
            var results = await _service.ParseDirectoryAsync(tempDir, "*.yaml");

            // Assert
            Assert.NotEmpty(results);
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
        }
    }

    #endregion

    #region ValidateMetadata Tests

    [Fact]
    public async Task ValidateMetadata_WithValidMetadata_ReturnsTrue()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SimpleClass.yml");
        var metadata = await _service.ParseYamlFileAsync(filePath);

        // Act
        var result = _service.ValidateMetadata(metadata);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateMetadata_WithNullMetadata_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateMetadata(null!);

        // Assert
        Assert.False(result);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}
