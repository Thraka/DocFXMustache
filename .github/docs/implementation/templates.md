# Default Templates

## Template Organization Modes

Templates can organize API members in two different ways, controlled by template configuration:

### Mode 1: Combined Members (Recommended Default)
**All members are documented on the parent type's page with anchor links.**

- `SadConsole.ColoredGlyph` → `ColoredGlyph.md` (includes all members)
- `SadConsole.ColoredGlyph.Foreground` → `ColoredGlyph.md#foreground` (anchor link)
- `SadConsole.ColoredGlyph.Clone()` → `ColoredGlyph.md#clone` (anchor link)

**Advantages:**
- Single-page documentation per type
- Better for browsing and reading
- Reduces file count
- Common pattern in most documentation systems

**Template Configuration** (`template.json`):
```json
{
  "combineMembers": true,
  "generateIndexFiles": true,
  "includeInheritedMembers": false
}
```

### Mode 2: Separate Member Files
**Each member gets its own documentation file.**

- `SadConsole.ColoredGlyph` → `ColoredGlyph.md`
- `SadConsole.ColoredGlyph.Foreground` → `ColoredGlyph.Foreground.md`
- `SadConsole.ColoredGlyph.Clone()` → `ColoredGlyph.Clone.md`

**Advantages:**
- More granular organization
- Better for deep-linking to specific members
- Easier to track changes in version control (per-member)

**Template Configuration** (`template.json`):
```json
{
  "combineMembers": false,
  "generateIndexFiles": true,
  "includeInheritedMembers": false
}
```

### Impact on UID Mappings

The `combineMembers` setting affects how UIDs are mapped to file paths during Pass 1:

```csharp
// When combineMembers = true
_uidToFileMap["SadConsole.ColoredGlyph.Foreground"] = new OutputFileInfo
{
    FilePath = "ColoredGlyph.md",
    Anchor = "foreground"
};

// When combineMembers = false
_uidToFileMap["SadConsole.ColoredGlyph.Foreground"] = new OutputFileInfo
{
    FilePath = "ColoredGlyph.Foreground.md",
    Anchor = null
};
```

### Link Resolution Differences

**Combined Members:**
```markdown
See [Foreground](../../ColoredGlyph.md#foreground) property.
See [Clone](../../ColoredGlyph.md#clone) method.
See [ColoredGlyph](../../ColoredGlyph.md) class.
```

**Separate Files:**
```markdown
See [Foreground](../../ColoredGlyph.Foreground.md) property.
See [Clone](../../ColoredGlyph.Clone.md) method.
See [ColoredGlyph](../../ColoredGlyph.md) class.
```

## Template Configuration File

Each template directory should include a `template.json` configuration file:

**Location**: `templates/{template-name}/template.json`

**Schema**:
```json
{
  "name": "basic",
  "description": "Basic Markdown template for general documentation",
  "version": "1.0.0",
  "combineMembers": true,
  "generateIndexFiles": true,
  "includeInheritedMembers": false,
  "templates": {
    "class": "class.mustache",
    "interface": "interface.mustache",
    "enum": "enum.mustache",
    "struct": "struct.mustache",
    "namespace": "namespace.mustache",
    "assembly": "assembly.mustache",
    "link": "link.mustache",
    "member": "member.mustache"
  }
}
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `combineMembers` | boolean | `true` | Combine members with parent type vs. separate files |
| `generateIndexFiles` | boolean | `true` | Generate namespace and assembly index files |
| `includeInheritedMembers` | boolean | `false` | Include inherited members in type documentation |
| `templates.class` | string | `"class.mustache"` | Template file for classes |
| `templates.interface` | string | `"interface.mustache"` | Template file for interfaces |
| `templates.enum` | string | `"enum.mustache"` | Template file for enums |
| `templates.link` | string | `"link.mustache"` | Template for rendering individual links |
| `templates.member` | string | `"member.mustache"` | Template for individual members (when `combineMembers=false`) |

## Template Types

The system provides default Mustache templates for different API item types:

### 1. Class Template (`class.mustache`)
**Purpose**: Document classes and their members
**Features**:
- Class overview and inheritance hierarchy
- Constructor documentation
- Property and method listings with details
- Event documentation
- Example usage sections
- Related types and namespaces

### 2. Interface Template (`interface.mustache`)
**Purpose**: Document interfaces and their contracts
**Features**:
- Interface description and purpose
- Method and property signatures
- Generic type parameter documentation
- Implementation notes and guidelines
- Usage examples

### 3. Enum Template (`enum.mustache`)
**Purpose**: Document enumerations and their values
**Features**:
- Enum overview and purpose
- Individual value descriptions
- Usage examples and patterns
- Flag enum documentation (if applicable)

### 4. Namespace Template (`namespace.mustache`)
**Purpose**: Document namespace contents and organization
**Features**:
- Namespace overview
- Type listings by category
- Related namespace references
- Assembly information

### 5. Link Template (`link.mustache`) **[REQUIRED]**
**Purpose**: Format individual cross-reference links
**Features**:
- Platform-specific link formatting
- Internal vs. external link handling
- Anchor link support for combined members

**Example - Basic Markdown** (`templates/basic/link.mustache`):
```mustache
[{{displayName}}]({{relativePath}})
```

**Example - MDX with Components** (`templates/mdx/link.mustache`):
```mustache
<ApiLink href="{{relativePath}}">{{displayName}}</ApiLink>
```

**Example - HTML Output** (`templates/html/link.mustache`):
```mustache
<a href="{{relativePath}}" class="api-link">{{displayName}}</a>
```

**Link Data Structure**:
```json
{
  "uid": "SadConsole.ColoredGlyph",
  "displayName": "ColoredGlyph",
  "relativePath": "../../ColoredGlyph.md",
  "anchor": "foreground",
  "isExternal": false,
  "externalUrl": null
}
```

### 6. Member Template (`member.mustache`)
**Purpose**: Document individual members (when `combineMembers=false`)
**Features**:
- Property documentation
- Method documentation
- Event and field documentation
- Standalone member pages

## Template Variables

### Common Variables
Available across all template types:

```mustache
{{name}}              <!-- Item name -->
{{fullName}}          <!-- Fully qualified name -->
{{uid}}               <!-- Unique identifier -->
{{summary}}           <!-- Summary documentation -->
{{remarks}}           <!-- Detailed remarks -->
{{examples}}          <!-- Code examples -->
{{seeAlso}}           <!-- See also references -->
{{namespace}}         <!-- Containing namespace -->
{{assembly}}          <!-- Assembly information -->
{{isPublic}}          <!-- Public visibility -->
{{isStatic}}          <!-- Static modifier -->
{{isAbstract}}        <!-- Abstract modifier -->
```

### Type-Specific Variables

#### Class Template Variables
```mustache
{{inheritance}}       <!-- Inheritance chain -->
{{implements}}        <!-- Implemented interfaces -->
{{constructors}}      <!-- Constructor list -->
{{properties}}        <!-- Property list -->
{{methods}}           <!-- Method list -->
{{events}}            <!-- Event list -->
{{fields}}            <!-- Field list -->
{{isSealed}}          <!-- Sealed modifier -->
{{genericParameters}} <!-- Generic type parameters -->
```

#### Interface Template Variables
```mustache
{{methods}}           <!-- Interface methods -->
{{properties}}        <!-- Interface properties -->
{{events}}            <!-- Interface events -->
{{inherits}}          <!-- Inherited interfaces -->
{{genericParameters}} <!-- Generic type parameters -->
```

#### Enum Template Variables
```mustache
{{values}}            <!-- Enum values -->
{{underlyingType}}    <!-- Underlying type -->
{{isFlags}}           <!-- Flags attribute -->
```

#### Method Template Variables
```mustache
{{parameters}}        <!-- Parameter list -->
{{returns}}           <!-- Return documentation -->
{{exceptions}}        <!-- Exception documentation -->
{{overloads}}         <!-- Method overloads -->
{{genericParameters}} <!-- Generic parameters -->
{{isVirtual}}         <!-- Virtual modifier -->
{{isOverride}}        <!-- Override modifier -->
```

## Template Structure Examples

### Basic Class Template
```mustache
# {{name}}

{{#summary}}
{{summary}}
{{/summary}}

## Namespace
{{namespace}}

{{#inheritance}}
## Inheritance
{{#items}}
- {{name}}
{{/items}}
{{/inheritance}}

{{#constructors}}
## Constructors

{{#items}}
### {{name}}
{{summary}}

**Parameters:**
{{#parameters}}
- `{{name}}` ({{type}}): {{description}}
{{/parameters}}
{{/items}}
{{/constructors}}

{{#properties}}
## Properties

{{#items}}
### {{name}}
{{summary}}

**Type:** {{type}}
{{#isReadOnly}}*Read-only*{{/isReadOnly}}
{{/items}}
{{/properties}}
```

### Basic Interface Template
```mustache
# {{name}} Interface

{{#summary}}
{{summary}}
{{/summary}}

## Namespace
{{namespace}}

{{#methods}}
## Methods

{{#items}}
### {{name}}
{{summary}}

**Signature:**
```csharp
{{signature}}
```

{{#parameters}}
**Parameters:**
{{#items}}
- `{{name}}` ({{type}}): {{description}}
{{/items}}
{{/parameters}}

{{#returns}}
**Returns:** {{type}} - {{description}}
{{/returns}}
{{/items}}
{{/methods}}
```

## Link Processing in Templates

Templates receive structured link data and can choose rendering approach:

### Markdown Links
```mustache
{{#summary}}
{{text}}{{#links}} [{{displayName}}]({{relativePath}}){{/links}}
{{/summary}}
```

### MDX Component Links
```mustache
{{#summary}}
{{text}}{{#links}} <ApiLink uid="{{uid}}" href="{{relativePath}}">{{displayName}}</ApiLink>{{/links}}
{{/summary}}
```

### Conditional External Links
```mustache
{{#links}}
{{#isExternal}}
<a href="{{externalUrl}}" target="_blank">{{displayName}}</a>
{{/isExternal}}
{{^isExternal}}
[{{displayName}}]({{relativePath}})
{{/isExternal}}
{{/links}}
```

## Template Customization

### Custom Template Directory
```bash
DocFXMustache -i "./api" -o "./docs" -t "./custom-templates" -f md
```

### Template Partials
Templates can include shared partials:
```mustache
{{> header}}

# {{name}}

{{> parameter-list}}

{{> footer}}
```

### Custom Helpers
Register custom Mustache helpers for advanced formatting:
```csharp
templateEngine.RegisterHelper("formatCode", (context, args) => 
{
    var code = args[0].ToString();
    return $"```csharp\n{code}\n```";
});
```

## Output Format Variations

### Markdown (.md) Templates
Focus on standard Markdown syntax:
- Standard link format: `[text](url)`
- Code blocks with language tags
- Standard table formatting

### MDX (.mdx) Templates
Support React components and enhanced features:
- Custom components: `<ApiLink>`, `<CodeBlock>`
- Import statements for components
- JSX syntax within Markdown

## Template Validation

### Required Sections
All templates should include:
- Title/heading with item name
- Summary/description
- Namespace information
- Appropriate member listings

### Optional Sections
- Examples (when available)
- See also references
- Remarks/notes
- Version information

## Best Practices

### Performance
- Keep templates lightweight
- Avoid complex logic in templates
- Use helpers for data transformation

### Maintainability
- Use consistent variable naming
- Document custom helpers
- Organize templates by type
- Use partials for common sections

### Accessibility
- Provide meaningful heading structure
- Include alt text for images
- Use semantic markup
- Ensure proper link context