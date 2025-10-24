using Xunit;
using DocFXMustache.Services;
using DocFXMustache.Models;

namespace DocFXMustache.Tests.Unit.Services;

public class XrefProcessingServiceTests
{
    #region XRef Tag Parsing Tests
    
    [Fact]
    public void ExtractXrefTags_SingleXref_ExtractsCorrectly()
    {
        // Arrange
        var content = @"Based on the <xref href=""SadConsole.Ansi.State.Bold"" data-throw-if-not-resolved=""false""></xref> property.";
        var service = CreateService();
        
        // Act
        var xrefs = service.ExtractXrefTags(content);
        
        // Assert
        Assert.Single(xrefs);
        Assert.Equal("SadConsole.Ansi.State.Bold", xrefs[0]);
    }
    
    [Fact]
    public void ExtractXrefTags_MultipleXrefs_ExtractsAll()
    {
        // Arrange
        var content = @"This uses <xref href=""SadConsole.ColoredGlyph""></xref> and <xref href=""SadConsole.Mirror""></xref> types.";
        var service = CreateService();
        
        // Act
        var xrefs = service.ExtractXrefTags(content);
        
        // Assert
        Assert.Equal(2, xrefs.Count);
        Assert.Contains("SadConsole.ColoredGlyph", xrefs);
        Assert.Contains("SadConsole.Mirror", xrefs);
    }
    
    [Fact]
    public void ExtractXrefTags_NoXrefs_ReturnsEmpty()
    {
        // Arrange
        var content = "This is plain text with no xrefs.";
        var service = CreateService();
        
        // Act
        var xrefs = service.ExtractXrefTags(content);
        
        // Assert
        Assert.Empty(xrefs);
    }
    
    [Fact]
    public void ExtractXrefTags_MixedContent_ExtractsOnlyXrefs()
    {
        // Arrange
        var content = @"See <xref href=""System.String""></xref> for details. Also check https://example.com and <xref href=""System.Int32""></xref>.";
        var service = CreateService();
        
        // Act
        var xrefs = service.ExtractXrefTags(content);
        
        // Assert
        Assert.Equal(2, xrefs.Count);
        Assert.Contains("System.String", xrefs);
        Assert.Contains("System.Int32", xrefs);
    }
    
    #endregion
    
    #region XRef Processing Tests
    
    [Fact]
    public void ProcessXrefs_SingleInternalLink_ReplacesWithMarkdown()
    {
        // Arrange
        var content = @"Based on the <xref href=""SadConsole.ColoredGlyph"" data-throw-if-not-resolved=""false""></xref> class.";
        var currentFile = @"C:\output\SadConsole\ColoredGlyphBase.md";
        var mappings = CreateMappings();
        var service = CreateService(mappings);
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.DoesNotContain("<xref", result);
        Assert.Contains("[ColoredGlyph](ColoredGlyph.md)", result);
    }
    
    [Fact]
    public void ProcessXrefs_InternalLinkWithAnchor_IncludesAnchor()
    {
        // Arrange - linking to a property on a type
        var content = @"Uses the <xref href=""SadConsole.ColoredGlyph.Foreground""></xref> property.";
        var currentFile = @"C:\output\SadConsole\Console.md";
        var mappings = CreateMappingsWithAnchors();
        var service = CreateService(mappings);
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.Contains("[Foreground](ColoredGlyph.md#foreground)", result);
    }
    
    [Fact]
    public void ProcessXrefs_ExternalLink_UsesExternalUrl()
    {
        // Arrange
        var content = @"Returns <xref href=""System.String""></xref> value.";
        var currentFile = @"C:\output\SadConsole\ColoredGlyph.md";
        var mappings = CreateMappingsWithExternalRefs();
        var service = CreateService(mappings);
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.Contains("[String](https://learn.microsoft.com/dotnet/api/system.string)", result);
    }
    
    [Fact]
    public void ProcessXrefs_SameFileAnchor_UsesAnchorOnly()
    {
        // Arrange - linking to a method on the same page
        var content = @"See <xref href=""SadConsole.ColoredGlyph.Clone""></xref> method.";
        var currentFile = @"C:\output\SadConsole\ColoredGlyph.md";
        var mappings = CreateMappingsWithAnchors();
        var service = CreateService(mappings);
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.Contains("[Clone](#clone)", result);
    }
    
    [Fact]
    public void ProcessXrefs_MultipleLinks_ReplacesAll()
    {
        // Arrange
        var content = @"Uses <xref href=""SadConsole.ColoredGlyph""></xref> and <xref href=""SadConsole.Mirror""></xref>.";
        var currentFile = @"C:\output\SadConsole\Console.md";
        var mappings = CreateMappings();
        var service = CreateService(mappings);
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.DoesNotContain("<xref", result);
        Assert.Contains("[ColoredGlyph](ColoredGlyph.md)", result);
        Assert.Contains("[Mirror](Mirror.md)", result);
    }
    
    [Fact]
    public void ProcessXrefs_NoXrefs_ReturnsUnchanged()
    {
        // Arrange
        var content = "This is plain text.";
        var currentFile = @"C:\output\SadConsole\ColoredGlyph.md";
        var service = CreateService();
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.Equal(content, result);
    }
    
    [Fact]
    public void ProcessXrefs_CrossNamespaceLink_UsesCorrectRelativePath()
    {
        // Arrange - linking from UI namespace to root namespace
        var content = @"Inherits from <xref href=""SadConsole.ScreenObject""></xref>.";
        var currentFile = @"C:\output\SadConsole\UI\Controls\Button.md";
        var mappings = CreateMappingsWithNamespaces();
        var service = CreateService(mappings);
        
        // Act
        var result = service.ProcessXrefs(content, currentFile);
        
        // Assert
        Assert.Contains("[ScreenObject](../../ScreenObject.md)", result);
    }
    
    #endregion
    
    #region Display Name Extraction Tests
    
    [Fact]
    public void ExtractDisplayName_TypeUid_ReturnsTypeName()
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var displayName = service.ExtractDisplayName("SadConsole.ColoredGlyph");
        
        // Assert
        Assert.Equal("ColoredGlyph", displayName);
    }
    
    [Fact]
    public void ExtractDisplayName_PropertyUid_ReturnsPropertyName()
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var displayName = service.ExtractDisplayName("SadConsole.ColoredGlyph.Foreground");
        
        // Assert
        Assert.Equal("Foreground", displayName);
    }
    
    [Fact]
    public void ExtractDisplayName_MethodUid_ReturnsMethodName()
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var displayName = service.ExtractDisplayName("SadConsole.ColoredGlyph.Clone");
        
        // Assert
        Assert.Equal("Clone", displayName);
    }
    
    [Fact]
    public void ExtractDisplayName_SystemType_ReturnsShortName()
    {
        // Arrange
        var service = CreateService();
        
        // Act
        var displayName = service.ExtractDisplayName("System.String");
        
        // Assert
        Assert.Equal("String", displayName);
    }
    
    #endregion
    
    #region Template Rendering Tests
    
    [Fact]
    public void RenderLink_InternalLink_RendersMarkdown()
    {
        // Arrange
        var service = CreateServiceWithTemplate();
        var linkInfo = new LinkInfo
        {
            Uid = "SadConsole.ColoredGlyph",
            DisplayName = "ColoredGlyph",
            RelativePath = "ColoredGlyph.md",
            IsExternal = false
        };
        
        // Act
        var result = service.RenderLink(linkInfo);
        
        // Assert
        Assert.Equal("[ColoredGlyph](ColoredGlyph.md)", result);
    }
    
    [Fact]
    public void RenderLink_ExternalLink_RendersMarkdown()
    {
        // Arrange
        var service = CreateServiceWithTemplate();
        var linkInfo = new LinkInfo
        {
            Uid = "System.String",
            DisplayName = "String",
            RelativePath = "https://learn.microsoft.com/dotnet/api/system.string",
            IsExternal = true
        };
        
        // Act
        var result = service.RenderLink(linkInfo);
        
        // Assert
        Assert.Equal("[String](https://learn.microsoft.com/dotnet/api/system.string)", result);
    }
    
    #endregion
    
    #region Helper Methods
    
    private XrefProcessingService CreateService(UidMappings? mappings = null)
    {
        var linkResolver = new LinkResolutionService();
        
        // Populate linkResolver with mappings if provided
        if (mappings != null)
        {
            foreach (var kvp in mappings.UidToFilePath)
            {
                // Check if it's an external URL (starts with http)
                if (kvp.Value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    kvp.Value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // Record as external reference
                    linkResolver.RecordExternalReference(kvp.Key, kvp.Value);
                }
                else
                {
                    // Check if path contains anchor (hash)
                    var parts = kvp.Value.Split('#');
                    var filePath = parts[0];
                    var anchor = parts.Length > 1 ? parts[1] : null;
                    
                    linkResolver.RecordGeneratedFile(kvp.Key, filePath, anchor);
                }
            }
        }
        
        // Use basic template directory - navigate from bin/Debug/net10.0 to templates/basic
        // Test bin: C:\Code\Fun\DocFXMustache\tests\bin\Debug\net10.0
        // Templates: C:\Code\Fun\DocFXMustache\templates\basic
        var templateDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "templates", "basic"));
        return new XrefProcessingService(linkResolver, templateDir);
    }
    
    private XrefProcessingService CreateServiceWithTemplate()
    {
        return CreateService();
    }
    
    private UidMappings CreateMappings()
    {
        var mappings = new UidMappings();
        
        // Same directory links
        mappings.UidToFilePath["SadConsole.ColoredGlyph"] = @"C:\output\SadConsole\ColoredGlyph.md";
        mappings.UidToFilePath["SadConsole.Mirror"] = @"C:\output\SadConsole\Mirror.md";
        mappings.UidToFilePath["SadConsole.Console"] = @"C:\output\SadConsole\Console.md";
        mappings.UidToFilePath["SadConsole.ColoredGlyphBase"] = @"C:\output\SadConsole\ColoredGlyphBase.md";
        
        return mappings;
    }
    
    private UidMappings CreateMappingsWithAnchors()
    {
        var mappings = CreateMappings();
        
        // Members map to parent file with anchors
        mappings.UidToFilePath["SadConsole.ColoredGlyph.Foreground"] = @"C:\output\SadConsole\ColoredGlyph.md#foreground";
        mappings.UidToFilePath["SadConsole.ColoredGlyph.Clone"] = @"C:\output\SadConsole\ColoredGlyph.md#clone";
        
        return mappings;
    }
    
    private UidMappings CreateMappingsWithExternalRefs()
    {
        var mappings = CreateMappings();
        
        // Note: External references are now stored separately via RecordExternalReference
        // These will be added to LinkResolutionService in CreateService helper
        mappings.UidToFilePath["System.String"] = "https://learn.microsoft.com/dotnet/api/system.string";
        mappings.UidToFilePath["System.Int32"] = "https://learn.microsoft.com/dotnet/api/system.int32";
        
        return mappings;
    }
    
    private UidMappings CreateMappingsWithNamespaces()
    {
        var mappings = CreateMappings();
        
        // Cross-namespace references
        mappings.UidToFilePath["SadConsole.ScreenObject"] = @"C:\output\SadConsole\ScreenObject.md";
        mappings.UidToFilePath["SadConsole.UI.Controls.Button"] = @"C:\output\SadConsole\UI\Controls\Button.md";
        
        return mappings;
    }
    
    #endregion
}
