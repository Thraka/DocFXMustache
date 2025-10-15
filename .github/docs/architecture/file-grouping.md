# File Grouping Strategies

A key consideration for large API documentation sets is how to organize the output files. The tool supports multiple file grouping strategies to accommodate different documentation needs and site structures.

## Available Strategies

### 1. Flat Structure (`flat`)
**Default behavior** - All files in a single output directory using fully qualified names:
```
output/
├── SadConsole.ColoredGlyph.md
├── SadConsole.Components.Cursor.md
├── SadConsole.UI.Controls.Button.md
├── Microsoft.Xna.Framework.Graphics.yml.md
└── Hexa.NET.ImGui.SC.yml.md
```

### 2. Namespace Hierarchy (`namespace`)
Organize files by namespace structure:
```
output/
├── SadConsole/
│   ├── ColoredGlyph.md
│   ├── Components/
│   │   └── Cursor.md
│   └── UI/
│       └── Controls/
│           └── Button.md
├── Microsoft/
│   └── Xna/
│       └── Framework/
│           └── Graphics.md
└── Hexa/
    └── NET/
        └── ImGui/
            └── SC.md
```

### 3. Assembly + Namespace (`assembly-namespace`)
Group by assembly first, then namespace:
```
output/
├── SadConsole/                    # Assembly name
│   ├── SadConsole/               # Namespace
│   │   ├── ColoredGlyph.md
│   │   ├── Components/
│   │   │   └── Cursor.md
│   │   └── UI/
│   │       └── Controls/
│   │           └── Button.md
├── Microsoft.Xna.Framework/       # Assembly name
│   └── Microsoft/
│       └── Xna/
│           └── Framework/
│               └── Graphics.md
└── Hexa.NET.ImGui/               # Assembly name
    └── Hexa/
        └── NET/
            └── ImGui/
                └── SC.md
```

### 4. Assembly + Flat (`assembly-flat`)
Group by assembly, but keep types flat within each assembly:
```
output/
├── SadConsole/
│   ├── SadConsole.ColoredGlyph.md
│   ├── SadConsole.Components.Cursor.md
│   └── SadConsole.UI.Controls.Button.md
├── Microsoft.Xna.Framework/
│   └── Microsoft.Xna.Framework.Graphics.md
└── Hexa.NET.ImGui/
    └── Hexa.NET.ImGui.SC.md
```

## Configuration Options

```bash
# Use namespace hierarchy
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping namespace

# Use assembly + namespace structure  
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping assembly-namespace

# Default flat structure
DocFXMustache -i "./api" -o "./docs" -t "./templates" -f md --grouping flat
```

## Benefits by Strategy

| Strategy | Pros | Cons | Best For |
|----------|------|------|----------|
| **Flat** | Simple, searchable, no deep paths | Can become cluttered | Small APIs, search-driven sites |
| **Namespace** | Logical grouping, mirrors code | Can create deep paths | Large APIs, browsable documentation |
| **Assembly-Namespace** | Clear assembly boundaries | Most complex structure | Multi-assembly projects |
| **Assembly-Flat** | Assembly separation, simple paths | Less logical grouping | Multi-assembly, moderate size |

## Implementation Considerations

- **Path Length**: Consider file system path limitations on different platforms
- **URL Structure**: How the grouping affects website URLs and navigation
- **Link Resolution**: How relative links work within each strategy
- **Build Performance**: Impact on static site generator build times
- **SEO**: How URL structure affects search engine optimization

## File Naming Conventions

### Default Behavior
- **Lowercase naming**: All file and directory names are converted to lowercase by default for consistency
- **Directory names**: Namespace dots (`.`) are converted to hyphens (`-`) and made lowercase (e.g., `System.Collections` → `system-collections`)
- **File names**: Type names are converted to lowercase with invalid characters replaced by hyphens (e.g., `FileListBox` → `filelistbox.md`)

### Flat Strategy
- Use fully qualified type names in lowercase
- Replace invalid file system characters with hyphens
- Example: `System.Collections.Generic.List<T>` → `system-collections-generic-list-t-.md`

### Hierarchical Strategies
- Use simple type names in lowercase within namespace folders
- Create index files for namespaces when needed
- Handle special characters in namespace names
- Example: `Microsoft.Extensions.DependencyInjection` namespace becomes `microsoft-extensions-dependencyinjection/` directory

### Planned Enhancement
A CLI option for controlling filename case (uppercase/lowercase) will be added in Phase 2 of development, allowing users to override the default lowercase behavior when needed.

## Future Enhancements

- Custom grouping strategies via configuration
- Hybrid approaches (e.g., flat within assemblies, hierarchical between)
- Template-driven grouping rules
- Grouping by custom metadata attributes