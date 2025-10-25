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

### Phase 3: Doc Generation Pipeline (100% Complete) ✅
**Goal**: Implement complete documentation generation with template rendering and file output

#### Tasks Completed
- [x] TypeDocumentationService - Convert DocFX YAML Items to TypeDocumentation objects ✅
- [x] Complete Generation Pipeline - End-to-end workflow implemented in Program.cs ✅
- [x] Link processing system ✅
- [x] LinkResolutionService ✅
- [x] XrefProcessingService ✅
- [x] TemplateProcessingService ✅
- [x] Template Configuration System ✅
- [x] Data Models ✅
- [x] Default Templates ✅
- [x] File Generation ✅

#### Deliverables
- [x] Complete doc generation pipeline ✅
- [x] Type conversion service ✅
- [x] Link resolution service ✅
- [x] XRef processing system ✅
- [x] Template engine integration ✅
- [x] Default template set ✅
- [x] File generation and I/O ✅

---

### Phase 4: Advanced Features & Enhancements (25% Complete) ✅
**Goal**: Enhanced functionality and advanced features for production use

#### Tasks Completed
- [x] **XRef Link Resolution Enhancement** ✅ (Oct 24, 2025)
  - [x] Process existing `<xref href="...">` tags into proper markdown links
  - [x] Integrate XRef processing into the generation pipeline
  - [x] Two-pass workflow orchestrator (Pass 1: render templates → Pass 2: resolve XRefs)
  - [x] HTML decoding for YAML-sourced xref tags
  - [x] Template format standardization (href vs uid)

#### Tasks Remaining
- [ ] Link validation and broken reference detection
- [ ] **Multiple Output Formats**
  - [ ] MDX format support with enhanced frontmatter
  - [ ] Custom output format configuration
- [ ] **Advanced File Organization**
  - [ ] Assembly detection and metadata extraction
  - [ ] Namespace-based directory organization
  - [ ] Assembly-namespace hybrid grouping
- [ ] **Index File Generation**
  - [ ] Assembly overview pages
  - [ ] Namespace index files
  - [ ] Table of contents generation
- [ ] **Enhanced Template System**
  - [ ] Advanced Mustache template features
  - [ ] Template inheritance and composition
  - [ ] Custom helper functions
- [ ] **Performance Optimization**
  - [ ] Batch processing for large codebases
  - [ ] Memory optimization for large API sets
  - [ ] Parallel processing support

#### Deliverables
- Enhanced link processing with full resolution
- Multiple output format support
- Advanced file organization strategies
- Index file generation system
- Performance optimizations for large codebases

#### Key Components
- Enhanced `XrefProcessingService` with full pipeline integration
- `IndexGenerationService`
- Advanced template processing features
- Performance optimization framework

---

### Phase 5: Testing & Polish (95% Complete) ✅

#### Tasks Completed
- [x] Core Unit Tests - 124+ tests for all services ✅
- [x] End-to-End Testing ✅
- [x] Real-World Testing ✅
- [x] CLI Integration ✅
- [x] Template Processing Tests ✅

#### Tasks Remaining
- [ ] Performance Testing
- [ ] Documentation Enhancement

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

- ✅ **Week 3 Milestone: Core Generation Complete** ✅
  - Complete end-to-end documentation generation working
  - Generated 14 high-quality markdown files from test fixtures
  - Template rendering with real data working
  - All member types supported

- ✅ **Week 4 Milestone: Enhanced Features** ✅ (Oct 24, 2025)
  - XRef link resolution fully integrated
  - Two-pass processing working end-to-end
  - Template format standardized
  - Cross-namespace linking functional

- **Week 5 Milestone: Production Ready** (Phase 5)
  - Performance optimized for large APIs
  - Complete documentation and examples
  - Ready for production use

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

**✅ CORE FUNCTIONALITY COMPLETE!** Doc generation is working end-to-end!

### What's Working Now
- ✅ Phases 1, 2 & 3 Complete!
- ✅ CLI framework with all options working
- ✅ Complete doc generation pipeline working
- ✅ Generated 14 high-quality markdown files
- ✅ TypeDocumentationService converting YAML Items to documentation
- ✅ Template processing with Mustache rendering
- ✅ File I/O and output generation

###  **Next Steps** (Phase 4 - Optional Enhancements):
- XRef link resolution integration
- MDX output format support
- Advanced grouping strategies  
- Index file generation
- Performance optimization

**Current Status**: **TWO-PASS XREF RESOLUTION COMPLETE** ✅

### 🎯 **AI Collaboration Quick Reference**
**Working on Templates?** → [Template Implementation](docs/implementation/templates.md) + [Core Architecture](docs/architecture/core-architecture.md)  
**Working on Links?** → [Link Processing System](docs/architecture/link-processing.md) + [File Grouping](docs/architecture/file-grouping.md)  
**Adding Tests?** → [Testing Strategy](docs/development/testing-strategy.md) + [Usage Examples](docs/development/usage-examples.md)  
**Need Examples?** → [Usage Examples](docs/development/usage-examples.md) + [Configuration](docs/implementation/configuration.md)

## Contributing

See individual documentation files for detailed information on architecture, implementation, and development processes.

---

*Last updated: October 24, 2025 - **✅ TWO-PASS XREF RESOLUTION COMPLETE!** Phases 1, 2, 3 at 100% + Phase 4 XRef processing complete - Full link resolution working!*