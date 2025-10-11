# Default Templates

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

### 5. Method Template (`method.mustache`)
**Purpose**: Document individual methods (when generated separately)
**Features**:
- Method signature and parameters
- Return value documentation
- Exception documentation
- Usage examples

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