# DocFX Mustache - Project Plan

## Project Overview

**DocFX Mustache** is a command-line tool designed to transform DocFX-generated .NET API metadata files into Markdown (.md) or MDX (.mdx) files using Mustache templating.

## Goal

Transform DocFX generated .NET API metadata files (YAML format) into customizable Markdown or MDX files suitable for static site generators, documentation platforms, or other publishing systems.

## Key Features

- **Multiple Output Formats**: Support for Markdown (.md) and MDX (.mdx)
- **Flexible File Organization**: Multiple grouping strategies (flat, namespace, assembly-based)
- **Intelligent Link Processing**: UID-based link resolution with context-aware path generation
- **Customizable Templates**: Mustache-based templating system
- **Comprehensive CLI**: Intuitive command-line interface with validation

## Core Architecture

> 📖 **Detailed Architecture**: See [Core Architecture](docs/architecture/core-architecture.md) for system design and components

### File Processing Pipeline
1. **Input**: DocFX YAML metadata files
2. **Parse**: Load and validate metadata structure
3. **Transform**: Apply Mustache templates with context data
4. **Link Resolution**: Process internal references and generate correct paths → [Link Processing Details](docs/architecture/link-processing.md)
5. **Output**: Write formatted Markdown/MDX files

### File Grouping Strategies
> 📖 **Complete Guide**: [File Grouping Strategies](docs/architecture/file-grouping.md) with examples and implementation details

- **Flat**: All files in single directory with fully qualified names
- **Namespace**: Organize by namespace hierarchy
- **Assembly + Namespace**: Group by assembly, then namespace
- **Assembly + Flat**: Group by assembly with flat type structure

## Implementation Status

> 📖 **Detailed Roadmap**: [Implementation Phases](docs/implementation/phases.md) with task breakdowns and dependencies

### Phase 1: Core Foundation (100% Complete) ✅
- [x] Project setup and CLI framework with System.CommandLine
- [x] YAML metadata parsing with VYaml
- [x] NuGet packages configured (VYaml, ZString, Stubble.Core, System.CommandLine)
- [x] Logging infrastructure (structured logging with console output) ✅ (Oct 24, 2025)
- [ ] Reference model validation against `.github\reference-files\Models\` *(deferred to Phase 5)*

### Phase 2: Metadata Processing & Discovery (100% Complete) ✅
- [x] Multiple file grouping strategies → [File Grouping Details](docs/architecture/file-grouping.md)
- [x] UID discovery and mapping system (4,075+ UIDs discovered)
- [x] Error handling and validation
- [x] YAML parsing and metadata models
- [x] CLI option for filename case control (uppercase/lowercase/mixed)
- [x] **Testing with reference files from `.github\reference-files\api\`** ✅ (Oct 24, 2025)
  - All 431 YAML files parsed successfully
  - 4,075 UIDs mapped correctly
  - All grouping strategies validated
- [x] **Logging infrastructure** ✅ (Oct 24, 2025)
  - LoggerFactory service created
  - Integrated with MetadataParsingService and DiscoveryService
  - Respects `--verbose` flag (Debug vs Information level)
  - 12/12 logging tests passing

### Phase 3: Link Resolution & Template Engine (0% Complete)
**Next Priority**: Implement Pass 2 generation pipeline
- [ ] Link processing system → [Link Processing Architecture](docs/architecture/link-processing.md)
  - **Approach**: Use dedicated `link.mustache` template for rendering individual links
  - XrefProcessingService resolves UIDs, creates `LinkInfo` objects, renders through `link.mustache`
  - Rendered links injected back into content strings
  - Templates control link format without code changes
- [ ] Template customization → [Template Implementation](docs/implementation/templates.md)
- [ ] XrefProcessingService - Parse `<xref>` tags, resolve UIDs, render using `link.mustache`
- [ ] TemplateProcessingService - Integrate Stubble.Core Mustache rendering
- [ ] DocumentationGenerator - Orchestrate two-pass workflow
- [ ] FileGenerationService - Handle file I/O and directory creation
- [ ] Default templates for each API item type (class, interface, enum, method) + `link.mustache`

### Phase 4: File Generation & Output (0% Complete)
- [ ] Two-pass generation process orchestration
- [ ] .md and .mdx output format support
- [ ] Link validation and broken reference detection
- [ ] Index file generation for assemblies and namespaces
- [ ] Dry-run and overwrite protection

### Phase 5: Testing & Polish (0% Complete)
- [x] Unit tests for core services and models (59 tests)
- [ ] Comprehensive testing suite → [Testing Strategy](docs/development/testing-strategy.md)
  - [ ] Integration tests for end-to-end workflows
  - [ ] Performance tests for large documentation sets
  - [ ] Template processing tests
  - [ ] Link resolution tests
- [ ] Documentation and examples → [Usage Examples](docs/development/usage-examples.md)
- [ ] Performance optimization
- [ ] User experience improvements

## Quick Start

> 📖 **More Examples**: [Usage Examples](docs/development/usage-examples.md) for detailed CLI scenarios

```bash
# Basic usage
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md

# With custom grouping
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping namespace

# Dry run to preview structure
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --dry-run
```

> ⚙️ **Configuration Options**: See [Configuration Guide](docs/implementation/configuration.md) for all CLI parameters

## Documentation Structure

This project plan is organized into focused documents for better maintainability:

### Architecture Documents
- [File Grouping Strategies](docs/architecture/file-grouping.md) - Detailed grouping options and examples
- [Link Processing System](docs/architecture/link-processing.md) - UID-based link resolution
- [Core Architecture](docs/architecture/core-architecture.md) - System design and components

### Implementation Documents
- [Implementation Phases](docs/implementation/phases.md) - Detailed development roadmap
- [Default Templates](docs/implementation/templates.md) - Template structure and examples
- [Configuration](docs/implementation/configuration.md) - Configuration options and settings

### Development Documents
- [Testing Strategy](docs/development/testing-strategy.md) - Testing approach and test data
- [Usage Examples](docs/development/usage-examples.md) - CLI usage scenarios

### Reference Documents
- [Resources](docs/reference/resources.md) - External resources and documentation

## Success Criteria

1. **Functionality**: Successfully transform DocFX metadata into readable Markdown/MDX
2. **Flexibility**: Support custom templates and multiple output formats
3. **Usability**: Intuitive command-line interface with helpful error messages
4. **Performance**: Process large API documentation sets efficiently
5. **Maintainability**: Clean, well-documented, testable code

## Quick Start for LLM Implementation

**🚀 What's Next?** Phase 1 & 2 are 100% complete! Ready to start Phase 3 implementation:

1. **✅ Phase 1 & 2 Complete!** (October 24, 2025)
   - ✅ CLI framework with all options working
   - ✅ `--case` CLI option for filename casing control
   - ✅ UID discovery validated with 4,075 UIDs from reference files
   - ✅ Logging infrastructure fully integrated
   - ✅ All 71 tests passing (59 unit tests + 12 logging tests)

2. **Phase 3 Implementation** (**READY TO START NOW** 🎯):
   - XrefProcessingService → TemplateProcessingService → DocumentationGenerator
   - FileGenerationService for robust output
   - Default Mustache templates
   - Integration with Program.cs

**Current Status**: Phases 1 & 2 at 100% complete! ✅ All prerequisites met. UID discovery working with 4,075+ UIDs mapped. Logging infrastructure operational. Ready for Phase 3: Template Engine & Link Resolution.

### 🎯 **AI Collaboration Quick Reference**
**Working on Templates?** → [Template Implementation](docs/implementation/templates.md) + [Core Architecture](docs/architecture/core-architecture.md)  
**Working on Links?** → [Link Processing System](docs/architecture/link-processing.md) + [File Grouping](docs/architecture/file-grouping.md)  
**Adding Tests?** → [Testing Strategy](docs/development/testing-strategy.md) + [Usage Examples](docs/development/usage-examples.md)  
**Need Examples?** → [Usage Examples](docs/development/usage-examples.md) + [Configuration](docs/implementation/configuration.md)

## Contributing

See individual documentation files for detailed information on architecture, implementation, and development processes.

---

*Last updated: October 24, 2025 - **Phase 1 & 2 at 100% complete** ✅ (Logging infrastructure, reference file testing with 4,075 UIDs validated, all 71 tests passing), **Phase 3 ready to begin***