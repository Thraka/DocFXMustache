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

## Implementation Phases

> 📖 **Detailed Roadmap**: [Implementation Phases](docs/implementation/phases.md) with task breakdowns and dependencies

### Phase 1: Core Foundation
- [x] Project setup and CLI framework
- [x] YAML metadata parsing
- [ ] Basic Mustache template processing → [Template Configuration](docs/implementation/templates.md)
- [ ] Simple file output

### Phase 2: Advanced Features
- [x] Multiple file grouping strategies → [File Grouping Details](docs/architecture/file-grouping.md)
- [x] UID discovery and mapping system
- [ ] Link processing system → [Link Processing Architecture](docs/architecture/link-processing.md)
- [ ] Template customization → [Template Implementation](docs/implementation/templates.md)
- [x] Error handling and validation
- [ ] CLI option for filename case control (uppercase/lowercase)

### Phase 3: Polish & Testing
- [ ] Comprehensive testing suite → [Testing Strategy](docs/development/testing-strategy.md)
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

**🚀 Ready to implement?** See [IMPLEMENTATION_NEXT_STEPS.md](IMPLEMENTATION_NEXT_STEPS.md) for step-by-step guidance on what to build next.

**Current Status**: Phase 2 (Advanced Features) is mostly complete! UID discovery, file grouping, and YAML parsing working with 4075+ UIDs. Ready for Phase 3 (Template Engine & Link Processing).

### 🎯 **AI Collaboration Quick Reference**
**Working on Templates?** → [Template Implementation](docs/implementation/templates.md) + [Core Architecture](docs/architecture/core-architecture.md)  
**Working on Links?** → [Link Processing System](docs/architecture/link-processing.md) + [File Grouping](docs/architecture/file-grouping.md)  
**Adding Tests?** → [Testing Strategy](docs/development/testing-strategy.md) + [Usage Examples](docs/development/usage-examples.md)  
**Need Examples?** → [Usage Examples](docs/development/usage-examples.md) + [Configuration](docs/implementation/configuration.md)

## Contributing

See individual documentation files for detailed information on architecture, implementation, and development processes.

---

*Last updated: October 14, 2025 - Phase 2 implementation complete with 4075 UIDs successfully processed*