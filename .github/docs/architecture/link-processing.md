# Link Processing System

A critical aspect of generating documentation from DocFX metadata is handling links between types and members. The system must resolve UIDs (Unique Identifiers) from metadata files to relative paths in the generated output, accounting for different file organization strategies.

## Overview

DocFX metadata files contain two types of links:
1. **Internal Links**: References to types/members within the same documentation set (identified by UID)
2. **External Links**: References to external documentation (e.g., Microsoft docs, NuGet packages)

## UID-Based Link Resolution

### Link Resolution Process

#### 1. UID Mapping
During the metadata parsing phase, the system builds a comprehensive UID-to-file mapping:

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

## XRef Tag Processing

### Data Transformation Approach
The system parses xref tags and transforms them into structured data, but templates decide the final rendering:

**Raw YAML Content:**
```yaml
summary: "Based on the <xref href=\"SadConsole.Ansi.State.Bold\" data-throw-if-not-resolved=\"false\"></xref> property."
```

**Transformed Template Data:**
```json
{
  "summary": {
    "text": "Based on the {0} property.",
    "links": [
      {
        "uid": "SadConsole.Ansi.State.Bold",
        "displayName": "Bold",
        "relativePath": "../../Bold.md",
        "isExternal": false
      }
    ]
  }
}
```

### Template-Controlled Rendering
Templates choose how to render these links:

```mustache
<!-- For Markdown output -->
{{summary.text}}{{#summary.links}} [{{displayName}}]({{relativePath}}){{/summary.links}}

<!-- For MDX with custom components -->
{{summary.text}}{{#summary.links}} <ApiLink href="{{relativePath}}">{{displayName}}</ApiLink>{{/summary.links}}

<!-- For HTML output -->
{{summary.text}}{{#summary.links}} <a href="{{relativePath}}">{{displayName}}</a>{{/summary.links}}
```

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
    public void BuildUidMappings(IEnumerable<MetadataFile> metadataFiles);
    public string ResolveInternalLink(string fromPath, string targetUid);
    public string ResolveExternalLink(string uid, string fallbackUrl);
    public bool IsExternalReference(string uid);
    public ProcessedContent ProcessXrefTags(string content, string currentFilePath);
}
```

### 2. XRef Processing Service
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
```

### 3. Data Models
```csharp
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