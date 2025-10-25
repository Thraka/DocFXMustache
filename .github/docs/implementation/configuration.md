# Configuration

## Current Implementation Status

This document describes the configuration options that are currently implemented in DocFX Mustache. The system currently supports command-line configuration with template-specific settings.

**Note**: Configuration file support (`docfx-mustache.json`) and environment variables are planned for future implementation.

## Command Line Interface

### Basic Usage
```bash
DocFXMustache [options]
```

### Available Options

#### Input/Output Options
```bash
-i, --input <path>        Input directory containing DocFX metadata files
-o, --output <path>       Output directory for generated files
-t, --templates <path>    Templates directory
```

#### Format Options
```bash
-f, --format <format>     Output format: md or mdx. If not specified, uses template default.
-g, --grouping <strategy> File grouping strategy: flat, namespace, 
                         assembly-namespace, assembly-flat. If not specified, uses template default.
--case <handling>        Filename case handling: lowercase, uppercase, mixed. 
                         If not specified, uses template default.
```

#### Behavior Options
```bash
--force                  Overwrite existing output files
--dry-run               Preview changes without writing files
-v, --verbose           Enable verbose logging
```

### Usage Examples

#### Basic Generation (using template defaults)
```bash
DocFXMustache -i "./api" -o "./docs" -t "./templates/basic"
# Uses template defaults: md format, flat grouping, lowercase filenames
```

#### Using Starlight Template (with template defaults)
```bash
DocFXMustache -i "./api" -o "./docs" -t "./templates/starlight"
# Uses template defaults: mdx format, namespace grouping, lowercase filenames
```

#### Override Template Defaults
```bash
DocFXMustache -i "./api" -o "./docs" -t "./templates/starlight" --format md --grouping flat
# Overrides: md format, flat grouping (template default: mdx, namespace)
```

#### Custom Templates and Format
```bash
DocFXMustache -i "./api" -o "./docs" -t "./custom-templates" -f mdx
```

#### Namespace Grouping with Preview
```bash
DocFXMustache -i "./api" -o "./docs" -t "./templates" --grouping namespace --dry-run
```

#### Verbose Output
```bash
DocFXMustache -i "./api" -o "./docs" -t "./templates" --verbose
```

## Template Configuration

### Template Directory Structure

Templates are organized in directories with a `template.json` configuration file:

```
templates/
├── basic/
│   ├── template.json
│   ├── class.mustache
│   ├── interface.mustache
│   ├── enum.mustache
│   ├── struct.mustache
│   ├── delegate.mustache
│   ├── member.mustache
│   └── link.mustache
└── starlight/
    ├── template.json
    └── ... (template files)
```

### Template Configuration File (`template.json`)

Each template directory must include a `template.json` file:

```json
{
  "name": "basic",
  "description": "Basic Markdown template for general documentation",
  "version": "1.0.0",
  "outputFormat": "md",
  "fileGrouping": "flat",
  "filenameCase": "lowercase",
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
    "member": "member.mustache",
    "delegate": "delegate.mustache"
  }
}
```

### Template Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `name` | string | `"default"` | Template set name |
| `description` | string | `""` | Template set description |
| `version` | string | `"1.0.0"` | Template version |
| `outputFormat` | string | `"md"` | Default output format: "md" or "mdx" |
| `fileGrouping` | string | `"flat"` | Default file grouping strategy |
| `filenameCase` | string | `"lowercase"` | Default filename case handling |
| `combineMembers` | boolean | `true` | Combine members with parent type vs. separate files |
| `generateIndexFiles` | boolean | `true` | Generate namespace and assembly index files |
| `includeInheritedMembers` | boolean | `false` | Include inherited members in type documentation |
| `templates.class` | string | `"class.mustache"` | Template file for classes |
| `templates.interface` | string | `"interface.mustache"` | Template file for interfaces |
| `templates.enum` | string | `"enum.mustache"` | Template file for enums |
| `templates.struct` | string | `"struct.mustache"` | Template file for structs |
| `templates.delegate` | string | `"delegate.mustache"` | Template file for delegates |
| `templates.link` | string | `"link.mustache"` | **Required**: Template for rendering individual links |
| `templates.member` | string | `"member.mustache"` | Template for individual members (when `combineMembers=false`) |

### Template Organization Modes

**Combined Members Mode (`combineMembers: true`)** - Default
- All members documented on parent type's page with anchor links
- `SadConsole.ColoredGlyph` → `ColoredGlyph.md` (includes all members)
- `SadConsole.ColoredGlyph.Foreground` → `ColoredGlyph.md#foreground` (anchor link)

**Separate Files Mode (`combineMembers: false`)**
- Each member gets its own documentation file
- `SadConsole.ColoredGlyph` → `ColoredGlyph.md`
- `SadConsole.ColoredGlyph.Foreground` → `ColoredGlyph.Foreground.md`

See [Template Documentation](../implementation/templates.md#template-organization-modes) for complete details.

## CLI Option Priority

The command-line interface uses the following priority order for configuration values:

1. **CLI Arguments** (highest priority) - Override everything
2. **Template Configuration** (default values) - Used when CLI arguments not provided
3. **System Defaults** (fallback) - Used if template.json is missing or invalid

### Examples of Priority Resolution

```bash
# Uses template defaults entirely
DocFXMustache -i ./api -o ./docs -t ./templates/starlight
# → outputFormat: "mdx", fileGrouping: "namespace", filenameCase: "lowercase"

# Partial CLI override
DocFXMustache -i ./api -o ./docs -t ./templates/starlight --format md
# → outputFormat: "md" (CLI), fileGrouping: "namespace" (template), filenameCase: "lowercase" (template)

# Full CLI override  
DocFXMustache -i ./api -o ./docs -t ./templates/starlight --format md --grouping flat --case mixed
# → outputFormat: "md" (CLI), fileGrouping: "flat" (CLI), filenameCase: "mixed" (CLI)
```

The output clearly indicates the source of each setting:
- `(from template)` - Value came from template configuration
- `(overridden)` - Value was overridden by CLI argument

## Validation Rules

### Required Settings
- `input`: Must be a valid directory path containing YAML files
- `output`: Must be a writable directory path  
- `templates`: Must be a valid directory path containing template.json

### Format Validation
- `outputFormat` / `--format`: Must be "md" or "mdx"
- `fileGrouping` / `--grouping`: Must be "flat", "namespace", "assembly-namespace", or "assembly-flat"
- `filenameCase` / `--case`: Must be "lowercase", "uppercase", or "mixed"

### Template Validation
- Templates directory must contain `template.json`
- Templates directory must contain `link.mustache` (required)
- Template files specified in `template.json` must exist

## Current Limitations

The following features are documented but not yet implemented:

- **Configuration file support** (`docfx-mustache.json`)
- **Environment variables** 
- **Configuration profiles** (development, production, etc.)
- **Advanced processing options** (include-private, validate-links, etc.)
- **Link processing configuration**
- **File grouping fine-tuning**

These features are planned for future releases as the project moves through Phase 4 (File Generation) and Phase 5 (Testing & Polish).

### Built-in Templates

### Basic Template Set
Location: `templates/basic/`
- **Purpose**: General-purpose Markdown documentation
- **Defaults**: MD format, flat grouping, lowercase filenames
- **Organization**: Combined members mode by default
- **Features**: Clean, readable documentation with proper cross-references

### Starlight Template Set  
Location: `templates/starlight/`
- **Purpose**: Astro Starlight documentation framework
- **Defaults**: MDX format, namespace grouping, lowercase filenames  
- **Organization**: Combined members mode by default
- **Features**: Integration with Starlight's documentation features

Both templates can have their defaults overridden using CLI options (`--format`, `--grouping`, `--case`).