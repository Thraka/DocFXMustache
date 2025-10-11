# Usage Examples

## Basic Usage Scenarios

### Simple Conversion
Convert DocFX metadata to Markdown with default settings:
```bash
DocFXMustache -i "./api" -o "./docs"
```

### Specify Output Format
Generate MDX files instead of Markdown:
```bash
DocFXMustache -i "./api" -o "./docs" -f mdx
```

### Custom Templates
Use custom templates for formatting:
```bash
DocFXMustache -i "./api" -o "./docs" -t "./my-templates"
```

### Preview Changes
Preview what will be generated without writing files:
```bash
DocFXMustache -i "./api" -o "./docs" --dry-run
```

## File Organization Examples

### Flat Structure (Default)
All files in a single directory:
```bash
DocFXMustache -i "./api" -o "./docs" --grouping flat
```

**Result:**
```
docs/
├── SadConsole.ColoredGlyph.md
├── SadConsole.Components.Cursor.md
├── SadConsole.UI.Controls.Button.md
└── Microsoft.Xna.Framework.Graphics.md
```

### Namespace Hierarchy
Organize by namespace structure:
```bash
DocFXMustache -i "./api" -o "./docs" --grouping namespace
```

**Result:**
```
docs/
├── SadConsole/
│   ├── ColoredGlyph.md
│   ├── Components/
│   │   └── Cursor.md
│   └── UI/
│       └── Controls/
│           └── Button.md
└── Microsoft/
    └── Xna/
        └── Framework/
            └── Graphics.md
```

### Assembly + Namespace
Group by assembly first, then namespace:
```bash
DocFXMustache -i "./api" -o "./docs" --grouping assembly-namespace
```

**Result:**
```
docs/
├── SadConsole/
│   └── SadConsole/
│       ├── ColoredGlyph.md
│       ├── Components/
│       │   └── Cursor.md
│       └── UI/
│           └── Controls/
│               └── Button.md
└── Microsoft.Xna.Framework/
    └── Microsoft/
        └── Xna/
            └── Framework/
                └── Graphics.md
```

### Assembly + Flat
Group by assembly with flat type structure:
```bash
DocFXMustache -i "./api" -o "./docs" --grouping assembly-flat
```

**Result:**
```
docs/
├── SadConsole/
│   ├── SadConsole.ColoredGlyph.md
│   ├── SadConsole.Components.Cursor.md
│   └── SadConsole.UI.Controls.Button.md
└── Microsoft.Xna.Framework/
    └── Microsoft.Xna.Framework.Graphics.md
```

## Development Workflow Examples

### Development Preview
Quick preview during development:
```bash
DocFXMustache -i "./api" -o "./docs" --dry-run --verbose
```

### Incremental Updates
Update existing documentation:
```bash
DocFXMustache -i "./api" -o "./docs" --overwrite
```

### Custom Template Testing
Test custom templates:
```bash
DocFXMustache -i "./api" -o "./test-docs" -t "./custom-templates" --dry-run
```

### Link Validation
Validate all generated links:
```bash
DocFXMustache -i "./api" -o "./docs" --validate-links --verbose
```

## Static Site Generator Integration

### Docusaurus (MDX)
Generate MDX files for Docusaurus:
```bash
# Generate MDX files with proper frontmatter
DocFXMustache -i "./api" -o "./docusaurus/docs/api" -f mdx --grouping namespace

# Directory structure for Docusaurus
# docusaurus/docs/api/
# ├── SadConsole/
# │   ├── ColoredGlyph.mdx
# │   └── Components/
# │       └── Cursor.mdx
# └── Microsoft/
#     └── Xna/
#         └── Framework/
#             └── Graphics.mdx
```

### GitBook (Markdown)
Generate Markdown files for GitBook:
```bash
# Generate with GitBook-compatible structure
DocFXMustache -i "./api" -o "./gitbook/api" -f md --grouping flat

# Create SUMMARY.md for GitBook navigation
# (This would be generated automatically in future versions)
```

### Jekyll (Markdown)
Generate Jekyll-compatible files:
```bash
# Generate with Jekyll frontmatter in templates
DocFXMustache -i "./api" -o "./_posts/api" -f md --grouping flat
```

### Next.js (MDX)
Generate for Next.js documentation:
```bash
# Generate MDX files for Next.js pages
DocFXMustache -i "./api" -o "./pages/docs/api" -f mdx --grouping namespace
```

## CI/CD Integration Examples

### GitHub Actions
```yaml
name: Generate API Documentation

on:
  push:
    branches: [main]
    paths: ['docs/api/**']

jobs:
  generate-docs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
          
      - name: Install DocFXMustache
        run: dotnet tool install -g DocFXMustache
        
      - name: Generate Documentation
        run: |
          DocFXMustache -i "./api" -o "./docs/generated" -f mdx \
            --grouping namespace --overwrite --verbose
            
      - name: Commit Generated Docs
        run: |
          git config --local user.email "action@github.com"
          git config --local user.name "GitHub Action"
          git add docs/generated/
          git commit -m "Update generated API documentation" || exit 0
          git push
```

### Azure DevOps
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.0.x'

- script: |
    dotnet tool install -g DocFXMustache
    DocFXMustache -i "$(Build.SourcesDirectory)/api" \
      -o "$(Build.ArtifactStagingDirectory)/docs" \
      -f mdx --grouping assembly-namespace --verbose
  displayName: 'Generate API Documentation'

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: '$(Build.ArtifactStagingDirectory)/docs'
    artifactName: 'api-documentation'
```

## Configuration File Examples

### Basic Configuration (`docfx-mustache.json`)
```json
{
  "input": "./api",
  "output": "./docs",
  "format": "md",
  "grouping": "namespace",
  "overwrite": true,
  "verbose": false
}
```

### Advanced Configuration
```json
{
  "input": "./api",
  "output": "./docs",
  "templates": "./templates",
  "format": "mdx",
  "grouping": "assembly-namespace",
  "overwrite": true,
  "processing": {
    "includePrivateMembers": false,
    "generateIndexFiles": true,
    "validateLinks": true
  },
  "linkProcessing": {
    "resolveExternalLinks": true,
    "brokenLinkHandling": "warn"
  }
}
```

### Development Configuration
```json
{
  "input": "./api",
  "output": "./test-docs",
  "format": "md",
  "grouping": "flat",
  "dryRun": true,
  "verbose": true,
  "processing": {
    "validateLinks": true,
    "generateLinkReport": true
  }
}
```

## Troubleshooting Examples

### Debug Link Issues
```bash
# Generate with link validation and detailed logging
DocFXMustache -i "./api" -o "./docs" --validate-links --verbose

# Generate link report
DocFXMustache -i "./api" -o "./docs" --generate-link-report
```

### Preview File Structure
```bash
# Dry run to see what files would be created
DocFXMustache -i "./api" -o "./docs" --grouping namespace --dry-run --verbose
```

### Test Custom Templates
```bash
# Test templates without writing files
DocFXMustache -i "./api" -o "./docs" -t "./custom-templates" --dry-run
```

### Validate Configuration
```bash
# Test configuration file
DocFXMustache --config "./docfx-mustache.json" --dry-run --verbose
```

## Performance Optimization Examples

### Large API Sets
```bash
# Process large APIs efficiently
DocFXMustache -i "./large-api" -o "./docs" \
  --grouping assembly-flat \
  --no-link-validation \
  --verbose
```

### Parallel Processing
```bash
# Enable parallel processing (future feature)
DocFXMustache -i "./api" -o "./docs" \
  --parallel 4 \
  --memory-limit 2GB
```

## Integration with Build Tools

### MSBuild Integration
```xml
<Target Name="GenerateApiDocs" AfterTargets="Build">
  <Exec Command="DocFXMustache -i &quot;$(OutputPath)api&quot; -o &quot;$(OutputPath)docs&quot; -f mdx" />
</Target>
```

### PowerShell Script
```powershell
# generate-docs.ps1
param(
    [string]$InputPath = "./api",
    [string]$OutputPath = "./docs",
    [string]$Format = "md",
    [string]$Grouping = "namespace"
)

Write-Host "Generating API documentation..."
Write-Host "Input: $InputPath"
Write-Host "Output: $OutputPath"

& DocFXMustache -i $InputPath -o $OutputPath -f $Format --grouping $Grouping --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host "Documentation generated successfully!" -ForegroundColor Green
} else {
    Write-Host "Documentation generation failed!" -ForegroundColor Red
    exit 1
}
```

### Batch Script
```batch
@echo off
setlocal

set INPUT_DIR=./api
set OUTPUT_DIR=./docs
set FORMAT=mdx
set GROUPING=assembly-namespace

echo Generating API documentation...
echo Input: %INPUT_DIR%
echo Output: %OUTPUT_DIR%

DocFXMustache -i "%INPUT_DIR%" -o "%OUTPUT_DIR%" -f %FORMAT% --grouping %GROUPING% --verbose

if %ERRORLEVEL% equ 0 (
    echo Documentation generated successfully!
) else (
    echo Documentation generation failed!
    exit /b 1
)
```