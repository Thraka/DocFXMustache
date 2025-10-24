# Implementation Next Steps - LLM Quick Start Guide

## Current Status (October 24, 2025)
- **Phase 1**: ✅ **100% complete** (CLI framework, YAML parsing, logging infrastructure)
- **Phase 2**: ✅ **100% complete** (UID discovery with 4,075+ UIDs, all grouping strategies validated, logging integrated)
- **Phase 3**: 0% complete (**READY TO START** - Link Resolution & Template Engine)
- **Phase 4**: 0% complete (File Generation & Output)
- **Phase 5**: Partial (59 unit tests passing)

---

## Phase 3: Template Engine & Link Resolution

### Priority 1: XRef Processing Service
**File**: `src/Services/XrefProcessingService.cs`

**Purpose**: Process `<xref href="UID"/>` tags in content and resolve to relative paths (including anchor links for members)

**Core Methods**:
```csharp
public class XrefProcessingService
{
    private readonly IStubbleRenderer _renderer;
    private readonly string _linkTemplate;
    
    // Main method - processes all xrefs in content using link template
    public string ProcessXrefTags(string content, UidMappings mappings, string currentFilePath);
    
    // Helper to resolve UID to relative path (with anchors for members)
    public string ResolveUidToRelativePath(string fromPath, string targetUid, UidMappings mappings);
    
    // Creates link data object for a single xref
    public LinkInfo CreateLinkInfo(string uid, string currentFilePath, UidMappings mappings);
    
    // Renders a single link using link.mustache template
    public string RenderLink(LinkInfo linkInfo);
}

public class LinkInfo
{
    public string Uid { get; set; }
    public string DisplayName { get; set; }
    public string RelativePath { get; set; }
    public bool IsExternal { get; set; }
    public string ExternalUrl { get; set; }
    public string Anchor { get; set; }        // NEW: For member links (e.g., "#foreground")
}
```

**Implementation Steps**:
1. Parse `<xref href="UID" data-throw-if-not-resolved="false"/>` tags from content
2. Use existing UidMappings to resolve UIDs to file paths
3. **Check if UID is a member on a parent type page** - add anchor if needed
4. Calculate relative paths from current file to target file
5. **Process links using link template**:
   - For each xref tag found:
     - Extract UID and resolve to relative path
     - Create `LinkInfo` data object (uid, displayName, relativePath, isExternal, anchor)
     - **Render using `link.mustache` template** to generate formatted link
     - Replace xref tag in content with rendered link
   - **Link template controls format** (basic markdown, MDX components, HTML, etc.)
   - This keeps XrefProcessingService platform-agnostic while giving templates full control
6. Handle missing UIDs gracefully (external links or broken references)

**Key Innovation**: Use a dedicated `link.mustache` template for rendering individual links, allowing different template sets to format links differently without changing code.

**Template Configuration**: The UID mapping system must know how templates organize content:
- If `CombineMembersWithType = true`: Members link to parent page with anchor
- If `CombineMembersWithType = false`: Members get their own pages

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
    private async Task<UidMappings> ExecutePass1(string inputDir, string grouping, TemplateConfiguration templateConfig);
    private async Task ExecutePass2(UidMappings mappings, string outputDir, string templateDir, string format);
    private TemplateConfiguration LoadTemplateConfiguration(string templateDir);
}
```

**Implementation Steps**:
1. **Load template configuration** from template directory (e.g., `template.json`)
   - Determines if members are combined with types or separate
   - Controls index file generation
   - Other template-specific settings
2. **Pass 1**: Use existing DiscoveryService to build UID mappings
   - **Pass template config to discovery** so it knows how to map UIDs
   - If `CombineMembersWithType = true`, map member UIDs to parent file + anchor
   - If `CombineMembersWithType = false`, map member UIDs to individual files
3. Pass 2: For each item, process XRefs and render templates
4. Generate output files with proper directory structure
5. Handle both MD and MDX output formats
6. Implement error recovery and progress reporting

**Template Configuration File** (`templates/basic/template.json`):
```json
{
  "combineMembers": true,
  "generateIndexFiles": true,
  "includeInheritedMembers": false
}
```

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
  "summary": "Represents a glyph with [foreground/background](./Color.md) colors",
  "syntax": "public class ColoredGlyph",
  "inheritance": [{"name": "Object", "link": "#"}],
  "constructors": [...],
  "properties": [...],
  "methods": [...]
}
```
*Note: `summary` contains pre-processed text with links already rendered using `link.mustache`*

**Link Template Examples** (`link.mustache` - Platform-Specific):
```mustache
<!-- templates/basic/link.mustache - Simple Markdown -->
[{{displayName}}]({{relativePath}})

<!-- templates/starlight/link.mustache - Astro/Starlight (no .md extension) -->
[{{displayName}}]({{relativePath}})

<!-- templates/mdx/link.mustache - MDX with custom component -->
<ApiLink href="{{relativePath}}">{{displayName}}</ApiLink>

<!-- templates/html/link.mustache - HTML output -->
<a href="{{relativePath}}" class="api-link">{{displayName}}</a>
```

**Main Template Usage** (`class.mustache`):
```mustache
# {{name}} Class

## Summary
{{{summary}}}
<!-- Note: triple braces {{{ }}} to render HTML/markdown unescaped -->
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

### Phase 3 - Week 1: Basic Template Rendering (8-10 hours) **← NEXT**
1. Create XrefProcessingService with basic UID resolution (~2 hours)
2. Implement TemplateProcessingService with Stubble.Core (~2-3 hours)
3. Create basic class.mustache template (~1 hour)
4. Test with single template and simple content (~1-2 hours)
5. **Success Criteria**: Generate first real documentation file

### Phase 3 - Week 2: Complete Pipeline (6-8 hours)
1. Build DocumentationGenerator orchestrating workflow (~2 hours)
2. Add FileGenerationService for robust file operations (~1.5 hours)
3. Update Program.cs integration (~1 hour)
4. Create remaining templates (interface, enum, method) (~1-2 hours)
5. **Success Criteria**: End-to-end generation working for all grouping strategies

### Phase 3 - Week 3: Polish & Features (4-6 hours)
1. Add advanced template data helpers (~1-2 hours)
2. Implement link validation and error reporting (~1-2 hours)
3. Comprehensive testing and optimization (~2 hours)
4. **Success Criteria**: Production-ready tool generating comprehensive docs

## Test Commands

```bash
# Verify Phase 2 completion (currently working)
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --dry-run --verbose

# Test UID discovery with all grouping strategies (working)
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping flat --dry-run
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping namespace --dry-run
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping assembly-namespace --dry-run
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping assembly-flat --dry-run

# Test basic generation (Phase 3 - not yet implemented)
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --verbose

# Test all grouping strategies with actual file generation (Phase 3 - not yet implemented)
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping namespace
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping assembly-namespace

# Full production test (Phase 3 - not yet implemented)
DocFXMustache -i ".github/reference-files/api" -o "./docs" -t "./templates" -f mdx --grouping assembly-namespace --force
```