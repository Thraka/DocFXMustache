# Reference Resources

## Project-Specific Resources

### Existing Models
**Location**: `.github\reference-files\Models\`

Pre-built data models that can be adapted for this project:
- `Root.cs` - Root YAML document structure
- `Item.cs` - Base API item model
- `TypeDocumentation.cs` - Main documentation model
- `ParameterDocumentation.cs` - Method parameter models
- `ReturnDocumentation.cs` - Return value documentation
- `ExceptionDocumentation.cs` - Exception documentation
- `TypeReferenceDocumentation.cs` - Type reference handling
- `Link.cs` - URL and display text models
- `ItemType.cs` - Enumeration of API item types

### Sample Metadata
**Location**: `.github\reference-files\api\`

Real DocFX metadata files for testing and validation:

#### Class Examples
- `SadConsole.ColoredGlyph.yml` - Standard class with properties and methods
- `SadConsole.Components.Cursor.yml` - Component class example
- `SadConsole.UI.Controls.Button.yml` - UI control class

#### Namespace Examples
- `SadConsole.yml` - Main namespace documentation
- `Microsoft.Xna.Framework.yml` - External framework namespace
- `Microsoft.Xna.Framework.Graphics.yml` - Graphics namespace

#### Specialized Cases
- `Microsoft.Xna.Framework.Graphics.MonoGame_TextureExtensions.yml` - Extension methods
- Interface implementations
- Enum definitions
- Delegate types

## External Documentation

### DocFX Framework
- **Official Documentation**: https://dotnet.github.io/docfx/
- **Metadata Schema**: https://dotnet.github.io/docfx/spec/metadata_format_spec.html
- **YAML Structure**: https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html
- **API Documentation**: https://dotnet.github.io/docfx/api/

### Template Engine Resources

#### Mustache Template System
- **Mustache Specification**: https://mustache.github.io/
- **Mustache Manual**: https://mustache.github.io/mustache.5.html
- **Template Examples**: https://github.com/mustache/spec

#### Stubble.Core Library
- **GitHub Repository**: https://github.com/StubbleOrg/Stubble
- **Documentation**: https://github.com/StubbleOrg/Stubble/wiki
- **API Reference**: https://stubbleorg.github.io/Stubble/
- **Examples**: https://github.com/StubbleOrg/Stubble/tree/master/src/Stubble.Core.Tests

### .NET and C# Resources

#### Command Line Interface
- **System.CommandLine**: https://docs.microsoft.com/en-us/dotnet/standard/commandline/
- **CLI Patterns**: https://docs.microsoft.com/en-us/dotnet/core/tools/
- **Tab Completion**: https://docs.microsoft.com/en-us/dotnet/standard/commandline/tab-completion

#### YAML Processing
- **VYaml Library**: https://github.com/hadashiA/VYaml
- **YAML Specification**: https://yaml.org/spec/1.2/spec.html
- **YAML Best Practices**: https://yaml.org/spec/1.2/spec.html#id2760395

#### Performance Libraries
- **ZString**: https://github.com/Cysharp/ZString
- **String Formatting**: https://docs.microsoft.com/en-us/dotnet/api/system.string.format

## Static Site Generator Integration

### Docusaurus
- **Official Docs**: https://docusaurus.io/
- **MDX Support**: https://docusaurus.io/docs/markdown-features/react
- **API Documentation**: https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-content-docs

### GitBook
- **Documentation**: https://docs.gitbook.com/
- **Markdown Support**: https://docs.gitbook.com/content-creation/editor/markdown
- **API Reference**: https://developer.gitbook.com/

### Jekyll
- **Official Site**: https://jekyllrb.com/
- **Markdown Processing**: https://jekyllrb.com/docs/configuration/markdown/
- **Front Matter**: https://jekyllrb.com/docs/front-matter/

### Next.js
- **Documentation**: https://nextjs.org/docs
- **MDX Integration**: https://nextjs.org/docs/advanced-features/using-mdx
- **Static Generation**: https://nextjs.org/docs/basic-features/data-fetching/get-static-props

## Development Tools and Libraries

### Testing Frameworks
- **MSTest**: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest
- **xUnit**: https://xunit.net/
- **NUnit**: https://nunit.org/

### Mocking and Test Data
- **Moq**: https://github.com/moq/moq4
- **Bogus**: https://github.com/bchavez/Bogus (for generating test data)
- **FluentAssertions**: https://fluentassertions.com/

### Code Quality
- **SonarAnalyzer**: https://rules.sonarsource.com/csharp
- **StyleCop**: https://github.com/DotNetAnalyzers/StyleCopAnalyzers
- **EditorConfig**: https://editorconfig.org/

## Markdown and MDX Resources

### Markdown Specification
- **CommonMark**: https://commonmark.org/
- **GitHub Flavored Markdown**: https://github.github.com/gfm/
- **Markdown Guide**: https://www.markdownguide.org/

### MDX (Markdown + JSX)
- **MDX Specification**: https://mdxjs.com/
- **MDX Components**: https://mdxjs.com/docs/using-mdx/#components
- **React Integration**: https://mdxjs.com/docs/getting-started/

## API Documentation Best Practices

### Documentation Standards
- **Microsoft API Documentation Guidelines**: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/
- **API Documentation Best Practices**: https://swagger.io/resources/articles/best-practices-in-api-documentation/
- **Technical Writing Guide**: https://developers.google.com/tech-writing

### Accessibility
- **WCAG Guidelines**: https://www.w3.org/WAI/WCAG21/quickref/
- **Accessible Documentation**: https://accessibility.18f.gov/content-design/
- **Screen Reader Testing**: https://webaim.org/articles/screenreader_testing/

## Version Control and CI/CD

### Git Workflows
- **GitFlow**: https://nvie.com/posts/a-successful-git-branching-model/
- **GitHub Flow**: https://guides.github.com/introduction/flow/
- **Conventional Commits**: https://www.conventionalcommits.org/

### CI/CD Platforms
- **GitHub Actions**: https://docs.github.com/en/actions
- **Azure DevOps**: https://docs.microsoft.com/en-us/azure/devops/
- **GitLab CI**: https://docs.gitlab.com/ee/ci/

## Performance and Monitoring

### Performance Analysis
- **BenchmarkDotNet**: https://benchmarkdotnet.org/
- **dotMemory**: https://www.jetbrains.com/dotmemory/
- **PerfView**: https://github.com/Microsoft/perfview

### Monitoring and Logging
- **Serilog**: https://serilog.net/
- **NLog**: https://nlog-project.org/
- **Application Insights**: https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview

## Community Resources

### Forums and Discussion
- **Stack Overflow**: https://stackoverflow.com/questions/tagged/docfx
- **Reddit**: https://www.reddit.com/r/dotnet/
- **Discord**: Various .NET community servers

### Blogs and Articles
- **Scott Hanselman's Blog**: https://www.hanselman.com/
- **.NET Blog**: https://devblogs.microsoft.com/dotnet/
- **Code Maze**: https://code-maze.com/

### Open Source Examples
- **DocFX Templates**: https://github.com/dotnet/docfx/tree/dev/templates
- **API Documentation Generators**: Various GitHub repositories
- **Mustache Template Examples**: Community template collections

## Tools and Utilities

### Development Environment
- **Visual Studio**: https://visualstudio.microsoft.com/
- **Visual Studio Code**: https://code.visualstudio.com/
- **JetBrains Rider**: https://www.jetbrains.com/rider/

### Markdown Editors
- **Typora**: https://typora.io/
- **Mark Text**: https://marktext.app/
- **Obsidian**: https://obsidian.md/

### Documentation Tools
- **Pandoc**: https://pandoc.org/ (for format conversion)
- **MkDocs**: https://www.mkdocs.org/
- **Sphinx**: https://www.sphinx-doc.org/

---

*These resources provide comprehensive coverage of technologies, patterns, and best practices relevant to the DocFX Mustache project.*