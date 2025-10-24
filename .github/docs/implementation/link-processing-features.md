# Link Processing Features

## Overview
This document maps the key features required for the link processing system based on the [Link Processing Architecture](../architecture/link-processing.md).

## Core Features

### 1. XRef Tag Parsing & Processing
**Purpose**: Extract and process `<xref>` tags from generated markdown content

**Key Capabilities**:
- Parse `<xref href="UID" data-throw-if-not-resolved="false"></xref>` tags
- Extract UID from href attribute
- Handle both self-closing and paired tags
- Preserve non-xref content unchanged

**Test Scenarios**:
- Single xref in text
- Multiple xrefs in same content
- Mixed content (text + xrefs)
- Edge cases (malformed tags, empty hrefs)

### 2. UID Resolution
**Purpose**: Resolve UIDs to output file paths using Pass 1 mappings

**Key Capabilities**:
- Look up UID in UidMappings from Pass 1
- Distinguish between internal and external references
- Handle missing UIDs gracefully
- Support both type UIDs and member UIDs (with anchors)

**Test Scenarios**:
- Internal type reference (separate file)
- Internal member reference (anchor on parent page)
- External reference (System.String)
- Missing/unknown UID
- Same-file reference (anchor only)

### 3. Relative Path Calculation
**Purpose**: Calculate correct relative paths between source and target files

**Key Capabilities**:
- Calculate relative path from current file to target file
- Handle different directory depths
- Support same-directory links
- Support parent/ancestor directory navigation
- Add anchor fragments for member links

**Test Scenarios**:
- Same directory: `Button.md` → `TextBox.md` = `./TextBox.md`
- Parent directory: `UI/Button.md` → `ColoredGlyph.md` = `../ColoredGlyph.md`
- Deep navigation: `UI/Controls/Button.md` → `ColoredGlyph.md` = `../../ColoredGlyph.md`
- With anchor: `Button.md` → `ColoredGlyph.md#foreground` = `./ColoredGlyph.md#foreground`
- Same file anchor: `ColoredGlyph.md` → `ColoredGlyph.md#clone` = `#clone`

### 4. Link Template Rendering
**Purpose**: Render resolved links using Mustache template

**Key Capabilities**:
- Create LinkInfo model with resolved data
- Render through `link.mustache` template
- Support custom link formats per template pack
- Generate proper markdown links by default

**Test Scenarios**:
- Basic markdown link: `[DisplayName](path)`
- Link with anchor: `[Property](../Type.md#property)`
- External link: `[String](https://docs.microsoft.com/...)`

### 5. External Reference Handling
**Purpose**: Resolve external UIDs to external documentation URLs

**Key Capabilities**:
- Detect external references (System.*, Microsoft.*, etc.)
- Map to external documentation URLs
- Use reference metadata from YAML files
- Provide fallback URLs when metadata missing

**Test Scenarios**:
- System.String → Microsoft Docs URL
- System.Collections.Generic.List → Microsoft Docs URL
- Custom external reference with href in metadata

### 6. Link Display Name Extraction
**Purpose**: Generate user-friendly display names from UIDs

**Key Capabilities**:
- Extract type name from fully qualified UID
- Handle generic types (List<T>)
- Handle method names with parameters
- Use short names for readability

**Test Scenarios**:
- Type: `SadConsole.ColoredGlyph` → `ColoredGlyph`
- Property: `SadConsole.ColoredGlyph.Foreground` → `Foreground`
- Method: `SadConsole.ColoredGlyph.Clone` → `Clone`
- Generic: `System.Collections.Generic.List\`1` → `List`

## Implementation Priority

### Phase 1: Core Link Resolution (Week 1) ✅ COMPLETE
1. ✅ UidMappings model (already exists)
2. ✅ LinkResolutionService - Record and resolve UIDs (Oct 24, 2025)
3. ✅ Relative path calculation utilities (Oct 24, 2025)
4. ✅ Basic LinkInfo and OutputFileInfo models (Oct 24, 2025)

### Phase 2: XRef Processing (Week 1-2) ✅ COMPLETE
1. ✅ XRef regex pattern and parsing (Oct 24, 2025)
2. ✅ XrefProcessingService - Process xrefs in content (Oct 24, 2025)
3. ✅ Integration with LinkResolutionService (Oct 24, 2025)
4. ✅ Display name extraction (Oct 24, 2025)

### Phase 3: Template Integration (Week 2) 🚧 IN PROGRESS
1. ✅ link.mustache template (Oct 24, 2025)
2. ✅ Template rendering in XrefProcessingService (Oct 24, 2025)
3. ⏳ Support for different template packs
4. ⏳ Link format customization

### Phase 4: External References (Week 2-3) ✅ COMPLETE
1. ✅ External UID detection (Oct 24, 2025)
2. ✅ Reference metadata parsing (Oct 24, 2025)
3. ✅ Fallback URL generation (Oct 24, 2025)
4. ⏳ External link validation

## Data Models

### LinkInfo
```csharp
public class LinkInfo
{
    public string Uid { get; set; }
    public string DisplayName { get; set; }
    public string RelativePath { get; set; }
    public bool IsExternal { get; set; }
}
```

### OutputFileInfo (extend UidMappings)
```csharp
public class OutputFileInfo
{
    public string FilePath { get; set; }
    public string? Anchor { get; set; }  // For member links
}
```

## Test Data Sources

We have excellent real-world test data in `tests/Fixtures/`:
- `SadConsole.ColoredGlyph.yml` - Has xrefs in summary/remarks
- `SimpleClass.yml` - Basic type with references
- `SystemCollectionsGenericList.yml` - External type references
- `MultipleNamespaces.yml` - Cross-namespace references

## Success Metrics

- [x] All xref tags correctly parsed from real YAML content ✅
- [x] Internal UIDs resolve to correct relative paths ✅
- [x] External UIDs resolve to Microsoft Docs URLs ✅
- [x] Relative paths work across all grouping strategies ✅
- [x] Anchors generated correctly for member links ✅
- [x] Templates render links in expected format ✅
- [x] Edge cases handled gracefully (missing UIDs, malformed xrefs) ✅

### Test Coverage (Oct 24, 2025)
- **LinkResolutionService**: 24 tests passing
  - UID recording for generated files (Pass 1)
  - External reference recording from YAML hrefs (Pass 1)
  - Relative path calculation (same dir, parent, deep, different branch)
  - Internal link resolution with anchors
  - External reference detection (generated file check)
  - External link resolution from YAML hrefs
  - Fallback URL generation for System.*/Microsoft.* types
  - Priority-based resolution (Generated → YAML → Fallback)
  - Error handling for unknown UIDs
- **XrefProcessingService**: 17 tests passing
  - XRef tag extraction (single, multiple, mixed content)
  - XRef processing and replacement
  - Internal links with anchors
  - External links
  - Same-file anchor links
  - Cross-namespace links
  - Display name extraction
  - Template rendering
- **Total**: 112 tests passing (71 existing + 41 new link processing tests)
