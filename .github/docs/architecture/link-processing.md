# Link Processing System

A critical aspect of generating documentation from DocFX metadata is handling links between types and members. The system must resolve UIDs (Unique Identifiers) from metadata files to relative paths in the generated output, accounting for different file organization strategies.

## Overview

DocFX metadata files contain two types of links:
1. **Internal Links**: References to types/members within the same documentation set (identified by UID)
2. **External Links**: References to external documentation (e.g., Microsoft docs, NuGet packages)

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

### Link Template Approach (Chosen Implementation)
The system uses a dedicated **`link.mustache` template** to format individual links. XrefProcessingService resolves UIDs and processes each link through the link template, then injects the rendered links back into the content.

**Design Principle**: XrefProcessingService resolves UIDs to paths and creates `LinkInfo` data objects, but **delegates link formatting to `link.mustache`**. This keeps the service platform-agnostic while giving templates full control over link rendering.

**Processing Flow:**
```
Raw YAML → Extract xref → Resolve UID → Create LinkInfo → Render link.mustache → Inject into content
```

**Raw YAML Content:**
```yaml
summary: "Based on the <xref href=\"SadConsole.Ansi.State.Bold\" data-throw-if-not-resolved=\"false\"></xref> property."
```

**Step 1: XrefProcessingService extracts and resolves:**
```csharp
// Found xref tag with UID "SadConsole.Ansi.State.Bold"
var linkInfo = new LinkInfo 
{
    Uid = "SadConsole.Ansi.State.Bold",
    DisplayName = "Bold",
    RelativePath = "../../Bold.md",
    IsExternal = false
};
```

**Step 2: Render through `link.mustache`:**
```mustache
<!-- templates/basic/link.mustache -->
[{{displayName}}]({{relativePath}})
```
Result: `[Bold](../../Bold.md)`

**Step 3: Final template data:**
```json
{
  "summary": "Based on the [Bold](../../Bold.md) property."
}
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

Main templates (e.g., `class.mustache`) receive fully processed content:

```mustache
# {{name}} Class

## Summary
{{{summary}}}
<!-- Triple braces {{{ }}} render unescaped (preserves markdown/HTML) -->

## Remarks
{{{remarks}}}
```

### Benefits of This Approach
1. **Template Control**: Link formatting entirely controlled by `link.mustache`
2. **Platform-Agnostic Core**: XrefProcessingService has no format-specific logic
3. **Easy Extensibility**: New output formats only require new `link.mustache`
4. **Simple Integration**: Main templates just render pre-processed strings
5. **Testability**: Link rendering can be tested independently

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

### 1. Link Resolution Service
```csharp
public class LinkResolutionService
{
    private readonly Dictionary<string, OutputFileInfo> _uidToFileMap;
    private readonly TemplateConfiguration _templateConfig;
    
    public void BuildUidMappings(IEnumerable<Item> items, TemplateConfiguration config);
    public string ResolveInternalLink(string fromPath, string targetUid);
    public string ResolveExternalLink(string uid, string fallbackUrl);
    public bool IsExternalReference(string uid);
    public ProcessedContent ProcessXrefTags(string content, string currentFilePath);
    
    // NEW: Determine if member should be on separate page or parent page
    private OutputFileInfo DetermineOutputLocation(Item item);
    
    // NEW: Generate anchor for members on parent pages
    private string GenerateAnchor(string memberName);
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

### 2. XRef Processing Service
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
    /// Processes xref tags by rendering each through link.mustache template.
    /// Returns content with all xrefs replaced by formatted links.
    /// </summary>
    public string ProcessXrefs(string content, string currentFilePath)
    {
        return XrefPattern.Replace(content, match => 
        {
            var uid = match.Groups[1].Value;
            
            // Create link info object
            var linkInfo = CreateLinkInfo(uid, currentFilePath);
            
            // Render through link.mustache template
            return RenderLink(linkInfo);
        });
    }
    
    public LinkInfo CreateLinkInfo(string uid, string currentFilePath)
    {
        var resolvedPath = _linkResolver.ResolveInternalLink(currentFilePath, uid);
        var isExternal = _linkResolver.IsExternalReference(uid);
        
        return new LinkInfo 
        { 
            Uid = uid, 
            DisplayName = ExtractDisplayName(uid), 
            RelativePath = resolvedPath,
            IsExternal = isExternal,
            ExternalUrl = isExternal ? _linkResolver.ResolveExternalLink(uid, null) : null
        };
    }
    
    public string RenderLink(LinkInfo linkInfo)
    {
        // Render using link.mustache template
        return _renderer.Render(_linkTemplate, linkInfo);
    }
    
    private string ExtractDisplayName(string uid)
    {
        // Extract simple name from UID (e.g., "SadConsole.ColoredGlyph" -> "ColoredGlyph")
        var lastDot = uid.LastIndexOf('.');
        return lastDot >= 0 ? uid.Substring(lastDot + 1) : uid;
    }
}
```

### 3. Data Models
```csharp
public class LinkInfo
{
    public string Uid { get; set; }
    public string DisplayName { get; set; }
    public string RelativePath { get; set; }
    public bool IsExternal { get; set; }
    public string ExternalUrl { get; set; }
}

// Note: ProcessedContent class not needed - XrefProcessingService returns string directly
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