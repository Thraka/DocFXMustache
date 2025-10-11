# DocFX Mustache - Project Plan

## Project Overview

**DocFX Mustache** is a command-line tool designed to transform DocFX-generated .NET API metadata files into Markdown (.md) or MDX (.mdx) files using Mustache templating.

## Goal

Transform DocFX generated .NET API metadata files (YAML format) into customizable Markdown or MDX files suitable for static site generators, documentation platforms, or other publishing systems.

## File Grouping Strategies

A key consideration for large API documentation sets is how to organize the output files. The tool will support multiple file grouping strategies:

### 1. Flat Structure (`flat`)
**Default behavior** - All files in a single output directory using fully qualified names:
```
output/
├── SadConsole.ColoredGlyph.md
├── SadConsole.Components.Cursor.md
├── SadConsole.UI.Controls.Button.md
├── Microsoft.Xna.Framework.Graphics.yml.md
└── Hexa.NET.ImGui.SC.yml.md
```

### 2. Namespace Hierarchy (`namespace`)
Organize files by namespace structure:
```
output/
├── SadConsole/
│   ├── ColoredGlyph.md
│   ├── Components/
│   │   └── Cursor.md
│   └── UI/
│       └── Controls/
│           └── Button.md
├── Microsoft/
│   └── Xna/
│       └── Framework/
│           └── Graphics.md
└── Hexa/
    └── NET/
        └── ImGui/
            └── SC.md
```

### 3. Assembly + Namespace (`assembly-namespace`)
Group by assembly first, then namespace:
```
output/
├── SadConsole/                    # Assembly name
│   ├── SadConsole/               # Namespace
│   │   ├── ColoredGlyph.md
│   │   ├── Components/
│   │   │   └── Cursor.md
│   │   └── UI/
│   │       └── Controls/
│   │           └── Button.md
├── Microsoft.Xna.Framework/       # Assembly name
│   └── Microsoft/
│       └── Xna/
│           └── Framework/
│               └── Graphics.md
└── Hexa.NET.ImGui/               # Assembly name
    └── Hexa/
        └── NET/
            └── ImGui/
                └── SC.md
```

### 4. Assembly + Flat (`assembly-flat`)
Group by assembly, but keep types flat within each assembly:
```
output/
├── SadConsole/
│   ├── SadConsole.ColoredGlyph.md
│   ├── SadConsole.Components.Cursor.md
│   └── SadConsole.UI.Controls.Button.md
├── Microsoft.Xna.Framework/
│   └── Microsoft.Xna.Framework.Graphics.md
└── Hexa.NET.ImGui/
    └── Hexa.NET.ImGui.SC.md
```

### Configuration Options

```bash
# Use namespace hierarchy
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping namespace

# Use assembly + namespace structure  
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping assembly-namespace

# Default flat structure
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping flat
```

### Benefits by Strategy

| Strategy | Pros | Cons | Best For |
|----------|------|------|----------|
| **Flat** | Simple, searchable, no deep paths | Can become cluttered | Small APIs, search-driven sites |
| **Namespace** | Logical grouping, mirrors code | Can create deep paths | Large APIs, browsable documentation |
| **Assembly-Namespace** | Clear assembly boundaries | Most complex structure | Multi-assembly projects |
| **Assembly-Flat** | Assembly separation, simple paths | Less logical grouping | Multi-assembly, moderate size |

## Link Processing System

A critical aspect of generating documentation from DocFX metadata is handling links between types and members. The system must resolve UIDs (Unique Identifiers) from metadata files to relative paths in the generated output, accounting for different file organization strategies.

### UID-Based Link Resolution

DocFX metadata files contain two types of links:
1. **Internal Links**: References to types/members within the same documentation set (identified by UID)
2. **External Links**: References to external documentation (e.g., Microsoft docs, NuGet packages)

### Link Resolution Process

#### 1. UID Mapping
During the metadata parsing phase, the system will build a comprehensive UID-to-file mapping:

```csharp
public class UidResolver
{
    private readonly Dictionary<string, OutputFileInfo> _uidToFileMap;
    private readonly IFileOrganizationStrategy _organizationStrategy;
    
    public string ResolveUidToRelativePath(string fromFilePath, string targetUid)
    {
        // Resolve target UID to output file path
        // Calculate relative path from current context
        // Return proper relative path or external URL
    }
}
```

#### 2. Context-Aware Path Resolution
The link resolver must consider the context (current file location) when generating relative paths:

**Example Scenario:**
- Current file: `output/SadConsole/UI/Controls/Button.md`
- Target UID: `SadConsole.ColoredGlyph`
- Target file: `output/SadConsole/ColoredGlyph.md`
- Required relative path: `../../ColoredGlyph.md`

### Link Types and Processing

#### Internal Type References via XRef Tags
In DocFX metadata YAML files, links to other types are embedded as `<xref>` tags within documentation text:

```yaml
# In metadata YAML - example from summary/description text
summary: Forces the Background of the print appearance to be the darkened color and the foreground to be bright or not based on the <xref href="SadConsole.Ansi.State.Bold" data-throw-if-not-resolved="false"></xref> property.
```

**XRef Tag Processing - Data Transformation Only:**
The system parses xref tags and transforms them into structured data with resolved paths, but does NOT render the final output format:

1. Extract the `href` attribute (contains the target UID)
2. Resolve the UID to the appropriate output file path relative to current context
3. Transform the content into structured data that templates can use

**Example Transformation:**
Raw YAML content:
```yaml
summary: "Based on the <xref href=\"SadConsole.Ansi.State.Bold\" data-throw-if-not-resolved=\"false\"></xref> property."
```

Gets transformed into template data:
```json
{
  "summary": "Based on the {0} property.",
  "summaryLinks": [
    {
      "uid": "SadConsole.Ansi.State.Bold",
      "displayName": "Bold",
      "relativePath": "../../Bold.md",
      "isExternal": false
    }
  ]
}
```

**Template Decides Rendering:**
Templates then choose how to render these links:

```mustache
<!-- For Markdown output -->
{{summary}} with links: {{#summaryLinks}}[{{displayName}}]({{relativePath}}){{/summaryLinks}}

<!-- For MDX with custom components -->
{{summary}} with links: {{#summaryLinks}}<ApiLink href="{{relativePath}}">{{displayName}}</ApiLink>{{/summaryLinks}}

<!-- For HTML output -->
{{summary}} with links: {{#summaryLinks}}<a href="{{relativePath}}">{{displayName}}</a>{{/summaryLinks}}
```

#### External References
```yaml
# In metadata YAML  
references:
- uid: System.String
  name: String
  fullName: System.String
  isExternal: true
  href: https://docs.microsoft.com/dotnet/api/system.string
```

### File Grouping Impact on Links

Different file grouping strategies require different link resolution logic:

#### Flat Structure Links
```
Button.md → ColoredGlyph.md
Relative path: "./ColoredGlyph.md"
```

#### Namespace Hierarchy Links
```
SadConsole/UI/Controls/Button.md → SadConsole/ColoredGlyph.md  
Relative path: "../../ColoredGlyph.md"
```

#### Assembly-Namespace Links
```
SadConsole/SadConsole/UI/Controls/Button.md → SadConsole/SadConsole/ColoredGlyph.md
Relative path: "../../ColoredGlyph.md"
```

### Implementation Components

#### 1. Link Resolution Service
```csharp
public class LinkResolutionService
{
    public void BuildUidMappings(IEnumerable<MetadataFile> metadataFiles);
    public string ResolveInternalLink(string fromPath, string targetUid);
    public string ResolveExternalLink(string uid, string fallbackUrl);
    public bool IsExternalReference(string uid);
    public string ProcessXrefTags(string content, string currentFilePath);
}
```

#### 2. XRef Processing Service
```csharp
public class XrefProcessor
{
    private static readonly Regex XrefPattern = new Regex(
        @"<xref\s+href=""([^""]+)""[^>]*></xref>", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    public ProcessedContent ProcessXrefs(string content, string currentFilePath)
    {
        var links = new List<LinkInfo>();
        var processedText = content;
        int linkIndex = 0;
        
        processedText = XrefPattern.Replace(content, match => 
        {
            var uid = match.Groups[1].Value;
            var resolvedPath = _linkResolver.ResolveInternalLink(currentFilePath, uid);
            var displayName = ExtractDisplayName(uid);
            var isExternal = _linkResolver.IsExternalReference(uid);
            
            links.Add(new LinkInfo 
            { 
                Uid = uid, 
                DisplayName = displayName, 
                RelativePath = resolvedPath,
                IsExternal = isExternal
            });
            
            return $"{{{linkIndex++}}}"; // Placeholder for template processing
        });
        
        return new ProcessedContent 
        { 
            Text = processedText, 
            Links = links 
        };
    }
}

public class ProcessedContent
{
    public string Text { get; set; }
    public List<LinkInfo> Links { get; set; }
}

public class LinkInfo
{
    public string Uid { get; set; }
    public string DisplayName { get; set; }
    public string RelativePath { get; set; }
    public bool IsExternal { get; set; }
    public string ExternalUrl { get; set; }
}
```

#### 2. Template Link Helpers
Templates receive structured data with resolved paths and choose how to render:

```csharp
public class LinkHelpers
{
    [MustacheHelper("processContent")]
    public ProcessedContent ProcessContent(string content, TemplateContext context)
    {
        var currentFilePath = context.GetValue("currentFilePath");
        return _xrefProcessor.ProcessXrefs(content, currentFilePath);
    }
    
    [MustacheHelper("resolveUid")]
    public string ResolveUid(string uid, TemplateContext context)
    {
        var currentFilePath = context.GetValue("currentFilePath");
        return _linkResolver.ResolveInternalLink(currentFilePath, uid);
    }
}
```

#### 3. Template-Controlled Link Rendering
Templates receive structured data and decide how to render links:

```mustache
{{! Process content to get text and link data }}
{{#processContent summary}}
  
{{! Render the text with different link styles based on template choice }}

<!-- Markdown Links -->
{{text}} 
{{#links}}[{{displayName}}]({{relativePath}}){{/links}}

<!-- Or MDX Components -->
{{text}}
{{#links}}
  {{#isExternal}}
    <ExternalLink href="{{externalUrl}}">{{displayName}}</ExternalLink>
  {{/isExternal}}
  {{^isExternal}}
    <ApiLink href="{{relativePath}}">{{displayName}}</ApiLink>
  {{/isExternal}}
{{/links}}

<!-- Or HTML Links -->
{{text}}
{{#links}}
  {{#isExternal}}
    <a href="{{externalUrl}}" target="_blank">{{displayName}}</a>
  {{/isExternal}}
  {{^isExternal}}
    <a href="{{relativePath}}">{{displayName}}</a>
  {{/isExternal}}
{{/links}}

{{/processContent}}
```

**Complete Example - Template Data:**
```json
{
  "summary": {
    "text": "Forces the Background based on the {0} property.",
    "links": [
      {
        "uid": "SadConsole.Ansi.State.Bold",
        "displayName": "Bold", 
        "relativePath": "./Bold.md",
        "isExternal": false
      }
    ]
  }
}
```

**Template Rendering Examples:**

For Markdown output template:
```mustache
## Summary
{{summary.text}}{{#summary.links}} [{{displayName}}]({{relativePath}}){{/summary.links}}
```

For MDX output template:
```mustache
## Summary  
{{summary.text}}{{#summary.links}} <ApiRef uid="{{uid}}" href="{{relativePath}}">{{displayName}}</ApiRef>{{/summary.links}}
```

**Generated Output:**
Markdown: `Forces the Background based on the [Bold](./Bold.md) property.`
MDX: `Forces the Background based on the <ApiRef uid="SadConsole.Ansi.State.Bold" href="./Bold.md">Bold</ApiRef> property.`

### Cross-Reference Index Generation

The system will generate index files with proper cross-references:

#### Assembly Index
```markdown
# SadConsole Assembly

## Types
- [ColoredGlyph](./SadConsole/ColoredGlyph.md)
- [UI.Controls.Button](./SadConsole/UI/Controls/Button.md)

## Namespaces  
- [SadConsole](./SadConsole/README.md)
- [SadConsole.UI](./SadConsole/UI/README.md)
```

#### Namespace Index
```markdown
# SadConsole.UI.Controls Namespace

## Classes
- [Button](./Button.md) - A clickable UI control
- [TextBox](./TextBox.md) - Text input control

## Related Namespaces
- [SadConsole.UI](../README.md) - Parent namespace
```

### Link Validation

The system will validate links during generation:

```csharp
public class LinkValidator
{
    public ValidationResult ValidateLinks(IEnumerable<GeneratedFile> files);
    public IEnumerable<BrokenLink> FindBrokenInternalLinks();
    public IEnumerable<UnresolvedUid> FindUnresolvedUids();
}
```

### Configuration Options

```json
{
  "LinkProcessing": {
    "ValidateLinks": true,
    "FailOnBrokenLinks": false,
    "ExternalLinkHandling": "preserve", // preserve, remove, validate
    "GenerateBacklinks": true,
    "IncludeCrossAssemblyLinks": true,
    "ExternalReferenceUrls": {
      "System.*": "https://docs.microsoft.com/dotnet/api/{uid}",
      "Microsoft.*": "https://docs.microsoft.com/dotnet/api/{uid}"
    }
  }
}
```

### Link Processing Workflow

The link processing system requires a **two-pass approach** to properly resolve UIDs to file paths:

#### Pass 1: Discovery and Mapping
1. **Parse all metadata files** to extract UIDs and determine output file structure
2. **Apply file organization strategy** to determine where each UID's file will be generated
3. **Build comprehensive UID-to-path mapping** for the entire documentation set
4. **Validate UID references** and identify external vs internal links

#### Pass 2: Template Processing and Generation
1. **Process templates** using the pre-built UID mapping
2. **Resolve xref tags** to relative paths using the mapping
3. **Generate final output files** with properly resolved links

```csharp
public class DocumentationGenerator
{
    public async Task GenerateAsync(string inputPath, string outputPath, string templatesPath)
    {
        // PASS 1: Discovery and Mapping
        var metadataFiles = await _metadataParser.ParseAllFilesAsync(inputPath);
        var uidMappings = await _discoveryService.BuildUidMappingsAsync(metadataFiles, outputPath);
        
        // PASS 2: Template Processing  
        foreach (var metadataFile in metadataFiles)
        {
            var processedContent = _xrefProcessor.ProcessContent(metadataFile, uidMappings);
            var outputContent = _templateEngine.Process(processedContent);
            await _fileWriter.WriteAsync(outputContent);
        }
    }
}
```

#### Pass 1: Discovery Service
```csharp
public class DiscoveryService
{
    public async Task<UidMappings> BuildUidMappingsAsync(
        IEnumerable<MetadataFile> metadataFiles, 
        string outputBasePath)
    {
        var mappings = new UidMappings();
        
        foreach (var file in metadataFiles)
        {
            foreach (var item in file.Items)
            {
                // Determine output file path based on organization strategy
                var outputPath = _organizationStrategy.GetOutputPath(item, outputBasePath);
                
                // Map UID to output path
                mappings.AddMapping(item.Uid, outputPath);
            }
        }
        
        return mappings;
    }
}

public class UidMappings
{
    private readonly Dictionary<string, string> _uidToFilePath = new();
    
    public void AddMapping(string uid, string filePath) => _uidToFilePath[uid] = filePath;
    
    public string ResolveRelativePath(string fromFilePath, string targetUid)
    {
        if (!_uidToFilePath.TryGetValue(targetUid, out var targetPath))
            return null; // External or missing reference
            
        return Path.GetRelativePath(
            Path.GetDirectoryName(fromFilePath), 
            targetPath);
    }
    
    public bool IsInternalReference(string uid) => _uidToFilePath.ContainsKey(uid);
}
```

This comprehensive link processing system ensures that regardless of the file organization strategy chosen, all internal references will be properly resolved to correct relative paths, while external references are preserved or enhanced with proper URLs.

### Two-Pass Architecture Benefits

#### Advantages:
- **Complete UID Resolution**: All internal UIDs are known before template processing begins
- **Accurate Link Validation**: Can detect broken references before generating any files
- **Consistent Path Resolution**: All files use the same UID mapping, ensuring consistency
- **Efficient Processing**: No need to re-scan files during template processing
- **Cross-Assembly Linking**: Can resolve links between different assemblies in the same documentation set

#### Performance Considerations:
- **Memory Usage**: Must hold UID mappings for entire documentation set in memory
- **Initial Discovery Cost**: Pass 1 requires parsing all metadata files before any output generation
- **Large Documentation Sets**: May need optimization for very large API documentation projects

#### Error Handling:
- **Early Detection**: Broken internal references detected in Pass 1 before template processing
- **Graceful Degradation**: Can continue processing with warnings for missing UIDs
- **External Reference Handling**: Clear separation between internal and external link processing

## Core Features

### Command Line Interface

The application will use **System.CommandLine** (Microsoft's modern CLI library) with the following structure:

```bash
DocFXMustache --input <metadata-folder> --output <output-folder> --templates <template-folder> --format <md|mdx> [options]
```

**Benefits of System.CommandLine:**
- Built-in tab completion
- Automatic help generation and validation
- POSIX and Windows command-line conventions
- Response file support
- Trim-friendly for AOT compilation

#### Required Parameters
- `--input` or `-i`: Path to the folder containing DocFX metadata files (YAML files)
- `--output` or `-o`: Path to the folder where output files should be generated
- `--templates` or `-t`: Path to the folder containing Mustache template files
- `--format` or `-f`: Output file format (`md` or `mdx`)

#### Optional Parameters
- `--help` or `-h`: Display help information
- `--verbose` or `-v`: Enable verbose logging
- `--dry-run`: Preview operations without writing files
- `--overwrite`: Overwrite existing output files
- `--filter <pattern>`: Process only files matching the specified pattern
- `--grouping <strategy>`: File grouping strategy (see File Grouping section)

### Technology Stack

#### Core Dependencies (NuGet Packages)
1. **YAML Parser**: `VYaml` - For parsing DocFX metadata YAML files (matches reference models)
2. **String Operations**: `ZString` (Cysharp.Text) - For efficient string concatenation (used in reference models)
3. **Mustache Engine**: `Stubble.Core` - For processing template files and generating output
4. **Command Line Parsing**: `System.CommandLine` - Microsoft's modern CLI library with tab completion and validation
5. **File System**: Built-in .NET I/O classes

#### Additional Considerations
- **Logging**: `Microsoft.Extensions.Logging` for structured logging
- **Configuration**: `Microsoft.Extensions.Configuration` for settings management
- **Testing**: `xUnit` for unit testing

## Architecture

### Project Structure
```
DocFXMustache/
├── src/
│   ├── Models/           # Data models for DocFX metadata
│   ├── Services/         # Core business logic
│   ├── Templates/        # Template processing
│   ├── CLI/             # Command line interface
│   └── Utils/           # Utility classes
├── templates/           # Default Mustache templates
│   ├── class.mustache
│   ├── interface.mustache
│   ├── enum.mustache
│   └── namespace.mustache
├── tests/              # Unit tests
└── docs/               # Documentation
```

### Core Components

#### 1. Data Models (`Models/`)
Based on DocFX metadata structure and reference models in `.github\reference-files\Models\`:
- `Root` - Root YAML document structure (contains Items and References)
- `Item` - Base class for all API items from YAML
- `TypeDocumentation` - Main documentation model for types
- `ParameterDocumentation` - Method/constructor parameters
- `ReturnDocumentation` - Return value documentation
- `ExceptionDocumentation` - Exception documentation
- `TypeReferenceDocumentation` - Type references and links
- `Link` - URL and display text for links
- `ItemType` enum - API item types (Class, Interface, Method, etc.)

**YAML Structure Models:**
- `Parameter` - YAML parameter structure
- `Reference` - YAML reference structure  
- `AttributeDoc` - Attribute documentation
- `SyntaxContent` - Syntax highlighting content

#### 2. Services (`Services/`)
- `MetadataParsingService` - Parse YAML metadata files (Pass 1)
- `DiscoveryService` - Build UID mappings and file organization (Pass 1)
- `XrefProcessingService` - Process xref tags with UID mappings (Pass 2)
- `TemplateProcessingService` - Process Mustache templates (Pass 2)
- `FileGenerationService` - Generate output files (Pass 2)
- `DocumentationGenerator` - Orchestrate two-pass workflow
- `FileGroupingService` - Handle different grouping strategies
- `AssemblyDetectionService` - Extract assembly information from metadata
- `LinkValidationService` - Validate links after Pass 1 discovery
- `ConfigurationService` - Handle application configuration

#### 3. Template Engine (`Templates/`)
- `TemplateEngine` - Wrapper around Stubble.Core
- `TemplateResolver` - Resolve template files based on item type
- `TemplateHelpers` - Custom Mustache helpers for documentation

#### 4. CLI (`CLI/`)
- `Program` - Application entry point using System.CommandLine
- `CliConfiguration` - Configure root command, options, and arguments
- `CommandHandlers` - Action handlers for parsed commands

## Implementation Phases

### Phase 1: Foundation (Week 1)
- [ ] Set up project structure
- [ ] Configure NuGet packages (`VYaml`, `ZString`, `Stubble.Core`, `System.CommandLine`)
- [ ] Implement modern CLI using System.CommandLine with tab completion
- [ ] Adapt core data models from `.github\reference-files\Models\`
- [ ] Set up logging infrastructure
- [ ] Create initial project structure based on reference implementation

### Phase 2: Metadata Processing & Discovery (Week 2)
- [ ] Implement YAML parsing for DocFX metadata using reference models
- [ ] Adapt existing model structure from `.github\reference-files\Models\`
- [ ] Create metadata-to-model mapping for YAML structure
- [ ] **Implement Discovery Service for Pass 1 processing**
- [ ] **Build UID extraction and mapping functionality**
- [ ] **Integrate file grouping strategies with UID mapping**
- [ ] Handle different API item types (classes, interfaces, enums, methods, properties, etc.)
- [ ] Implement metadata validation and error handling
- [ ] Test with sample files from `.github\reference-files\api\`

### Phase 3: Link Resolution & Template Engine (Week 3)
- [ ] **Implement UidMappings class for Pass 1 results**
- [ ] **Create XRef processing with pre-built mappings for Pass 2**
- [ ] Integrate Stubble.Core Mustache engine
- [ ] Create default templates for each API item type
- [ ] Implement template resolution logic
- [ ] **Add structured link data helpers for templates**
- [ ] **Implement two-pass DocumentationGenerator workflow**

### Phase 4: File Generation & Link Validation (Week 4)
- [ ] **Implement two-pass generation process**
- [ ] Handle .md and .mdx output formats with proper link rendering
- [ ] Implement all file grouping strategies (flat, namespace, assembly-namespace, assembly-flat)
- [ ] Create assembly detection logic from metadata
- [ ] Implement file naming conventions and path generation
- [ ] Add overwrite protection and dry-run mode
- [ ] **Implement link validation after Pass 1 discovery**
- [ ] **Generate cross-reference reports and broken link detection**
- [ ] Generate index files for assemblies and namespaces

### Phase 5: Testing & Polish (Week 5)
- [ ] Comprehensive unit tests
- [ ] Integration tests with sample DocFX metadata
- [ ] Performance optimization
- [ ] Documentation and usage examples

## Default Templates

### Template Types
1. **Class Template** (`class.mustache`)
   - Class overview, inheritance hierarchy
   - Constructor documentation
   - Property and method listings
   - Example usage

2. **Interface Template** (`interface.mustache`)
   - Interface description
   - Method and property signatures
   - Implementation notes

3. **Enum Template** (`enum.mustache`)
   - Enum values and descriptions
   - Usage examples

4. **Namespace Template** (`namespace.mustache`)
   - Namespace overview
   - Type listings

### Template Variables
Common Mustache variables across templates:
- `{{name}}` - Item name
- `{{fullName}}` - Fully qualified name
- `{{summary}}` - Summary documentation
- `{{remarks}}` - Detailed remarks
- `{{examples}}` - Code examples
- `{{seeAlso}}` - See also references
- `{{inheritance}}` - Inheritance chain
- `{{namespace}}` - Containing namespace

## Configuration

### Default Configuration File (`appsettings.json`)
```json
{
  "Output": {
    "DefaultFormat": "md",
    "DefaultGrouping": "flat",
    "FileNamingConvention": "lowercase-hyphen",
    "OverwriteExisting": false
  },
  "Templates": {
    "DefaultTemplateSet": "standard",
    "CustomHelpers": []
  },
  "Processing": {
    "IncludePrivateMembers": false,
    "IncludeInternalMembers": false,
    "GenerateIndexFiles": true,
    "CreateAssemblyIndexes": true
  },
  "FileGrouping": {
    "MaxDirectoryDepth": 10,
    "GroupingStrategies": ["flat", "namespace", "assembly-namespace", "assembly-flat"],
    "AssemblyDetection": {
      "UseMetadataAssemblyInfo": true,
      "FallbackToFilename": true
    }
  }
}
```

## Usage Examples

### Basic Usage
```bash
# Convert DocFX metadata to Markdown
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md

# Convert to MDX with verbose output
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f mdx --verbose

# Dry run to preview changes
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --dry-run
```

### Advanced Usage
```bash
# Process only class files with namespace organization
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --filter "*.yml" --grouping namespace

# Assembly-based organization with MDX output
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f mdx --grouping assembly-namespace

# Overwrite existing files with verbose logging
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f mdx --overwrite --verbose

# Dry run with custom grouping to preview structure
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping assembly-flat --dry-run
```

## Testing Strategy

### Unit Tests
- Model serialization/deserialization
- Template processing with sample data
- File generation logic
- CLI parameter parsing

### Integration Tests
- End-to-end processing with sample DocFX metadata
- Template customization scenarios
- Error handling and validation

### Test Data
- Sample DocFX metadata files from `.github\reference-files\api\`
  - Class examples: `SadConsole.ColoredGlyph.yml`
  - Namespace examples: `SadConsole.yml`, `Microsoft.Xna.Framework.yml` 
  - Extension methods: `Microsoft.Xna.Framework.Graphics.MonoGame_TextureExtensions.yml`
- Expected output files for comparison testing
- Custom template examples for different scenarios
- Reference model implementations from `.github\reference-files\Models\`

## Success Criteria

1. **Functionality**: Successfully transform DocFX metadata into readable Markdown/MDX
2. **Flexibility**: Support custom templates and multiple output formats
3. **Usability**: Intuitive command-line interface with helpful error messages
4. **Performance**: Process large API documentation sets efficiently
5. **Maintainability**: Clean, well-documented, testable code

## Future Enhancements

- Support for additional output formats (HTML, AsciiDoc)
- Integration with popular static site generators
- Template marketplace/sharing
- Watch mode for continuous processing
- Plugin system for custom transformations
- GUI interface for non-technical users

## Reference Resources

- **Existing Models**: `.github\reference-files\Models\` (Available in workspace)
- **Sample Metadata**: `.github\reference-files\api\` (Available in workspace)
- **DocFX Documentation**: https://dotnet.github.io/docfx/
- **Mustache Specification**: https://mustache.github.io/
- **Stubble.Core Documentation**: https://github.com/StubbleOrg/Stubble

---

*This plan serves as a roadmap for the DocFX Mustache project. It should be reviewed and updated as development progresses and requirements evolve.*