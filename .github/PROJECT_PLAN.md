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
3. **Pass 1 - Template Rendering**: Apply Mustache templates, preserve `<xref>` tags, build UID-to-file mappings
4. **Pass 2 - Link Resolution**: Process `<xref>` tags using actual output paths, render through `link.mustache` → [Link Processing Details](docs/architecture/link-processing.md)
5. **Output**: Write formatted Markdown/MDX files with resolved links

### File Grouping Strategies
> 📖 **Complete Guide**: [File Grouping Strategies](docs/architecture/file-grouping.md) with examples and implementation details

- **Flat**: All files in single directory with fully qualified names
- **Namespace**: Organize by namespace hierarchy
- **Assembly + Namespace**: Group by assembly, then namespace
- **Assembly + Flat**: Group by assembly with flat type structure

## Implementation Status

### Phase 1: Core Foundation (100% Complete) ✅
**Goal**: Establish project structure and basic infrastructure

#### Tasks Completed
- [x] Project setup and CLI framework with System.CommandLine
- [x] YAML metadata parsing with VYaml
- [x] NuGet packages configured (VYaml, ZString, Stubble.Core, System.CommandLine)
- [x] Logging infrastructure (structured logging with console output) ✅ (Oct 24, 2025)
- [x] Set up project structure (Core app under `src/`, tests under `tests/`)
- [x] Implement modern CLI with tab completion
- [x] Configure logging infrastructure

#### Deliverables
- ✅ Project solution with proper structure
- ✅ CLI framework with command definitions
- ✅ Basic data models
- ✅ Logging infrastructure configured
- ✅ Initial unit test project

---

### Phase 2: Metadata Processing & Discovery (100% Complete) ✅
**Goal**: Implement Pass 1 processing for metadata discovery and UID mapping

#### Tasks Completed
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
- [x] YAML parsing for DocFX metadata using reference models
- [x] Metadata-to-model mapping for YAML structure
- [x] Handle different API item types (classes, interfaces, enums, methods, properties)
- [x] Metadata validation and error handling

#### Deliverables
- ✅ Metadata parsing service
- ✅ UID discovery and mapping system
- ✅ File grouping strategy implementations
- ✅ Validation and error handling
- ✅ Pass 1 processing pipeline

#### Key Components
- `MetadataParsingService`
- `DiscoveryService`
- `UidMappings` class
- File organization strategies
- Error handling framework

---

### Phase 3: Link Resolution & Template Engine (60% Complete) 🚧
**Goal**: Implement Pass 2 processing with link resolution and template rendering

**Current Status**: Core link processing implemented and tested

#### Tasks Completed
- [x] **Link processing system** → [Link Processing Architecture](docs/architecture/link-processing.md) ✅ (Oct 24, 2025)
  - **Architecture**: Two-pass process with `link.mustache` template
  - **Pass 1**: TemplateProcessingService renders files with `<xref>` preserved, builds UID mappings
  - **Pass 2**: XrefProcessingService resolves `<xref>` tags using Pass 1 mappings, renders through `link.mustache`
  - Templates control both content structure (Pass 1) and link format (Pass 2)
- [x] **LinkResolutionService** - Record generated files in Pass 1, resolve links in Pass 2 ✅ (Oct 24, 2025)
  - UID-to-path mapping with anchor support
  - Relative path calculation (same dir, parent, deep navigation)
  - External reference detection (System.*, Microsoft.*)
  - External link resolution to Microsoft Docs URLs
  - 19 tests passing
- [x] **XrefProcessingService** - Parse `<xref>` tags, resolve UIDs, render using `link.mustache` (Pass 2) ✅ (Oct 24, 2025)
  - Regex-based xref tag extraction
  - UID resolution using LinkResolutionService
  - Display name extraction from UIDs
  - Stubble.Core integration for template rendering
  - 17 tests passing
- [x] **Data Models** - LinkInfo, OutputFileInfo ✅ (Oct 24, 2025)
- [x] **link.mustache template** - Basic markdown link format ✅ (Oct 24, 2025)

#### Tasks Remaining
- [ ] Template customization → [Template Implementation](docs/implementation/templates.md)
- [ ] TemplateProcessingService - Integrate Stubble.Core Mustache rendering (Pass 1)
- [ ] FileGenerationService - Handle file I/O and directory creation
- [ ] DocumentationGenerator - Orchestrate two-pass workflow (Pass 1: render templates → Pass 2: resolve XRefs)
- [ ] Default templates for each API item type (class, interface, enum, method)
- [ ] Integrate Stubble.Core Mustache engine for main templates
- [ ] Create default templates for each API item type
- [ ] Implement template resolution logic
- [ ] Add structured link data helpers for templates

#### Deliverables
- [x] Link resolution service ✅
- [x] XRef processing system ✅
- [ ] Template engine integration
- [ ] Default template set
- [ ] Two-pass workflow orchestrator

#### Key Components
- [x] `XrefProcessingService` ✅
- [ ] `TemplateEngine` wrapper
- [x] `LinkResolutionService` ✅
- [ ] `DocumentationGenerator`
- [x] Default link.mustache template ✅
- [ ] Default content templates (class, interface, enum, method)

---

### Phase 4: File Generation & Output (0% Complete)
**Goal**: Complete the generation pipeline with file output and validation

#### Tasks
- [ ] Implement two-pass generation process
- [ ] Handle .md and .mdx output formats with proper link rendering
- [ ] Implement all file grouping strategies (flat, namespace, assembly-namespace, assembly-flat)
- [ ] Create assembly detection logic from metadata
- [ ] Implement file naming conventions and path generation
- [ ] Add overwrite protection and dry-run mode
- [ ] Implement link validation after Pass 1 discovery
- [ ] Generate cross-reference reports and broken link detection
- [ ] Generate index files for assemblies and namespaces
- [ ] .md and .mdx output format support
- [ ] Link validation and broken reference detection
- [ ] Index file generation for assemblies and namespaces
- [ ] Dry-run and overwrite protection
- [ ] Error handling for missing UIDs in Pass 2

#### Deliverables
- Complete file generation pipeline
- All grouping strategies implemented
- Output format support (MD/MDX)
- Link validation and reporting
- Index file generation
- Dry-run and overwrite protection

#### Key Components
- `FileGenerationService`
- `FileGroupingService`
- `AssemblyDetectionService`
- `LinkValidationService`
- Index generators

---

### Phase 5: Testing & Polish (30% Complete) 🚧
**Goal**: Ensure quality, performance, and usability

#### Tasks Completed
- [x] Unit tests for core services and models (107 tests total) ✅ (Oct 24, 2025)
  - 19 LinkResolutionService tests
  - 17 XrefProcessingService tests
  - 59 existing tests (models, discovery, parsing, logging)
- [x] **Link resolution tests** using real YAML fixtures ✅ (Oct 24, 2025)

#### Tasks Remaining
- [ ] Comprehensive testing suite → [Testing Strategy](docs/development/testing-strategy.md)
  - [ ] Integration tests for end-to-end workflows
  - [ ] Performance tests for large documentation sets
  - [ ] Template processing tests
  - [ ] Grouping strategy integration tests
- [ ] Documentation and examples → [Usage Examples](docs/development/usage-examples.md)
- [ ] Performance optimization
- [ ] User experience improvements
- [ ] Comprehensive unit tests
- [ ] Integration tests with sample DocFX metadata
- [ ] Performance optimization
- [ ] Documentation and usage examples
- [ ] Error message improvements
- [ ] CLI help and validation enhancements

#### Deliverables
- Complete test suite with high coverage
- Performance benchmarks and optimizations
- User documentation and examples
- Polished CLI experience
- Error handling and user feedback

#### Quality Metrics
- **Test Coverage**: >90% code coverage
- **Performance**: Process 1000+ API items in <10 seconds
- **Error Handling**: Graceful degradation and helpful error messages
- **Documentation**: Complete API documentation and usage examples

---

### Development Milestones

- ✅ **Week 1 Milestone: Foundation Complete**
  - Project builds successfully
  - CLI accepts basic parameters
  - Basic models can parse simple YAML
  - Logging works correctly

- ✅ **Week 2 Milestone: Discovery Working**
  - Can parse all sample metadata files
  - UID mapping system functional
  - File grouping strategies implemented
  - Error handling for malformed input

- 🚧 **Week 3 Milestone: Templates Rendering** (60% Complete)
  - ✅ XRef links resolve correctly
  - [ ] Templates render with real data
  - [ ] Two-pass workflow operational
  - [ ] Basic output files generated

- **Week 4 Milestone: Feature Complete**
  - All grouping strategies working
  - Link validation functional
  - Index files generated
  - Both MD and MDX output supported

- **Week 5 Milestone: Production Ready**
  - Full test coverage
  - Performance optimized
  - Documentation complete
  - Ready for release

---

### Risk Mitigation

#### Technical Risks
- **Complex link resolution**: ✅ Mitigated - Started with simple cases, added complexity incrementally
- **Template system integration**: Use proven Stubble.Core library
- **Performance with large APIs**: Implement streaming and lazy evaluation early

#### Dependency Risks
- **DocFX metadata changes**: Version lock dependencies, add validation
- **Library compatibility**: Pin to stable versions, have fallback plans

#### Schedule Risks
- **Scope creep**: Keep MVP focused, defer advanced features
- **Testing complexity**: Prioritize core functionality testing
- **Documentation time**: Write docs incrementally during development

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

**🚀 What's Next?** Phases 1, 2, and core link processing complete! Ready to finish Phase 3:

1. **✅ Phase 1 & 2 Complete!** (October 24, 2025)
   - ✅ CLI framework with all options working
   - ✅ `--case` CLI option for filename casing control
   - ✅ UID discovery validated with 4,075 UIDs from reference files
   - ✅ Logging infrastructure fully integrated

2. **✅ Phase 3 - Link Processing Complete!** (October 24, 2025)
   - ✅ LinkResolutionService with UID mapping and path resolution
   - ✅ XrefProcessingService with regex parsing and template rendering
   - ✅ Data models (LinkInfo, OutputFileInfo)
   - ✅ link.mustache template for markdown links
   - ✅ 107 tests passing (added 36 link processing tests)

3. **Phase 3 - Remaining Tasks** (**NEXT** 🎯):
   - TemplateProcessingService for Pass 1 (render templates with xrefs preserved)
   - FileGenerationService for file I/O
   - DocumentationGenerator to orchestrate two-pass workflow
   - Default templates for classes, interfaces, enums, methods
   - Integration with Program.cs

**Current Status**: Phases 1 & 2 at 100%, Phase 3 at 60% complete! ✅ Link processing core implemented with 107 tests passing. Ready to build template rendering and file generation.

### 🎯 **AI Collaboration Quick Reference**
**Working on Templates?** → [Template Implementation](docs/implementation/templates.md) + [Core Architecture](docs/architecture/core-architecture.md)  
**Working on Links?** → [Link Processing System](docs/architecture/link-processing.md) + [File Grouping](docs/architecture/file-grouping.md)  
**Adding Tests?** → [Testing Strategy](docs/development/testing-strategy.md) + [Usage Examples](docs/development/usage-examples.md)  
**Need Examples?** → [Usage Examples](docs/development/usage-examples.md) + [Configuration](docs/implementation/configuration.md)

## Contributing

See individual documentation files for detailed information on architecture, implementation, and development processes.

---

*Last updated: October 24, 2025 - **Phases 1 & 2 at 100%, Phase 3 at 60% complete** ✅ (Link processing services implemented: LinkResolutionService + XrefProcessingService with 107 tests passing)*