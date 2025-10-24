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

**UID Mapping Creation:**
```csharp
public class UidResolver
{
    private readonly Dictionary<string, OutputFileInfo> _uidToFileMap;
    private readonly IFileOrganizationStrategy _organizationStrategy;
    private readonly TemplateConfiguration _templateConfig;
    
    public void BuildUidMappings(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            // Determine output location based on template configuration
            var outputLocation = DetermineOutputLocation(item);
            
            _uidToFileMap[item.Uid] = outputLocation;
            
            // If template combines members with parent type, map members to parent file
            if (_templateConfig.CombineMembersWithType)
            {
                foreach (var child in item.Children ?? Enumerable.Empty<string>())
                {
                    var childItem = FindItemByUid(child);
                    if (childItem != null && IsMember(childItem))
                    {
                        // Member UIDs point to parent file with anchor
                        _uidToFileMap[child] = new OutputFileInfo
                        {
                            FilePath = outputLocation.FilePath,
                            Anchor = GenerateAnchor(childItem.Name)
                        };
                    }
                }
            }
        }
    }
    
    public string ResolveUidToRelativePath(string fromFilePath, string targetUid)
    {
        var targetInfo = _uidToFileMap[targetUid];
        var relativePath = CalculateRelativePath(fromFilePath, targetInfo.FilePath);
        
        // Add anchor if member is on parent type page
        if (!string.IsNullOrEmpty(targetInfo.Anchor))
        {
            relativePath += $"#{targetInfo.Anchor}";
        }
        
        return relativePath;
    }
}

public class OutputFileInfo
{
    public string FilePath { get; set; }      // e.g., "output/SadConsole/ColoredGlyph.md"
    public string Anchor { get; set; }        // e.g., "foreground" (optional, for members)
}

public class TemplateConfiguration
{
    public bool CombineMembersWithType { get; set; }  // true = methods/props in type file
    public bool GenerateIndexFiles { get; set; }
    public bool IncludeInheritedMembers { get; set; }
}
```

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

### 1. Link Resolution Service (Pass 1 & 2)
```csharp
public class LinkResolutionService
{
    private readonly Dictionary<string, OutputFileInfo> _uidToFileMap;
    
    // PASS 1: Build UID mappings from generated output
    public void RecordGeneratedFile(string uid, string filePath, string anchor = null)
    {
        _uidToFileMap[uid] = new OutputFileInfo 
        { 
            FilePath = filePath, 
            Anchor = anchor 
        };
    }
    
    // PASS 2: Resolve links using recorded mappings
    public string ResolveInternalLink(string fromPath, string targetUid);
    public string ResolveExternalLink(string uid, string fallbackUrl);
    public bool IsExternalReference(string uid);
    public OutputFileInfo GetOutputInfo(string uid);
}

public class OutputFileInfo
{
    public string FilePath { get; set; }      // Full path to output file
    public string Anchor { get; set; }        // Optional anchor for members (e.g., "#foreground")
}

public class TemplateConfiguration
{
    public bool CombineMembersWithType { get; set; }  // Combine methods/props with parent type
    public bool GenerateIndexFiles { get; set; }       // Generate namespace/assembly index files
    public bool IncludeInheritedMembers { get; set; }  // Include inherited members in output
}
```

### 2. XRef Processing Service (Pass 2 Only)
```csharp
public class XrefProcessingService
{
    private static readonly Regex XrefPattern = new Regex(
        @"<xref\s+href=""([^""]+)""[^>]*></xref>", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    private readonly LinkResolutionService _linkResolver;
    private readonly IStubbleRenderer _renderer;
    private readonly string _linkTemplate;
    
    public XrefProcessingService(LinkResolutionService linkResolver, string templateDirectory)
    {
        _linkResolver = linkResolver;
        _renderer = new StubbleBuilder().Build();
        _linkTemplate = File.ReadAllText(Path.Combine(templateDirectory, "link.mustache"));
    }
    
    /// <summary>
    /// PASS 2: Processes xref tags in generated files.
    /// Resolves UIDs using Pass 1 mappings and renders through link.mustache template.
    /// Returns content with all xrefs replaced by formatted links.
    /// </summary>
    public string ProcessXrefs(string content, string currentFilePath)
    {
        return XrefPattern.Replace(content, match => 
        {
            var uid = match.Groups[1].Value;
            
            // Resolve using Pass 1 mappings
            var linkInfo = CreateLinkInfo(uid, currentFilePath);
            
            // Render through link.mustache template
            return RenderLink(linkInfo);
        });
    }
    
    public LinkInfo CreateLinkInfo(string uid, string currentFilePath)
    {
        var isExternal = _linkResolver.IsExternalReference(uid);
        
        if (isExternal)
        {
            return new LinkInfo 
            { 
                Uid = uid, 
                DisplayName = ExtractDisplayName(uid), 
                RelativePath = _linkResolver.ResolveExternalLink(uid, null),
                IsExternal = true
            };
        }
        
        // Get output info from Pass 1
        var outputInfo = _linkResolver.GetOutputInfo(uid);
        var relativePath = CalculateRelativePath(currentFilePath, outputInfo.FilePath);
        
        if (!string.IsNullOrEmpty(outputInfo.Anchor))
        {
            relativePath += $"#{outputInfo.Anchor}";
        }
        
        return new LinkInfo 
        { 
            Uid = uid, 
            DisplayName = ExtractDisplayName(uid), 
            RelativePath = relativePath,
            IsExternal = false
        };
    }
    
    public string RenderLink(LinkInfo linkInfo)
    {
        // Render using link.mustache template
        return _renderer.Render(_linkTemplate, linkInfo);
    }
    
    private string ExtractDisplayName(string uid)
    {
        var lastDot = uid.LastIndexOf('.');
        return lastDot >= 0 ? uid.Substring(lastDot + 1) : uid;
    }
    
    private string CalculateRelativePath(string fromPath, string toPath)
    {
        // Calculate relative path between two file paths
        // Implementation details...
    }
}
```

### 3. Template Processing Service (Pass 1 Only)
```csharp
public class TemplateProcessingService
{
    private readonly IStubbleRenderer _renderer;
    private readonly Dictionary<string, string> _templates;
    
    /// <summary>
    /// PASS 1: Render template with data, preserving XRef tags in output.
    /// </summary>
    public string RenderTemplate(string templateName, object data)
    {
        var template = _templates[templateName];
        return _renderer.Render(template, data);
        // Note: XRef tags in data.Summary, data.Remarks, etc. are preserved
    }
}
```

### 4. Documentation Generator (Orchestrates Both Passes)
```csharp
public class DocumentationGenerator
{
    private readonly TemplateProcessingService _templateService;
    private readonly XrefProcessingService _xrefService;
    private readonly LinkResolutionService _linkResolver;
    private readonly FileGenerationService _fileService;
    
    public async Task GenerateDocumentationAsync(
        IEnumerable<Item> items, 
        string outputDirectory)
    {
        // PASS 1: Generate files with templates, build UID mappings
        var generatedFiles = new List<string>();
        
        foreach (var item in items)
        {
            // Render template (XRefs preserved in output)
            var content = _templateService.RenderTemplate(GetTemplateName(item), item);
            
            // Determine output path (template may affect this)
            var outputPath = DetermineOutputPath(item, outputDirectory);
            
            // Write file
            await _fileService.WriteFileAsync(outputPath, content);
            generatedFiles.Add(outputPath);
            
            // Record in UID mapping
            _linkResolver.RecordGeneratedFile(item.Uid, outputPath);
            
            // If members are on same page, record anchor mappings
            if (ShouldCombineMembers(item))
            {
                foreach (var childUid in item.Children ?? Enumerable.Empty<string>())
                {
                    var anchor = GenerateAnchor(childUid);
                    _linkResolver.RecordGeneratedFile(childUid, outputPath, anchor);
                }
            }
        }
        
        // PASS 2: Process XRefs in all generated files
        foreach (var filePath in generatedFiles)
        {
            var content = await _fileService.ReadFileAsync(filePath);
            var processedContent = _xrefService.ProcessXrefs(content, filePath);
            await _fileService.WriteFileAsync(filePath, processedContent);
        }
    }
}
```

### 5. Data Models
```csharp
public class LinkInfo
{
    public string Uid { get; set; }
    public string DisplayName { get; set; }
    public string RelativePath { get; set; }
    public bool IsExternal { get; set; }
}

public class OutputFileInfo
{
    public string FilePath { get; set; }      // Full path to generated file
    public string Anchor { get; set; }        // Optional anchor (e.g., "foreground" for members)
}
```

## External References

External references are resolved using DocFX's reference metadata:

```yaml
references:
- uid: System.String
  name: String
  fullName: System.String
  isExternal: true
  href: https://docs.microsoft.com/dotnet/api/system.string
```

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