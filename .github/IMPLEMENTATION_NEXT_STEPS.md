# Implementation Next Steps - LLM Quick Start Guide

## Current Status (October 24, 2025)
- **Phase 1**: 66% complete (missing logging infrastructure)
- **Phase 2**: ✅ **95% complete** (case control ✅, reference file testing ✅, missing: logging infrastructure, formal model validation)
- **Phase 3**: 0% complete (**READY TO START** - discovery validated with 4,075 UIDs)

---

## Phase 2 Completion (Quick Wins - Do These Next!)

### ✅ Test with Reference Files - COMPLETED
**Status**: ✅ **COMPLETED** (October 24, 2025)

**Results**:
- Successfully tested with `.github/reference-files/api/` (431 YAML files)
- Discovered and mapped 4,075 UIDs across 4 assemblies and 36 namespaces
- All 4 grouping strategies validated (flat, namespace, assembly-namespace, assembly-flat)
- All case control options working (uppercase, lowercase, mixed)
- Error handling verified
- Dry-run and verbose modes functioning correctly

**See**: [PHASE2_REFERENCE_TEST_RESULTS.md](.github/PHASE2_REFERENCE_TEST_RESULTS.md) for detailed test report

**Time**: Completed

---

### Add Logging Infrastructure
**File**: `src/Services/LoggingService.cs` (new)

**What**: Structured logging that respects `--verbose` flag

**Implementation**:
1. Create simple logging service
2. Integrate with existing `--verbose` option
3. Add logging calls to MetadataParsingService, DiscoveryService

**Time**: ~1-1.5 hours

---

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

### Phase 2 Completion (2-3 hours total)
1. Add `--case` CLI option with case transformation logic (~30 min)
2. Test discovery with reference files (~1 hour)
3. Add logging infrastructure (~1-1.5 hours)
4. **Success Criteria**: All Phase 2 features working with real metadata

### Phase 3 - Week 1: Basic Template Rendering (8-10 hours)
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
# Test basic generation (once implemented)
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --verbose

# Test all grouping strategies
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping namespace
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping assembly-namespace

# Full production test
DocFXMustache -i ".github/reference-files/api" -o "./docs" -t "./templates" -f mdx --grouping assembly-namespace --force
```

## Success Criteria

### Phase 2 Completion
- [x] `--case` CLI option controls filename casing ✅ (Oct 24, 2025)
- [ ] All reference files in `.github/reference-files/api/` parse successfully
- [ ] Logging infrastructure respects `--verbose` flag
- [ ] All grouping strategies work with reference data

### Phase 3 Implementation
- [ ] XRef tags resolve to correct relative links
- [ ] Mustache templates render with real metadata
- [ ] Two-pass workflow generates organized documentation
- [ ] All 4 grouping strategies produce correct file structure
- [ ] Generated Markdown/MDX is valid and readable
- [ ] Process 4000+ UIDs efficiently (under 30 seconds)
- [ ] Handle errors gracefully with helpful messages

**Ready to complete Phase 2, then implement Phase 3!** Start with the quick wins in Phase 2. 🚀