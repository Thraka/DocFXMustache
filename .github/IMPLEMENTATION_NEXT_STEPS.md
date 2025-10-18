# Implementation Next Steps - LLM Quick Start Guide

## Phase 3: Template Engine & Link Resolution

### Priority 1: XRef Processing Service
**File**: `src/Services/XrefProcessingService.cs`

**Purpose**: Process `<xref href="UID"/>` tags in content and resolve to relative paths

**Core Methods**:
```csharp
public class XrefProcessingService
{
    public string ProcessXrefTags(string content, UidMappings mappings, string currentFilePath);
    public string ResolveUidToRelativePath(string fromPath, string targetUid, UidMappings mappings);
    public XrefLinkData ExtractXrefData(string xrefTag);
}
```

**Implementation Steps**:
1. Parse `<xref href="UID" data-throw-if-not-resolved="false"/>` tags
2. Use existing UidMappings to resolve UIDs to file paths
3. Calculate relative paths from current file to target file
4. Replace xref tags with markdown links `[DisplayText](relativePath)`
5. Handle missing UIDs gracefully (external links or broken references)

### Priority 2: Template Processing Service
**File**: `src/Services/TemplateProcessingService.cs`

**Purpose**: Integrate Stubble.Core and render Mustache templates

**Core Methods**:
```csharp
public class TemplateProcessingService
{
    public async Task<string> RenderTemplateAsync(Item item, string templateContent, UidMappings mappings);
    public TemplateData CreateTemplateData(Item item, UidMappings mappings);
    public string ResolveTemplateByType(ItemType itemType);
}
```

**Implementation Steps**:
1. Install Stubble.Core NuGet package
2. Create template data models from Item objects
3. Add template resolution logic (class.mustache, interface.mustache, etc.)
4. Implement data transformation helpers
5. Handle template compilation and caching

### Priority 3: Documentation Generator
**File**: `src/Services/DocumentationGenerator.cs`

**Purpose**: Orchestrate complete two-pass workflow

**Core Methods**:
```csharp
public class DocumentationGenerator
{
    public async Task GenerateAsync(string inputDir, string outputDir, string templateDir, 
                                   string format, string grouping, bool dryRun, bool verbose);
    private async Task<UidMappings> ExecutePass1(string inputDir, string grouping);
    private async Task ExecutePass2(UidMappings mappings, string outputDir, string templateDir, string format);
}
```

**Implementation Steps**:
1. Pass 1: Use existing DiscoveryService to build UID mappings
2. Pass 2: For each item, process XRefs and render templates
3. Generate output files with proper directory structure
4. Handle both MD and MDX output formats
5. Implement error recovery and progress reporting

### Priority 4: File Generation Service
**File**: `src/Services/FileGenerationService.cs`

**Purpose**: Handle file writing and directory creation

**Core Methods**:
```csharp
public class FileGenerationService
{
    public async Task WriteFileAsync(string filePath, string content, bool overwrite);
    public void CreateDirectoryStructure(string outputPath, string groupingStrategy);
    public bool ValidateOutputPath(string path);
}
```

**Implementation Steps**:
1. Create safe file writing with overwrite protection
2. Generate directory structure based on grouping strategy
3. Handle MD vs MDX format differences
4. Implement atomic file operations
5. Add progress reporting for large file sets

## Template System Enhancement

### Create Default Templates
**Location**: `templates/`

**Required Templates**:
- `class.mustache` - Class documentation
- `interface.mustache` - Interface documentation  
- `enum.mustache` - Enum documentation
- `namespace.mustache` - Namespace overview
- `method.mustache` - Method details (if generating separately)

**Template Data Structure**:
```json
{
  "uid": "SadConsole.ColoredGlyph",
  "name": "ColoredGlyph",
  "type": "Class",
  "namespace": "SadConsole",
  "summary": "Represents a glyph with foreground/background colors",
  "syntax": "public class ColoredGlyph",
  "inheritance": [{"name": "Object", "link": "#"}],
  "constructors": [...],
  "properties": [...],
  "methods": [...]
}
```

## Integration with Program.cs

**Update ProcessCommand method**:
```csharp
private static async Task ProcessCommand(...)
{
    // Initialize services
    var parsingService = new MetadataParsingService();
    var discoveryService = new DiscoveryService(parsingService);
    var xrefService = new XrefProcessingService();
    var templateService = new TemplateProcessingService();
    var generationService = new FileGenerationService();
    var documentationGenerator = new DocumentationGenerator(
        discoveryService, xrefService, templateService, generationService);

    // Execute two-pass workflow
    await documentationGenerator.GenerateAsync(
        input.FullName, output.FullName, template.FullName, 
        format, grouping, dryRun, verbose);
}
```

## Quick Implementation Strategy

### Week 1: Basic Template Rendering
1. Create XrefProcessingService with basic UID resolution
2. Implement TemplateProcessingService with Stubble.Core
3. Test with single template and simple content
4. **Success Criteria**: Generate first real documentation file

### Week 2: Complete Pipeline
1. Build DocumentationGenerator orchestrating workflow
2. Add FileGenerationService for robust file operations
3. Update Program.cs integration
4. **Success Criteria**: End-to-end generation working for all grouping strategies

### Week 3: Polish & Features
1. Create all default templates (class, interface, enum, etc.)
2. Add advanced template data helpers
3. Implement link validation and error reporting
4. **Success Criteria**: Production-ready tool generating comprehensive docs

## Test Commands

```bash
# Test basic generation (once implemented)
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --verbose

# Test all grouping strategies
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping namespace
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping assembly-namespace

# Full production test
DocFXMustache -i ".github/reference-files/api" -o "./docs" -t "./templates" -f mdx --grouping assembly-namespace --force
```

## Success Criteria

- [ ] XRef tags resolve to correct relative links
- [ ] Mustache templates render with real metadata
- [ ] Two-pass workflow generates organized documentation
- [ ] All 4 grouping strategies produce correct file structure
- [ ] Generated Markdown/MDX is valid and readable
- [ ] Process 4000+ UIDs efficiently (under 30 seconds)
- [ ] Handle errors gracefully with helpful messages

**Ready to implement Phase 3!** Start with XrefProcessingService using the existing UidMappings from Phase 2. 🚀