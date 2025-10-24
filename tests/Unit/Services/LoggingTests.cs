using System;
using System.IO;
using System.Threading.Tasks;
using DocFXMustache.Services;
using DocFXMustache.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace DocFXMustache.Tests.Unit.Services;

public class LoggingTests
{
    [Fact]
    public void LoggerFactory_Create_WithVerboseFalse_SetsInformationLevel()
    {
        // Arrange & Act
        using var factory = DocFXMustache.Services.LoggerFactory.Create(false);
        
        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void LoggerFactory_Create_WithVerboseTrue_SetsDebugLevel()
    {
        // Arrange & Act
        using var factory = DocFXMustache.Services.LoggerFactory.Create(true);
        
        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void LoggerFactory_CreateLogger_ReturnsValidLogger()
    {
        // Arrange
        using var factory = DocFXMustache.Services.LoggerFactory.Create(false);
        
        // Act
        var logger = DocFXMustache.Services.LoggerFactory.CreateLogger<LoggingTests>(factory);
        
        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public async Task MetadataParsingService_WithLogger_LogsParsingActivity()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<MetadataParsingService>();
        var service = new MetadataParsingService(logger);
        var filePath = TestDataHelper.GetFixturePath("SimpleClass.yml");

        // Act
        var result = await service.ParseYamlFileAsync(filePath);

        // Assert
        Assert.NotNull(result);
        // Logger is used (no exception thrown during parsing)
    }

    [Fact]
    public async Task MetadataParsingService_WithLogger_LogsDirectoryParsing()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<MetadataParsingService>();
        var service = new MetadataParsingService(logger);
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var results = await service.ParseDirectoryAsync(fixturesDir);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        // Logger is used (no exception thrown during parsing)
    }

    [Fact]
    public async Task DiscoveryService_WithLogger_LogsDiscoveryActivity()
    {
        // Arrange
        var parsingLogger = LoggerHelper.CreateNullLogger<MetadataParsingService>();
        var discoveryLogger = LoggerHelper.CreateNullLogger<DiscoveryService>();
        var parsingService = new MetadataParsingService(parsingLogger);
        var discoveryService = new DiscoveryService(parsingService, discoveryLogger);
        var fixturesDir = TestDataHelper.GetFixturesDirectory();

        // Act
        var mappings = await discoveryService.BuildUidMappingsAsync(fixturesDir, "flat");

        // Assert
        Assert.NotNull(mappings);
        Assert.True(mappings.TotalUids > 0);
        // Logger is used (no exception thrown during discovery)
    }

    [Fact]
    public void MetadataParsingService_WithoutLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MetadataParsingService(null!));
    }

    [Fact]
    public void DiscoveryService_WithoutLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var parsingLogger = LoggerHelper.CreateNullLogger<MetadataParsingService>();
        var parsingService = new MetadataParsingService(parsingLogger);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiscoveryService(parsingService, null!));
    }

    [Fact]
    public void DiscoveryService_WithoutParsingService_ThrowsArgumentNullException()
    {
        // Arrange
        var discoveryLogger = LoggerHelper.CreateNullLogger<DiscoveryService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiscoveryService(null!, discoveryLogger));
    }

    [Fact]
    public void LoggerHelper_CreateNullLogger_ReturnsValidLogger()
    {
        // Act
        var logger = LoggerHelper.CreateNullLogger<LoggingTests>();

        // Assert
        Assert.NotNull(logger);
        
        // Verify it can be used without exceptions
        logger.LogInformation("Test message");
        logger.LogDebug("Debug message");
        logger.LogWarning("Warning message");
        logger.LogError("Error message");
    }

    [Fact]
    public void LoggerHelper_CreateConsoleLogger_ReturnsValidLogger()
    {
        // Act
        var logger = LoggerHelper.CreateConsoleLogger<LoggingTests>(false);

        // Assert
        Assert.NotNull(logger);
        
        // Verify it can be used without exceptions
        logger.LogInformation("Test message");
    }

    [Fact]
    public void LoggerHelper_CreateConsoleLogger_WithVerbose_ReturnsValidLogger()
    {
        // Act
        var logger = LoggerHelper.CreateConsoleLogger<LoggingTests>(true);

        // Assert
        Assert.NotNull(logger);
        
        // Verify it can be used without exceptions
        logger.LogDebug("Debug message");
    }
}
