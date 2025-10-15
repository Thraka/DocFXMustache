# Configuration

## Configuration Sources

The application uses a hierarchical configuration system with the following priority order:

1. **Command line arguments** (highest priority)
2. **Configuration file** (docfx-mustache.json)
3. **Environment variables**
4. **Default values** (lowest priority)

## Configuration File Structure

### Default Configuration (`docfx-mustache.json`)
```json
{
  "input": "./api",
  "output": "./docs",
  "templates": "./templates",
  "format": "md",
  "grouping": "flat",
  "overwrite": false,
  "verbose": false,
  "dryRun": false,
  "output": {
    "defaultFormat": "md",
    "defaultGrouping": "flat",
    "fileNamingConvention": "lowercase-hyphen",
    "caseHandling": "lowercase",
    "overwriteExisting": false
  },
  "templates": {
    "defaultTemplateSet": "standard",
    "customHelpers": [],
    "partialDirectories": []
  },
  "processing": {
    "includePrivateMembers": false,
    "includeInternalMembers": false,
    "generateIndexFiles": true,
    "createAssemblyIndexes": true,
    "validateLinks": true
  },
  "fileGrouping": {
    "maxDirectoryDepth": 10,
    "groupingStrategies": ["flat", "namespace", "assembly-namespace", "assembly-flat"],
    "assemblyDetection": {
      "useMetadataAssemblyInfo": true,
      "fallbackToFilename": true
    }
  },
  "linkProcessing": {
    "resolveExternalLinks": true,
    "externalLinkValidation": false,
    "brokenLinkHandling": "warn",
    "generateLinkReport": true
  }
}
```

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
-t, --templates <path>    Templates directory (optional)
```

#### Format Options
```bash
-f, --format <format>     Output format: md (default) or mdx
--grouping <strategy>     File grouping strategy: flat, namespace, 
                         assembly-namespace, assembly-flat
--case <handling>        Filename case handling: lowercase (default), 
                         uppercase, preserve
```

#### Behavior Options
```bash
--overwrite              Overwrite existing output files
--dry-run               Preview changes without writing files
--verbose               Enable verbose logging
--config <path>         Custom configuration file path
```

#### Processing Options
```bash
--include-private       Include private members
--include-internal      Include internal members
--no-index-files        Skip generating index files
--validate-links        Validate all links after generation
```

### Usage Examples

#### Basic Generation
```bash
DocFXMustache -i "./api" -o "./docs"
```

#### Custom Templates and Format
```bash
DocFXMustache -i "./api" -o "./docs" -t "./custom-templates" -f mdx
```

#### Namespace Grouping with Validation
```bash
DocFXMustache -i "./api" -o "./docs" --grouping namespace --validate-links
```

#### Dry Run for Preview
```bash
DocFXMustache -i "./api" -o "./docs" --dry-run --verbose
```

## Configuration Sections

### Output Configuration
Controls how files are generated and organized:

```json
{
  "output": {
    "defaultFormat": "md",              // Default output format
    "defaultGrouping": "flat",          // Default grouping strategy
    "fileNamingConvention": "lowercase-hyphen", // File naming style
    "caseHandling": "lowercase",        // File/directory case: lowercase, uppercase, preserve
    "overwriteExisting": false,         // Overwrite protection
    "createBackups": false,             // Create .bak files before overwrite
    "cleanOutputDirectory": false       // Clean output dir before generation
  }
}
```

### Template Configuration
Controls template processing and customization:

```json
{
  "templates": {
    "defaultTemplateSet": "standard",   // Built-in template set to use
    "customTemplateDirectory": null,    // Override template directory
    "partialDirectories": [             // Additional partial directories
      "./templates/partials"
    ],
    "customHelpers": [                  // Custom helper assemblies
      "./MyHelpers.dll"
    ],
    "templateExtensions": {             // Template file extensions
      "class": ".mustache",
      "interface": ".mustache",
      "enum": ".mustache"
    }
  }
}
```

### Processing Configuration
Controls what content is included and how it's processed:

```json
{
  "processing": {
    "includePrivateMembers": false,     // Include private API members
    "includeInternalMembers": false,    // Include internal API members
    "generateIndexFiles": true,         // Create namespace/assembly indexes
    "createAssemblyIndexes": true,      // Create assembly overview files
    "validateLinks": true,              // Validate link targets
    "processInheritance": true,         // Include inheritance information
    "includeSourceLinks": false,        // Include source code links
    "generateMetadata": true            // Include metadata in output
  }
}
```

### File Grouping Configuration
Controls how output files are organized:

```json
{
  "fileGrouping": {
    "maxDirectoryDepth": 10,            // Maximum nested directory depth
    "groupingStrategies": [             // Available grouping strategies
      "flat", "namespace", "assembly-namespace", "assembly-flat"
    ],
    "assemblyDetection": {
      "useMetadataAssemblyInfo": true,  // Use assembly info from metadata
      "fallbackToFilename": true,       // Fall back to filename for assembly
      "customAssemblyMapping": {}       // Custom UID to assembly mapping
    },
    "namespaceFiltering": {
      "excludePatterns": [],            // Namespace patterns to exclude
      "includeOnlyPatterns": []         // Only include matching patterns
    }
  }
}
```

### Link Processing Configuration
Controls how links are resolved and validated:

```json
{
  "linkProcessing": {
    "resolveExternalLinks": true,       // Resolve external link URLs
    "externalLinkValidation": false,    // Validate external URLs (slow)
    "brokenLinkHandling": "warn",       // How to handle broken links: ignore, warn, error
    "generateLinkReport": true,         // Generate link validation report
    "customLinkResolvers": [],          // Custom link resolution rules
    "externalLinkMappings": {           // Custom external link mappings
      "System.": "https://docs.microsoft.com/dotnet/api/"
    }
  }
}
```

## Environment Variables

Environment variables use the format `DOCFX_MUSTACHE_<SECTION>_<SETTING>`:

```bash
# Basic settings
export DOCFX_MUSTACHE_INPUT="/path/to/api"
export DOCFX_MUSTACHE_OUTPUT="/path/to/docs"
export DOCFX_MUSTACHE_FORMAT="mdx"

# Nested settings
export DOCFX_MUSTACHE_OUTPUT_DEFAULTFORMAT="mdx"
export DOCFX_MUSTACHE_PROCESSING_INCLUDEPRIVATE="true"
export DOCFX_MUSTACHE_FILEGROUPING_MAXDIRECTORYDEPTH="5"
```

## Configuration Validation

### Required Settings
- `input`: Must be a valid directory path
- `output`: Must be a writable directory path

### Optional Settings with Defaults
- `format`: Defaults to "md"
- `grouping`: Defaults to "flat"
- `templates`: Defaults to built-in templates

### Validation Rules
- Input directory must exist and contain YAML files
- Output directory must be writable
- Template directory must exist if specified
- Format must be "md" or "mdx"
- Grouping strategy must be valid

## Configuration Best Practices

### Development Configuration
```json
{
  "verbose": true,
  "dryRun": true,
  "processing": {
    "validateLinks": true,
    "generateLinkReport": true
  }
}
```

### Production Configuration
```json
{
  "overwrite": true,
  "processing": {
    "includePrivateMembers": false,
    "validateLinks": true
  },
  "output": {
    "cleanOutputDirectory": true,
    "createBackups": true
  }
}
```

### CI/CD Configuration
```json
{
  "verbose": false,
  "overwrite": true,
  "linkProcessing": {
    "brokenLinkHandling": "error"
  }
}
```

## Custom Configuration Profiles

### Profile-Based Configuration
Support for different configuration profiles:

```bash
# Use development profile
DocFXMustache --profile development

# Use production profile  
DocFXMustache --profile production
```

### Profile Configuration Files
- `docfx-mustache.development.json`
- `docfx-mustache.production.json`
- `docfx-mustache.ci.json`

### Profile Inheritance
Profiles can inherit from base configuration:

```json
{
  "inherits": "docfx-mustache.json",
  "overwrite": true,
  "verbose": false
}
```