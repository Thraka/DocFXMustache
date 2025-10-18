# File Grouping Strategies

A key consideration for large API documentation sets is how to organize the output files. The tool supports multiple file grouping strategies to accommodate different documentation needs and site structures.

## Available Strategies

### 1. Flat Structure (`flat`)
**Default behavior** - All files in a single output directory using fully qualified names (namespace.type):
```
output/
├── sadconsole.coloredglyph.md
├── sadconsole.components.cursor.md
├── sadconsole.ui.controls.button.md
├── microsoft.xna.framework.graphics.md
└── system.collections.generic.list-1.md
```

**Naming Convention**: `<namespace>.<type>[-N].md` where N is the generic parameter count (following [Microsoft Learn pattern](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1))

**Benefits for SEO and Discoverability**:
- Dots are preserved to maintain namespace hierarchy semantically in URLs
- Lowercase for consistency and canonicalization
- Generic parameters indicated with `-#` suffix (e.g., `-1`, `-2`)
- Example: `System.Collections.Generic.Dictionary-2.md` is similar to Microsoft's `system.collections.generic.dictionary-2`

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
| **Flat** | Simple URLs, SEO-friendly (preserves namespace hierarchy), follows Microsoft Learn pattern | Can become cluttered with many types | Small to medium APIs, search-driven sites, SEO optimization |
| **Namespace** | Logical grouping, mirrors code structure | Can create deep paths, harder to reference | Large APIs, browsable documentation, intuitive navigation |
| **Assembly-Namespace** | Clear assembly boundaries, organized hierarchy | Most complex structure | Multi-assembly projects, enterprise APIs |
| **Assembly-Flat** | Assembly separation, simple paths within assembly | Less logical grouping | Multi-assembly, moderate size APIs |

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
- **File names**: Type names are converted to lowercase with invalid characters replaced by hyphens

### Flat Strategy
- Use **fully qualified type names** (namespace.type) in lowercase
- Preserve dots (`.`) to maintain namespace hierarchy semantically
- Generic parameters indicated with `-N` suffix (where N = parameter count)
- Example: `System.Collections.Generic.List<T>` → `system.collections.generic.list-1.md`
- This follows the [Microsoft Learn URL pattern](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)

### Hierarchical Strategies
- Use simple type names in lowercase within namespace folders
- Create index files for namespaces when needed
- Handle special characters in namespace names by replacing with hyphens
- Example: namespace `Microsoft.Extensions.DependencyInjection` becomes directory `microsoft-extensions-dependencyinjection/`

### Character Handling

All strategies follow these rules:
1. **Lowercase conversion**: All names converted to lowercase for consistency
2. **Dot handling**: 
   - Preserved in flat strategies to maintain namespace semantics
   - Converted to hyphens in directory names
3. **Generic parameters**: `<T>`, `<TKey, TValue>`, etc. are converted using the backtick notation replacement
   - `List<T>` has 1 parameter → suffix `-1`
   - `Dictionary<TKey, TValue>` has 2 parameters → suffix `-2`
4. **Invalid characters**: Replaced with hyphens (`<`, `>`, `|`, `:`, `#`, `(`, `)`, etc.)

### Planned Enhancement
A CLI option for controlling filename case (uppercase/lowercase) will be added in Phase 2 of development, allowing users to override the default lowercase behavior when needed.

## Future Enhancements

- Custom grouping strategies via configuration
- Hybrid approaches (e.g., flat within assemblies, hierarchical between)
- Template-driven grouping rules
- Grouping by custom metadata attributes