# Core Architecture

## Project Structure
```
DocFXMustache/
├── src/
│   ├── Models/          # Data models for DocFX metadata
│   ├── Services/        # Core business logic
│   ├── Templating/      # Template processing
│   ├── CLI/             # Command line interface
│   ├── Utils/           # Utility classes
│   └── Program.cs
├── templates/           # Template packs
│   ├── basic/                   # Testing template
│   │   └── default.md
│   └── starlight/               # Starlight template
│       ├── class.mdx
│       ├── interface.mdx
│       ├── enum.mdx
│       └── namespace.mdx
├── tests/              # Unit tests
└── docs/               # Documentation
```

## Core Components

### 1. Data Models (`src/Models/`)
Based on DocFX metadata structure and reference models in `.github\reference-files\Models\`:

**Primary Models:**
- `Root` - Root YAML document structure (contains Items and References)
- `Item` - Base class for all API items from YAML
- `TypeDocumentation` - Main documentation model for types
- `ParameterDocumentation` - Method/constructor parameters
- `ReturnDocumentation` - Return value documentation
- `ExceptionDocumentation` - Exception documentation
- `TypeReferenceDocumentation` - Type references and links
- `Link` - URL and display text for links
- `LinkInfo` - Template rendering model for resolved links ✅ (Oct 24, 2025)
- `OutputFileInfo` - File path with optional anchor for members ✅ (Oct 24, 2025)
- `ItemType` enum - API item types (Class, Interface, Method, etc.)

**YAML Structure Models:**
- `Parameter` - YAML parameter structure
- `Reference` - YAML reference structure  
- `AttributeDoc` - Attribute documentation
- `SyntaxContent` - Syntax highlighting content

### 2. Services (`src/Services/`)

**Core Processing Services:**
- `MetadataParsingService` - Parse YAML metadata files ✅ (Implemented)
- `DiscoveryService` - Build UID mappings and file organization ✅ (Implemented)
- `LinkResolutionService` - Record UIDs (Pass 1) and resolve to paths (Pass 2) ✅ (Oct 24, 2025)
- `XrefProcessingService` - Process xref tags with UID mappings (Pass 2) ✅ (Oct 24, 2025)
- `TemplateProcessingService` - Process Mustache templates (Pass 1 & 2) ⏳ (Planned)
- `FileGenerationService` - Generate output files (Pass 2) ⏳ (Planned)
- `DocumentationGenerator` - Orchestrate two-pass workflow ⏳ (Planned)

**Supporting Services:**
- `FileGroupingService` - Handle different grouping strategies
- `AssemblyDetectionService` - Extract assembly information from metadata
- `LinkValidationService` - Validate links after Pass 1 discovery ⏳ (Planned)
- `ConfigurationService` - Handle application configuration
- `LoggerFactory` - Structured logging with console output ✅ (Implemented)

### 3. Template Engine (`src/Templating/`)
- `TemplateEngine` - Wrapper around Stubble.Core
- `TemplateResolver` - Resolve template files based on item type
- `TemplateHelpers` - Custom Mustache helpers for documentation

### 4. CLI (`src/Program.cs`)
- `Program` - Application entry point using System.CommandLine
- `CliConfiguration` - Configure root command, options, and arguments
- `CommandHandlers` - Action handlers for parsed commands

## Processing Pipeline

### Two-Pass Architecture

The system uses a two-pass approach to handle cross-references correctly:

#### Pass 1: Discovery and Mapping
1. **Parse all metadata files** - Load YAML into data models
2. **Build UID mappings** - Create comprehensive UID-to-file mapping
3. **Determine file organization** - Apply grouping strategy
4. **Validate link targets** - Ensure all referenced UIDs exist

#### Pass 2: Generation
1. **Process cross-references** - Resolve UIDs to relative paths
2. **Apply templates** - Generate content using Mustache templates
3. **Write output files** - Save to target directory structure

### Data Flow

```
YAML Files → Parse → UID Mapping → Template Processing → Output Files
     ↓           ↓         ↓              ↓               ↓
   Models    Discovery  XRef Links    Mustache       MD/MDX Files
```

## Key Design Patterns

### Strategy Pattern
File grouping strategies implement a common interface:
```csharp
public interface IFileOrganizationStrategy
{
    OutputFileInfo DetermineOutputPath(Item item, MetadataContext context);
    string ResolveRelativePath(string fromPath, string toPath);
}
```

### Template Method Pattern
Processing pipeline defines the algorithm structure:
```csharp
public abstract class DocumentationGenerator
{
    public async Task GenerateAsync()
    {
        await DiscoverMetadataAsync();     // Pass 1
        await BuildUidMappingsAsync();     // Pass 1
        await ProcessTemplatesAsync();     // Pass 2
        await WriteOutputFilesAsync();     // Pass 2
    }
}
```

### Dependency Injection
Services are registered and injected for testability:
```csharp
services.AddSingleton<IMetadataParsingService, MetadataParsingService>();
services.AddSingleton<ITemplateEngine, TemplateEngine>();
services.AddScoped<IFileOrganizationStrategy>(provider =>
    CreateGroupingStrategy(configuration.Grouping));
```

## Configuration Management

### Configuration Sources
1. **Command line arguments** - Highest priority
2. **Configuration file** (docfx-mustache.json)
3. **Environment variables**
4. **Default values** - Lowest priority

### Configuration Schema
```json
{
  "input": "./api",
  "output": "./docs",
  "templates": "./templates",
  "format": "md",
  "grouping": "flat",
  "caseHandling": "lowercase",
  "overwrite": false,
  "verbose": false,
  "dryRun": false,
  "templateSettings": {
    "customProperty": "value"
  }
}
```

### File Naming Conventions
- **Default Case Handling**: All file and directory names are converted to lowercase for consistency
- **Directory Names**: Namespace dots (`.`) are converted to hyphens (`-`) and made lowercase
- **File Names**: Type names are converted to lowercase with invalid characters replaced by hyphens (`-`)
- **Planned Enhancement**: CLI option to control case handling (lowercase, preserve original)

## Error Handling Strategy

### Error Categories
- **Validation Errors** - Invalid input parameters or missing files
- **Parsing Errors** - Malformed YAML or unsupported structure
- **Template Errors** - Template compilation or rendering failures
- **I/O Errors** - File system access issues

### Error Recovery
- **Graceful degradation** - Continue processing when possible
- **Detailed logging** - Comprehensive error reporting
- **Dry run mode** - Validate without writing files
- **Partial success** - Report what was completed successfully

## Performance Considerations

### Memory Management
- **Streaming YAML parsing** - Avoid loading all files into memory
- **Lazy evaluation** - Load templates and metadata on demand
- **Resource disposal** - Proper cleanup of file handles and resources

### Parallel Processing
- **Concurrent metadata parsing** - Process multiple files simultaneously
- **Template compilation caching** - Reuse compiled templates
- **Batch file operations** - Minimize file system calls

## Extensibility Points

### Custom Templates
- Template discovery and registration
- Custom Mustache helpers
- Template inheritance and partials

### Custom Grouping Strategies
- Plugin architecture for new grouping strategies
- Strategy registration and configuration

### Custom Output Formats
- Format-specific template sets
- Custom file extensions and processing rules