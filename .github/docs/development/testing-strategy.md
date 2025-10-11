# Testing Strategy

## Overview

The testing strategy ensures reliability, correctness, and maintainability through comprehensive unit tests, integration tests, and validation scenarios.

## Test Categories

### Unit Tests
Focus on individual components and their isolated behavior:

#### Model Testing
- **YAML Serialization/Deserialization**: Validate that DocFX metadata correctly maps to data models
- **Data Model Validation**: Ensure model properties and relationships work correctly
- **Type Conversion**: Test conversion between YAML structures and internal models

#### Service Testing
- **Metadata Parsing**: Validate parsing of various YAML structures
- **UID Resolution**: Test UID mapping and path resolution logic
- **Template Processing**: Test Mustache template rendering with sample data
- **Link Processing**: Validate XRef tag processing and link resolution

#### CLI Testing
- **Parameter Parsing**: Validate command-line argument handling
- **Configuration Loading**: Test configuration file processing
- **Error Handling**: Validate error messages and exit codes

### Integration Tests
Test complete workflows and component interactions:

#### End-to-End Processing
- **Complete Pipeline**: Process sample metadata from input to output
- **Multiple Grouping Strategies**: Test all file organization approaches
- **Link Resolution**: Validate cross-references work correctly across grouping strategies
- **Template Customization**: Test custom templates with real data

#### Error Scenarios
- **Malformed YAML**: Handle invalid or corrupted metadata files
- **Missing References**: Handle broken UID references gracefully
- **File System Issues**: Handle read/write permission problems
- **Template Errors**: Handle template compilation and rendering failures

### Performance Tests
Ensure the system handles large documentation sets efficiently:

#### Load Testing
- **Large API Sets**: Process 1000+ API items
- **Deep Namespace Hierarchies**: Handle complex namespace structures
- **Memory Usage**: Monitor memory consumption during processing
- **Processing Time**: Benchmark generation speed

## Test Data

### Sample Metadata Files
Located in `.github\reference-files\api\`:

#### Class Examples
- `SadConsole.ColoredGlyph.yml` - Standard class with properties and methods
- Complex inheritance scenarios
- Generic type examples

#### Namespace Examples  
- `SadConsole.yml` - Namespace overview
- `Microsoft.Xna.Framework.yml` - External namespace reference
- Nested namespace structures

#### Specialized Cases
- `Microsoft.Xna.Framework.Graphics.MonoGame_TextureExtensions.yml` - Extension methods
- Interface implementations
- Enum types
- Delegate types

### Reference Models
Located in `.github\reference-files\Models\`:
- Pre-built model implementations for validation
- Expected data structures for comparison
- Baseline for model adaptation

### Expected Output Files
- Generated Markdown/MDX files for comparison testing
- Index files for assemblies and namespaces
- Link resolution validation files

### Custom Template Examples
- Basic templates for each API type
- Advanced templates with custom formatting
- Templates with custom helpers
- Error case templates

## Test Implementation

### Unit Test Structure
```csharp
[TestClass]
public class MetadataParsingServiceTests
{
    [TestMethod]
    public void ParseYaml_ValidClassMetadata_ReturnsCorrectModel()
    {
        // Arrange
        var yamlContent = LoadTestFile("SadConsole.ColoredGlyph.yml");
        var parser = new MetadataParsingService();
        
        // Act
        var result = parser.ParseYaml(yamlContent);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("SadConsole.ColoredGlyph", result.FullName);
        Assert.AreEqual(ItemType.Class, result.Type);
    }
}
```

### Integration Test Structure
```csharp
[TestClass]
public class EndToEndTests
{
    [TestMethod]
    public async Task GenerateDocumentation_CompleteWorkflow_ProducesExpectedOutput()
    {
        // Arrange
        var inputDirectory = TestDataHelper.GetApiDirectory();
        var outputDirectory = TestDataHelper.GetTempOutputDirectory();
        var configuration = new Configuration
        {
            Input = inputDirectory,
            Output = outputDirectory,
            Format = "md",
            Grouping = "namespace"
        };
        
        // Act
        var generator = new DocumentationGenerator(configuration);
        await generator.GenerateAsync();
        
        // Assert
        Assert.IsTrue(Directory.Exists(outputDirectory));
        Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "SadConsole", "ColoredGlyph.md")));
        
        var content = File.ReadAllText(Path.Combine(outputDirectory, "SadConsole", "ColoredGlyph.md"));
        Assert.IsTrue(content.Contains("# ColoredGlyph"));
    }
}
```

### Test Data Helpers
```csharp
public static class TestDataHelper
{
    public static string GetApiDirectory() => 
        Path.Combine(GetRepositoryRoot(), ".github", "reference-files", "api");
    
    public static string GetModelsDirectory() => 
        Path.Combine(GetRepositoryRoot(), ".github", "reference-files", "Models");
    
    public static string LoadTestFile(string filename) =>
        File.ReadAllText(Path.Combine(GetApiDirectory(), filename));
    
    public static string GetTempOutputDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
}
```

## Test Scenarios

### Basic Functionality Tests
1. **Parse Single Class**: Load and parse a simple class metadata file
2. **Generate Markdown**: Create basic Markdown output from parsed data
3. **Resolve Simple Links**: Handle basic UID to path resolution
4. **Apply Templates**: Render content using default templates

### Complex Scenario Tests
1. **Large API Set**: Process entire SadConsole API
2. **Cross-Assembly References**: Handle references between different assemblies
3. **Deep Inheritance**: Handle complex inheritance hierarchies
4. **Generic Types**: Process generic classes and methods correctly

### Error Handling Tests
1. **Malformed YAML**: Invalid YAML structure
2. **Missing UIDs**: References to non-existent types
3. **Template Errors**: Invalid template syntax
4. **File System Errors**: Permission denied, disk full, etc.

### Regression Tests
1. **Output Stability**: Ensure output remains consistent across versions
2. **Performance Stability**: Monitor performance regression
3. **Configuration Compatibility**: Maintain backward compatibility

## Validation Approaches

### Golden File Testing
Compare generated output against known-good reference files:
```csharp
[TestMethod]
public void GenerateClassMarkdown_SadConsoleColoredGlyph_MatchesGoldenFile()
{
    var output = generator.GenerateMarkdown("SadConsole.ColoredGlyph.yml");
    var expected = File.ReadAllText("expected/SadConsole.ColoredGlyph.md");
    
    Assert.AreEqual(NormalizeLineEndings(expected), NormalizeLineEndings(output));
}
```

### Property-Based Testing
Generate random test data to validate invariants:
```csharp
[TestMethod]
public void UidResolution_RandomPaths_AlwaysGeneratesValidRelativePaths()
{
    var generator = new RandomPathGenerator();
    
    for (int i = 0; i < 100; i++)
    {
        var (fromPath, toPath) = generator.GeneratePathPair();
        var relativePath = _linkResolver.ResolveRelativePath(fromPath, toPath);
        
        Assert.IsTrue(IsValidRelativePath(relativePath));
    }
}
```

### Mutation Testing
Validate test quality by introducing code mutations and ensuring tests fail.

## Continuous Integration

### Build Pipeline Tests
1. **Unit Tests**: Run on every commit
2. **Integration Tests**: Run on pull requests
3. **Performance Tests**: Run on release branches
4. **End-to-End Tests**: Run on release candidates

### Test Reporting
- **Coverage Reports**: Aim for >90% code coverage
- **Performance Metrics**: Track generation speed and memory usage
- **Test Results**: Detailed test failure analysis
- **Regression Detection**: Alert on performance or output changes

## Manual Testing Scenarios

### User Acceptance Testing
1. **CLI Usability**: Test command-line interface with real users
2. **Documentation Quality**: Review generated documentation readability
3. **Template Customization**: Validate template modification workflows
4. **Error Messages**: Ensure error messages are helpful and actionable

### Compatibility Testing
1. **Different Operating Systems**: Windows, macOS, Linux
2. **Different .NET Versions**: .NET 6, 8, 10
3. **Different DocFX Versions**: Validate against various DocFX outputs
4. **Large Documentation Sets**: Test with real-world large APIs

## Test Maintenance

### Test Data Management
- Keep test data in version control
- Update test data when DocFX format changes
- Maintain golden files with documentation generation
- Regular cleanup of obsolete test cases

### Test Performance
- Keep test execution time reasonable (<5 minutes for full suite)
- Parallelize independent tests
- Use test categories for selective execution
- Mock external dependencies appropriately