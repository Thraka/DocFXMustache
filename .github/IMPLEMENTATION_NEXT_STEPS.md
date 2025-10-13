# Implementation Next Steps - LLM Quick Start Guide

## Current State (Phase 1 Complete)
✅ Project structure and CLI framework implemented  
✅ NuGet packages configured  
✅ Command-line parsing working  

## Immediate Next Steps (Phase 2 Start)

### Step 1: Adapt Reference Models (Start Here)
**Location**: Copy and adapt models from `.github\reference-files\Models\`

**Order of Implementation**:
1. Copy `Yaml/Root.cs`, `Yaml/Item.cs`, `Yaml/Reference.cs` to `src/Models/Yaml/`
2. Copy core documentation models to `src/Models/`:
   - `TypeDocumentation.cs`
   - `ParameterDocumentation.cs` 
   - `ReturnDocumentation.cs`
   - `ExceptionDocumentation.cs`
   - `ItemType.cs`
   - `Link.cs`

**Key Adaptations Needed**:
- Update namespaces to `DocFXMustache.Models`
- Add any missing using statements
- Ensure compatibility with VYaml library

### Step 2: Implement MetadataParsingService
**File**: `src/Services/MetadataParsingService.cs`

**Test with**: `.github\reference-files\api\SadConsole.ColoredGlyph.yml`

**Core Methods**:
```csharp
public class MetadataParsingService
{
    public async Task<Root> ParseYamlFileAsync(string filePath);
    public async Task<IEnumerable<Root>> ParseDirectoryAsync(string directoryPath);
    public bool ValidateMetadata(Root metadata);
}
```

### Step 3: Implement DiscoveryService (Pass 1)
**File**: `src/Services/DiscoveryService.cs`

**Purpose**: Build UID mappings from all metadata files

**Core Methods**:
```csharp
public class DiscoveryService  
{
    public async Task<UidMappings> BuildUidMappingsAsync(string inputDirectory, string groupingStrategy);
    public Dictionary<string, string> ExtractUidsFromMetadata(Root metadata);
    public string DetermineOutputPath(Item item, string groupingStrategy);
}
```

### Step 4: Create UidMappings Data Structure
**File**: `src/Models/UidMappings.cs`

**Purpose**: Store results from Pass 1 for use in Pass 2

```csharp
public class UidMappings
{
    public Dictionary<string, string> UidToFilePath { get; set; }
    public Dictionary<string, Item> UidToItem { get; set; }
    public Dictionary<string, string> AssemblyMappings { get; set; }
}
```

## Two-Pass Architecture Implementation

### Pass 1: Discovery (Weeks 2)
1. Parse all YAML files → `Root` objects
2. Extract all UIDs from items
3. Apply grouping strategy to determine output paths
4. Build comprehensive UID → file path mapping
5. Store in `UidMappings` object

### Pass 2: Generation (Week 3)
1. For each metadata file:
   - Process XRef tags using pre-built mappings
   - Apply Mustache templates
   - Generate output files

## File Organization

```
src/
├── Models/
│   ├── Yaml/              # YAML structure models (adapted from reference)
│   │   ├── Root.cs
│   │   ├── Item.cs
│   │   └── Reference.cs
│   ├── TypeDocumentation.cs    # Main doc models
│   ├── Link.cs
│   └── UidMappings.cs     # Pass 1 results
├── Services/
│   ├── MetadataParsingService.cs    # Parse YAML → Models
│   ├── DiscoveryService.cs          # Pass 1: Build UID mappings
│   ├── XrefProcessingService.cs     # Pass 2: Process links
│   └── DocumentationGenerator.cs    # Orchestrate workflow
└── CLI/
    └── (existing Program.cs)
```

## Test-Driven Development

For each service, start with a test using reference files:

```csharp
[TestMethod]
public async Task ParseYaml_SadConsoleColoredGlyph_ReturnsValidModel()
{
    var service = new MetadataParsingService();
    var result = await service.ParseYamlFileAsync(
        @".github\reference-files\api\SadConsole.ColoredGlyph.yml");
    
    Assert.IsNotNull(result);
    Assert.AreEqual("SadConsole.ColoredGlyph", result.Items[0].Uid);
}
```

## Quick Commands for Testing

```bash
# Test with sample data
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --dry-run -v

# Test single grouping strategy
DocFXMustache -i ".github/reference-files/api" -o "./test-output" -t "./templates" -f md --grouping namespace --dry-run
```

## Success Criteria for Phase 2

- [ ] Can parse all reference YAML files without errors
- [ ] UID mappings correctly built for all items
- [ ] File grouping strategies generate correct output paths
- [ ] Pass 1 completes and stores mappings for Pass 2
- [ ] Basic validation and error handling working

**Next Document**: Once Phase 2 is complete, see `phases.md` for Phase 3 (Link Resolution & Templates)