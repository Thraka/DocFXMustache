using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFXMustache.Models;
using DocFXMustache.Models.Yaml;
using DocFXMustache.Services;
using DocFXMustache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DocFXMustache.Tests.Unit.Services;

/// <summary>
/// Unit tests for IndexGenerationService
/// </summary>
public class IndexGenerationServiceTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<IndexGenerationService> _logger;
    private readonly ILogger<TemplateProcessingService> _templateLogger;

    public IndexGenerationServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = LoggerHelper.CreateConsoleLogger<IndexGenerationService>(true);
        _templateLogger = LoggerHelper.CreateConsoleLogger<TemplateProcessingService>(true);
    }

    [Fact]
    public async Task GenerateTableOfContentsAsync_WithValidData_ShouldGenerateCorrectContent()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);

        var uidMappings = CreateTestUidMappings();

        try
        {
            // Act
            var tocPath = await service.GenerateTableOfContentsAsync(uidMappings, tempDir, "namespace");

            // Assert
            Assert.NotNull(tocPath);
            Assert.True(File.Exists(tocPath));
            
            var content = await File.ReadAllTextAsync(tocPath);
            Assert.Contains("API Documentation", content);
            Assert.Contains("TestAssembly", content);
            Assert.Contains("TestNamespace", content);
            Assert.Contains("5 types", content); // Total types in test data
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    [Fact]
    public async Task GenerateAssemblyIndexAsync_WithValidData_ShouldGenerateCorrectContent()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);

        var uidMappings = CreateTestUidMappings();

        try
        {
            // Act
            var assemblyIndexPath = await service.GenerateAssemblyIndexAsync(uidMappings, tempDir, "TestAssembly", "namespace");

            // Assert
            Assert.NotNull(assemblyIndexPath);
            Assert.True(File.Exists(assemblyIndexPath));
            
            var content = await File.ReadAllTextAsync(assemblyIndexPath);
            Assert.Contains("TestAssembly Assembly", content);
            Assert.Contains("TestNamespace", content);
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    [Fact]
    public async Task GenerateNamespaceIndexAsync_WithValidData_ShouldGenerateCorrectContent()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);

        var uidMappings = CreateTestUidMappings();

        try
        {
            // Act
            var namespaceIndexPath = await service.GenerateNamespaceIndexAsync(uidMappings, tempDir, "TestNamespace", "namespace");

            // Assert
            Assert.NotNull(namespaceIndexPath);
            Assert.True(File.Exists(namespaceIndexPath));
            
            var content = await File.ReadAllTextAsync(namespaceIndexPath);
            Assert.Contains("TestNamespace Namespace", content);
            Assert.Contains("Classes", content);
            Assert.Contains("Interfaces", content);
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    [Fact]
    public async Task GenerateAllIndexFilesAsync_WithValidData_ShouldGenerateAllIndexFiles()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);

        var uidMappings = CreateTestUidMappings();

        try
        {
            // Act
            var generatedFiles = await service.GenerateAllIndexFilesAsync(uidMappings, tempDir, "namespace");

            // Assert
            Assert.NotEmpty(generatedFiles);
            Assert.True(generatedFiles.Count >= 3); // TOC + at least 1 assembly + at least 1 namespace

            // Verify table of contents was generated
            Assert.Contains(generatedFiles, f => Path.GetFileName(f) == "README.md");

            // Verify all files exist
            foreach (var file in generatedFiles)
            {
                Assert.True(File.Exists(file), $"Generated file does not exist: {file}");
            }
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    [Theory]
    [InlineData("flat")]
    [InlineData("namespace")]
    [InlineData("assembly-namespace")]
    [InlineData("assembly-flat")]
    public async Task GenerateAllIndexFilesAsync_WithDifferentGroupingStrategies_ShouldGenerateAppropriateFiles(string groupingStrategy)
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);

        var uidMappings = CreateTestUidMappings();

        try
        {
            // Act
            var generatedFiles = await service.GenerateAllIndexFilesAsync(uidMappings, tempDir, groupingStrategy);

            // Assert
            Assert.NotEmpty(generatedFiles);

            // TOC should always be generated
            Assert.Contains(generatedFiles, f => Path.GetFileName(f) == "README.md");

            _output.WriteLine($"Generated {generatedFiles.Count} files for {groupingStrategy} strategy:");
            foreach (var file in generatedFiles)
            {
                var relativePath = Path.GetRelativePath(tempDir, file);
                _output.WriteLine($"  - {relativePath}");
            }
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    [Fact]
    public async Task GenerateTableOfContentsAsync_WithNullUidMappings_ShouldThrowArgumentNullException()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.GenerateTableOfContentsAsync(null!, tempDir, "namespace"));
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(tempDir);
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    [Fact]
    public async Task GenerateAllIndexFilesAsync_WithEmptyOutputDirectory_ShouldThrowArgumentException()
    {
        // Arrange
        var templateDir = CreateTestTemplateDirectory();
        var templateService = new TemplateProcessingService(templateDir, _templateLogger);
        var service = new IndexGenerationService(templateService, _logger);
        var uidMappings = CreateTestUidMappings();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateAllIndexFilesAsync(uidMappings, string.Empty, "namespace"));
        }
        finally
        {
            TestDataHelper.CleanupTempDirectory(templateDir);
        }
    }

    #region Helper Methods

    private UidMappings CreateTestUidMappings()
    {
        var mappings = new UidMappings();

        // Create test items using reflection since the Item class has private init properties
        var testClass = CreateTestItem("TestNamespace.TestClass", "TestClass", "Class", "TestNamespace", new[] { "TestAssembly" }, "A test class for unit testing");
        var testInterface = CreateTestItem("TestNamespace.ITestInterface", "ITestInterface", "Interface", "TestNamespace", new[] { "TestAssembly" }, "A test interface for unit testing");
        var testEnum = CreateTestItem("TestNamespace.TestEnum", "TestEnum", "Enum", "TestNamespace", new[] { "TestAssembly" }, "A test enum for unit testing");
        var testStruct = CreateTestItem("TestNamespace.TestStruct", "TestStruct", "Struct", "TestNamespace", new[] { "TestAssembly" }, "A test struct for unit testing");
        var testDelegate = CreateTestItem("TestNamespace.TestDelegate", "TestDelegate", "Delegate", "TestNamespace", new[] { "TestAssembly" }, "A test delegate for unit testing");

        // Add items to mappings
        mappings.UidToItem[testClass.Uid!] = testClass;
        mappings.UidToItem[testInterface.Uid!] = testInterface;
        mappings.UidToItem[testEnum.Uid!] = testEnum;
        mappings.UidToItem[testStruct.Uid!] = testStruct;
        mappings.UidToItem[testDelegate.Uid!] = testDelegate;

        // Add file path mappings
        mappings.UidToFilePath[testClass.Uid!] = "testnamespace/testclass.md";
        mappings.UidToFilePath[testInterface.Uid!] = "testnamespace/itestinterface.md";
        mappings.UidToFilePath[testEnum.Uid!] = "testnamespace/testenum.md";
        mappings.UidToFilePath[testStruct.Uid!] = "testnamespace/teststruct.md";
        mappings.UidToFilePath[testDelegate.Uid!] = "testnamespace/testdelegate.md";

        // Add assembly and namespace mappings
        mappings.AssemblyMappings["TestAssembly"] = "testassembly";
        mappings.NamespaceMappings["TestNamespace"] = "testnamespace";

        return mappings;
    }

    private Item CreateTestItem(string uid, string name, string typeString, string @namespace, string[] assemblies, string summary)
    {
        // Use VYaml deserialization to create an Item with the proper structure
        var yaml = $@"
uid: {uid}
name: {name}
type: {typeString}
namespace: {@namespace}
summary: {summary}
assemblies:
  - {string.Join("\n  - ", assemblies)}
";
        
        // For simplicity in tests, we'll create a simple test factory method
        // In a real scenario, we'd use VYaml, but for testing this is sufficient
        return ItemTestFactory.Create(uid, name, typeString, @namespace, assemblies, summary);
    }

    private string CreateTestTemplateDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DocFXMustacheTest_Templates_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        // Create simple test templates
        File.WriteAllText(Path.Combine(tempDir, "toc.mustache"), @"# {{Title}}

Generated: {{GeneratedDate}}
Strategy: {{GroupingStrategy}}

{{#Assemblies}}
## {{Name}}
{{#Namespaces}}
- [{{Name}}]({{Path}}) ({{TypeCount}} types)
{{/Namespaces}}
{{/Assemblies}}

Total: {{TotalTypes}} types");

        File.WriteAllText(Path.Combine(tempDir, "assembly-index.mustache"), @"# {{Name}} Assembly

Generated: {{GeneratedDate}}

{{#Namespaces}}
## {{Name}}
{{#Types}}
- [{{Name}}]({{Path}}) ({{Kind}})
{{/Types}}
{{/Namespaces}}

Total: {{TotalTypes}} types");

        File.WriteAllText(Path.Combine(tempDir, "namespace-index.mustache"), @"# {{Name}} Namespace

Generated: {{GeneratedDate}}

{{#Types}}
## {{Kind}}
{{#Types}}
- [{{Name}}]({{Path}}) - {{Summary}}
{{/Types}}
{{/Types}}

Total: {{TotalTypes}} types");

        File.WriteAllText(Path.Combine(tempDir, "template.json"), @"{
  ""name"": ""Test Template"",
  ""version"": ""1.0.0"",
  ""outputFormat"": ""md"",
  ""fileGrouping"": ""namespace"",
  ""filenameCase"": ""lowercase"",
  ""combineMembers"": true,
  ""templates"": {
    ""class"": ""class.mustache"",
    ""interface"": ""interface.mustache"",
    ""struct"": ""struct.mustache"",
    ""enum"": ""enum.mustache"",
    ""delegate"": ""delegate.mustache"",
    ""member"": ""member.mustache"",
    ""link"": ""link.mustache""
  }
}");

        return tempDir;
    }

    private string CreateTempDirectory()
    {
        return TestDataHelper.CreateTempDirectory();
    }

    #endregion
}
