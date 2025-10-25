# Link Processing System

A critical aspect of generating documentation from DocFX metadata is handling links between types and members. The system must resolve UIDs (Unique Identifiers) from metadata files to relative paths in the generated output, accounting for different file organization strategies.

## Overview

DocFX metadata files contain two types of links:
1. **Internal Links**: References to types/members within the same documentation set (identified by UID)
2. **External Links**: References to external documentation (e.g., Microsoft docs, NuGet packages)

## Two-Pass Processing Strategy

The system uses a **two-pass approach** to handle the complexity of link resolution when template decisions affect file organization:

### Pass 1: Template Rendering with Raw XRefs
1. **Generate all documentation files** from Mustache templates
2. **Preserve `<xref>` tags** in generated content (don't resolve yet)
3. **Build UID-to-file mapping** based on *actual generated output*
   - Record which UIDs got their own files
   - Record which UIDs are anchors on parent pages
   - Capture actual file paths and anchor names

**Why Keep XRefs in Pass 1?**
- Templates control file organization (separate vs. combined pages)
- Can't know final paths until templates execute
- UID mapping reflects reality, not assumptions

### Pass 2: XRef Resolution & Link Rendering
1. **Read generated files** from Pass 1
2. **Process each `<xref>` tag**:
   - Extract target UID
   - Resolve using UID-to-file mapping from Pass 1
   - Create `LinkInfo` object with resolved path/anchor
   - **Render through `link.mustache` template**
   - Replace `<xref>` tag with rendered link
3. **Write updated files** with resolved links

**Benefits:**
- Accurate link resolution based on actual output structure
- Template controls link formatting via `link.mustache`
- Handles both separate and combined page strategies
- External links resolved consistently

## UID-Based Link Resolution

### Link Resolution Process

#### 1. UID Mapping (Template-Aware)
During the metadata parsing phase, the system builds a comprehensive UID-to-file mapping. **Critically, this mapping must account for template decisions about file organization** - whether types, methods, and properties get individual files or are combined.

**Template Organization Strategies:**
- **Separate Pages**: Each type, method, property gets its own file
  - `SadConsole.ColoredGlyph` → `ColoredGlyph.md`
  - `SadConsole.ColoredGlyph.Foreground` → `ColoredGlyph.Foreground.md`
  
- **Combined Pages** (Common): Types contain members inline
  - `SadConsole.ColoredGlyph` → `ColoredGlyph.md`
  - `SadConsole.ColoredGlyph.Foreground` → `ColoredGlyph.md#foreground` (anchor link)
  - `SadConsole.ColoredGlyph.Clone()` → `ColoredGlyph.md#clone` (anchor link)

**Implementation:** See `src/Services/LinkResolutionService.cs` and `src/Models/OutputFileInfo.cs` for the actual implementation of UID-to-file mapping.

#### 2. Context-Aware Path Resolution
The link resolver must consider the context (current file location) when generating relative paths:

**Example Scenario 1 - Separate Files:**
- Current file: `output/SadConsole/UI/Controls/Button.md`
- Target UID: `SadConsole.ColoredGlyph` (separate file)
- Target file: `output/SadConsole/ColoredGlyph.md`
- Required relative path: `../../ColoredGlyph.md`

**Example Scenario 2 - Combined Files (Member Link):**
- Current file: `output/SadConsole/UI/Controls/Button.md`
- Target UID: `SadConsole.ColoredGlyph.Foreground` (property on parent type page)
- Target file: `output/SadConsole/ColoredGlyph.md`
- Target anchor: `#foreground`
- Required relative path: `../../ColoredGlyph.md#foreground`

**Example Scenario 3 - Link Within Same File:**
- Current file: `output/SadConsole/ColoredGlyph.md`
- Target UID: `SadConsole.ColoredGlyph.Clone()` (method on same page)
- Required relative path: `#clone` (anchor-only link)

## XRef Tag Processing

### Link Template Approach (Two-Pass Implementation)
The system uses a **two-pass process** combined with a dedicated **`link.mustache` template** for link formatting.

**Design Principle**: 
- **Pass 1**: Templates render content with `<xref>` tags preserved, building accurate UID mappings
- **Pass 2**: XrefProcessingService resolves UIDs using actual output paths, renders through `link.mustache`

**Complete Processing Flow:**
```
Pass 1: YAML → Mustache Template → Generated File (with <xref> tags) → Update UID Mapping
Pass 2: Read File → Extract <xref> → Resolve UID → Create LinkInfo → Render link.mustache → Write File
```

**Raw YAML Content:**
```yaml
summary: "Based on the <xref href=\"SadConsole.Ansi.State.Bold\" data-throw-if-not-resolved=\"false\"></xref> property."
```

**Pass 1: Template Rendering (XRefs Preserved)**
```mustache
<!-- templates/basic/class.mustache -->
## Summary
{{{summary}}}
```

**Pass 1 Output (ColoredGlyph.md):**
```markdown
## Summary
Based on the <xref href="SadConsole.Ansi.State.Bold" data-throw-if-not-resolved="false"></xref> property.
```

**Pass 1: UID Mapping Updated**
```
SadConsole.Ansi.State.Bold → output/SadConsole/Ansi/State.md#bold
SadConsole.ColoredGlyph → output/SadConsole/ColoredGlyph.md
```

**Pass 2: XRef Resolution**

**Step 1: Read generated file, extract xref with UID "SadConsole.Ansi.State.Bold"**

**Step 2: Resolve UID using mapping from Pass 1:**
```csharp
var linkInfo = new LinkInfo 
{
    Uid = "SadConsole.Ansi.State.Bold",
    DisplayName = "Bold",
    RelativePath = "Ansi/State.md#bold",  // Resolved from actual Pass 1 output
    IsExternal = false
};
```

**Step 3: Render through `link.mustache`:**
```mustache
<!-- templates/basic/link.mustache -->
[{{displayName}}]({{relativePath}})
```
Result: `[Bold](Ansi/State.md#bold)`

**Step 4: Final output (ColoredGlyph.md updated):**
```markdown
## Summary
Based on the [Bold](Ansi/State.md#bold) property.
```

### Link Template Examples

Different template directories provide different `link.mustache` implementations:

```mustache
<!-- templates/basic/link.mustache - Standard Markdown -->
[{{displayName}}]({{relativePath}})

<!-- templates/starlight/link.mustache - Astro/Starlight -->
[{{displayName}}]({{relativePath}})

<!-- templates/mdx/link.mustache - MDX with custom component -->
<ApiLink href="{{relativePath}}">{{displayName}}</ApiLink>

<!-- templates/docusaurus/link.mustache - Docusaurus format -->
[{{displayName}}](./{{relativePath}})

<!-- templates/html/link.mustache - HTML output -->
<a href="{{relativePath}}" class="api-link">{{displayName}}</a>
```

### Main Template Usage

Main templates (e.g., `class.mustache`) render with XRefs intact during Pass 1:

```mustache
# {{name}} Class

## Summary
{{{summary}}}
<!-- Triple braces {{{ }}} render unescaped - preserves <xref> tags in Pass 1 -->
<!-- XRef tags resolved to links in Pass 2 -->

## Remarks
{{{remarks}}}
```

**Pass 1 renders this as-is with `<xref>` tags preserved.**  
**Pass 2 replaces `<xref>` tags with rendered links from `link.mustache`.**

### Benefits of Two-Pass + Template Approach
1. **Accurate Resolution**: UID mappings based on actual generated output, not assumptions
2. **Template Control**: Link formatting entirely controlled by `link.mustache`
3. **Flexible Organization**: Templates decide file structure; links resolve correctly regardless
4. **Platform-Agnostic**: XrefProcessingService has no format-specific logic
5. **Easy Extensibility**: New output formats only require new `link.mustache`
6. **Handles Edge Cases**: Anchors, combined pages, relative paths all work correctly
7. **Testability**: Each pass independently testable

## File Grouping Impact on Links

Different file grouping strategies require different link resolution logic:

### Flat Structure Links
```
Button.md → ColoredGlyph.md
Relative path: "./ColoredGlyph.md"
```

### Namespace Hierarchy Links
```
SadConsole/UI/Controls/Button.md → SadConsole/ColoredGlyph.md  
Relative path: "../../ColoredGlyph.md"
```

### Assembly-Namespace Links
```
SadConsole/SadConsole/UI/Controls/Button.md → SadConsole/SadConsole/ColoredGlyph.md
Relative path: "../../ColoredGlyph.md"
```

## Implementation Components

The link processing system is implemented across several service classes and models:

### 1. Link Resolution Service (Pass 1 & 2) ✅
**Implementation:** `src/Services/LinkResolutionService.cs` - **COMPLETE**

**Key Responsibilities:**
- **Pass 1**: Records generated file paths and external YAML references
  - `RecordGeneratedFile()` - Stores UID-to-file mappings for generated documentation
  - `RecordExternalReference()` - Stores href values from YAML reference sections
- **Pass 2**: Resolves links using priority-based logic
  - `IsExternalReference()` - Checks if UID was generated (internal) or not (external)
  - `ResolveInternalLink()` - Calculates relative paths between files
  - `ResolveExternalLink()` - Returns YAML href or generates fallback URL
  - `CalculateRelativePath()` - Path calculation utility

**Data Models:** `src/Models/OutputFileInfo.cs`

### 2. XRef Processing Service (Pass 2 Only) ✅
**Implementation:** `src/Services/XrefProcessingService.cs` - **COMPLETE**

**Key Responsibilities:**
- Extracts `<xref>` tags from generated markdown using regex
- Resolves UIDs to `LinkInfo` objects using `LinkResolutionService`
- Renders links through `link.mustache` template
- Replaces xref tags with formatted links
- Generates user-friendly display names from UIDs

**Data Models:** `src/Models/LinkInfo.cs`

### 3. Template Processing Service (Pass 1 Only) ✅
**Implementation:** `src/Services/TemplateProcessingService.cs` - **COMPLETE**

**Responsibilities:**
- Render Mustache templates with YAML data
- Preserve `<xref>` tags in output (don't resolve during Pass 1)
- Support different template packs (basic, starlight, etc.)

### 4. Documentation Generator (Orchestrates Both Passes) ✅
**Implementation:** `src/Program.cs` - **COMPLETE**

**Workflow:**
1. **Pass 1**: Render all templates → Write files → Record UID mappings
2. **Pass 2**: Read files → Process xrefs → Write updated files

**Key Operations:**
- Determine output paths based on template configuration
- Record generated files and anchor mappings in `LinkResolutionService`
- Coordinate between `TemplateProcessingService` and `XrefProcessingService`

## External References

External references are resolved using a **priority-based approach** that leverages the 2-pass architecture:

### Resolution Priority (Pass 2)

1. **Generated Files First (Internal)**: If `RecordGeneratedFile()` was called for a UID in Pass 1, it's internal
   - Use the generated file path with `ResolveInternalLink()`
   - Overrides any YAML reference metadata

2. **YAML Reference Metadata (External)**: If UID is in YAML `references` section
   - Use the `href` value from metadata (absolute URL or relative path)
   - Example: `System.String` → `https://learn.microsoft.com/dotnet/api/system.string`

3. **Fallback Generation**: For unknown UIDs
   - `System.*` and `Microsoft.*` → Generate Microsoft Docs URL
   - Other types → Use provided fallback URL or generate generic URL

### YAML Reference Examples

```yaml
references:
# External type with absolute URL (truly external)
- uid: System.String
  name: String
  fullName: System.String
  isExternal: true
  href: https://learn.microsoft.com/dotnet/api/system.string

# Internal type with relative path (will be overridden if we generate it)
- uid: SadConsole.ColoredGlyph
  name: ColoredGlyph
  isExternal: true  # DocFX marks as external, but we generate it
  href: SadConsole.ColoredGlyph.html  # Our Pass 1 generation overrides this
```

### Key Insight: Generated Files Win

If we generate a file for `SadConsole.ColoredGlyph` in Pass 1:
- **YAML says**: `href: "SadConsole.ColoredGlyph.html"` (external)
- **We say**: `C:\output\SadConsole\ColoredGlyph.md` (internal)
- **Result**: Our generated path takes precedence → Internal link

This allows the YAML references to contain relative paths for documentation we might generate, while still using the actual hrefs for truly external types.

## Cross-Reference Index Generation

The system generates index files with proper cross-references:

### Assembly Index Example
```markdown
# SadConsole Assembly

## Types
- [ColoredGlyph](./SadConsole/ColoredGlyph.md)
- [UI.Controls.Button](./SadConsole/UI/Controls/Button.md)

## Namespaces  
- [SadConsole](./SadConsole/README.md)
- [SadConsole.UI](./SadConsole/UI/README.md)
```

### Namespace Index Example
```markdown
# SadConsole.UI.Controls Namespace

## Classes
- [Button](./Button.md) - A clickable UI control
- [TextBox](./TextBox.md) - Text input control
```

## Error Handling

- **Broken UIDs**: Log warnings for unresolvable UIDs
- **Circular References**: Detect and handle circular link chains
- **External Link Validation**: Optional validation of external URLs
- **Fallback Strategies**: Graceful degradation when links cannot be resolved