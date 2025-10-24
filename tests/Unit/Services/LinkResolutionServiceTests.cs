using Xunit;
using DocFXMustache.Services;
using DocFXMustache.Models;

namespace DocFXMustache.Tests.Unit.Services;

public class LinkResolutionServiceTests
{
    #region UID Recording Tests (Pass 1)
    
    [Fact]
    public void RecordGeneratedFile_TypeWithoutAnchor_RecordsCorrectly()
    {
        // Arrange
        var service = CreateService();
        var uid = "SadConsole.ColoredGlyph";
        var filePath = @"C:\output\SadConsole\ColoredGlyph.md";
        
        // Act
        service.RecordGeneratedFile(uid, filePath);
        
        // Assert
        var info = service.GetOutputInfo(uid);
        Assert.Equal(filePath, info.FilePath);
        Assert.Null(info.Anchor);
    }
    
    [Fact]
    public void RecordGeneratedFile_MemberWithAnchor_RecordsCorrectly()
    {
        // Arrange
        var service = CreateService();
        var uid = "SadConsole.ColoredGlyph.Foreground";
        var filePath = @"C:\output\SadConsole\ColoredGlyph.md";
        var anchor = "foreground";
        
        // Act
        service.RecordGeneratedFile(uid, filePath, anchor);
        
        // Assert
        var info = service.GetOutputInfo(uid);
        Assert.Equal(filePath, info.FilePath);
        Assert.Equal(anchor, info.Anchor);
    }
    
    [Fact]
    public void RecordGeneratedFile_MultipleUids_AllRecorded()
    {
        // Arrange
        var service = CreateService();
        
        // Act - Record a type and its members
        service.RecordGeneratedFile("SadConsole.ColoredGlyph", @"C:\output\SadConsole\ColoredGlyph.md");
        service.RecordGeneratedFile("SadConsole.ColoredGlyph.Foreground", @"C:\output\SadConsole\ColoredGlyph.md", "foreground");
        service.RecordGeneratedFile("SadConsole.ColoredGlyph.Background", @"C:\output\SadConsole\ColoredGlyph.md", "background");
        service.RecordGeneratedFile("SadConsole.ColoredGlyph.Clone", @"C:\output\SadConsole\ColoredGlyph.md", "clone");
        
        // Assert
        Assert.NotNull(service.GetOutputInfo("SadConsole.ColoredGlyph"));
        Assert.NotNull(service.GetOutputInfo("SadConsole.ColoredGlyph.Foreground"));
        Assert.NotNull(service.GetOutputInfo("SadConsole.ColoredGlyph.Background"));
        Assert.NotNull(service.GetOutputInfo("SadConsole.ColoredGlyph.Clone"));
    }
    
    #endregion
    
    #region Relative Path Calculation Tests
    
    [Fact]
    public void CalculateRelativePath_SameDirectory_ReturnsFileName()
    {
        // Arrange
        var service = CreateService();
        var fromPath = @"C:\output\SadConsole\ColoredGlyph.md";
        var toPath = @"C:\output\SadConsole\Mirror.md";
        
        // Act
        var result = service.CalculateRelativePath(fromPath, toPath);
        
        // Assert
        Assert.Equal("Mirror.md", result);
    }
    
    [Fact]
    public void CalculateRelativePath_ParentDirectory_UsesDoubleDot()
    {
        // Arrange
        var service = CreateService();
        var fromPath = @"C:\output\SadConsole\UI\Button.md";
        var toPath = @"C:\output\SadConsole\ColoredGlyph.md";
        
        // Act
        var result = service.CalculateRelativePath(fromPath, toPath);
        
        // Assert
        Assert.Equal("../ColoredGlyph.md", result);
    }
    
    [Fact]
    public void CalculateRelativePath_DeepNavigation_UsesMultipleDoubleDots()
    {
        // Arrange
        var service = CreateService();
        var fromPath = @"C:\output\SadConsole\UI\Controls\Button.md";
        var toPath = @"C:\output\SadConsole\ColoredGlyph.md";
        
        // Act
        var result = service.CalculateRelativePath(fromPath, toPath);
        
        // Assert
        Assert.Equal("../../ColoredGlyph.md", result);
    }
    
    [Fact]
    public void CalculateRelativePath_DifferentBranch_NavigatesCorrectly()
    {
        // Arrange
        var service = CreateService();
        var fromPath = @"C:\output\SadConsole\UI\Controls\Button.md";
        var toPath = @"C:\output\SadConsole\Entities\Entity.md";
        
        // Act
        var result = service.CalculateRelativePath(fromPath, toPath);
        
        // Assert
        Assert.Equal("../../Entities/Entity.md", result);
    }
    
    [Fact]
    public void CalculateRelativePath_SameFile_ReturnsEmpty()
    {
        // Arrange
        var service = CreateService();
        var path = @"C:\output\SadConsole\ColoredGlyph.md";
        
        // Act
        var result = service.CalculateRelativePath(path, path);
        
        // Assert
        Assert.Equal("", result);
    }
    
    #endregion
    
    #region Link Resolution Tests (Pass 2)
    
    [Fact]
    public void ResolveInternalLink_SameDirectory_ReturnsCorrectPath()
    {
        // Arrange
        var service = CreateService();
        service.RecordGeneratedFile("SadConsole.ColoredGlyph", @"C:\output\SadConsole\ColoredGlyph.md");
        service.RecordGeneratedFile("SadConsole.Mirror", @"C:\output\SadConsole\Mirror.md");
        
        var currentFile = @"C:\output\SadConsole\ColoredGlyph.md";
        var targetUid = "SadConsole.Mirror";
        
        // Act
        var result = service.ResolveInternalLink(currentFile, targetUid);
        
        // Assert
        Assert.Equal("Mirror.md", result);
    }
    
    [Fact]
    public void ResolveInternalLink_WithAnchor_IncludesAnchor()
    {
        // Arrange
        var service = CreateService();
        service.RecordGeneratedFile("SadConsole.ColoredGlyph", @"C:\output\SadConsole\ColoredGlyph.md");
        service.RecordGeneratedFile("SadConsole.ColoredGlyph.Foreground", @"C:\output\SadConsole\ColoredGlyph.md", "foreground");
        
        var currentFile = @"C:\output\SadConsole\Console.md";
        var targetUid = "SadConsole.ColoredGlyph.Foreground";
        
        // Act
        var result = service.ResolveInternalLink(currentFile, targetUid);
        
        // Assert
        Assert.Equal("ColoredGlyph.md#foreground", result);
    }
    
    [Fact]
    public void ResolveInternalLink_SameFileAnchor_ReturnsAnchorOnly()
    {
        // Arrange
        var service = CreateService();
        service.RecordGeneratedFile("SadConsole.ColoredGlyph", @"C:\output\SadConsole\ColoredGlyph.md");
        service.RecordGeneratedFile("SadConsole.ColoredGlyph.Clone", @"C:\output\SadConsole\ColoredGlyph.md", "clone");
        
        var currentFile = @"C:\output\SadConsole\ColoredGlyph.md";
        var targetUid = "SadConsole.ColoredGlyph.Clone";
        
        // Act
        var result = service.ResolveInternalLink(currentFile, targetUid);
        
        // Assert
        Assert.Equal("#clone", result);
    }
    
    [Fact]
    public void ResolveInternalLink_CrossNamespace_UsesCorrectRelativePath()
    {
        // Arrange
        var service = CreateService();
        service.RecordGeneratedFile("SadConsole.ScreenObject", @"C:\output\SadConsole\ScreenObject.md");
        service.RecordGeneratedFile("SadConsole.UI.Controls.Button", @"C:\output\SadConsole\UI\Controls\Button.md");
        
        var currentFile = @"C:\output\SadConsole\UI\Controls\Button.md";
        var targetUid = "SadConsole.ScreenObject";
        
        // Act
        var result = service.ResolveInternalLink(currentFile, targetUid);
        
        // Assert
        Assert.Equal("../../ScreenObject.md", result);
    }
    
    #endregion
    
    #region External Reference Tests
    
    [Fact]
    public void RecordExternalReference_WithAbsoluteUrl_Stored()
    {
        // Arrange
        var service = CreateService();
        var uid = "System.String";
        var href = "https://learn.microsoft.com/dotnet/api/system.string";
        
        // Act
        service.RecordExternalReference(uid, href);
        
        // Assert
        Assert.True(service.IsExternalReference(uid));
        Assert.Equal(href, service.ResolveExternalLink(uid, null));
    }
    
    [Fact]
    public void RecordExternalReference_WithRelativeHref_Stored()
    {
        // Arrange
        var service = CreateService();
        var uid = "SadConsole.ICellSurface";
        var relativeHref = "SadConsole.ICellSurface.html";
        
        // Act
        service.RecordExternalReference(uid, relativeHref);
        
        // Assert - Stored and treated as external (since we didn't generate a file for it)
        Assert.True(service.IsExternalReference(uid));
        Assert.Equal(relativeHref, service.ResolveExternalLink(uid, null));
    }
    
    [Fact]
    public void IsExternalReference_GeneratedFile_ReturnsFalse()
    {
        // Arrange - If we generated a file for a UID, it's internal
        var service = CreateService();
        var uid = "SadConsole.ColoredGlyph";
        
        // Record in YAML references (might have been there)
        service.RecordExternalReference(uid, "SadConsole.ColoredGlyph.html");
        
        // But we generated a file for it in Pass 1
        service.RecordGeneratedFile(uid, @"C:\output\SadConsole\ColoredGlyph.md");
        
        // Act & Assert - Generated file takes precedence, so it's internal
        Assert.False(service.IsExternalReference(uid));
    }
    
    [Fact]
    public void IsExternalReference_OnlyInReferences_ReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        service.RecordExternalReference("System.Collections.Generic.IEnumerable`1", "https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1");
        
        // Act & Assert - In references but not generated, so external
        Assert.True(service.IsExternalReference("System.Collections.Generic.IEnumerable`1"));
    }
    
    [Fact]
    public void IsExternalReference_NotRecorded_ReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        
        // Act & Assert - Unknown UIDs are treated as external
        Assert.True(service.IsExternalReference("Unknown.Type"));
        Assert.True(service.IsExternalReference("System.String"));
        Assert.True(service.IsExternalReference("Microsoft.Extensions.Logging.ILogger"));
    }
    
    [Fact]
    public void IsExternalReference_OnlyGenerated_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        service.RecordGeneratedFile("SadConsole.ColoredGlyph", @"C:\output\SadConsole\ColoredGlyph.md");
        
        // Act & Assert - Generated file means it's internal
        Assert.False(service.IsExternalReference("SadConsole.ColoredGlyph"));
    }
    
    [Fact]
    public void ResolveExternalLink_WithRecordedHref_UsesHref()
    {
        // Arrange
        var service = CreateService();
        var uid = "System.String";
        var expectedHref = "https://learn.microsoft.com/dotnet/api/system.string";
        service.RecordExternalReference(uid, expectedHref);
        
        // Act
        var result = service.ResolveExternalLink(uid, null);
        
        // Assert
        Assert.Equal(expectedHref, result);
    }
    
    [Fact]
    public void ResolveExternalLink_SystemTypeNoRecord_GeneratesUrl()
    {
        // Arrange
        var service = CreateService();
        var uid = "System.String";
        
        // Act
        var result = service.ResolveExternalLink(uid, null);
        
        // Assert
        Assert.StartsWith("https://learn.microsoft.com/dotnet/api/", result);
        Assert.Contains("system.string", result.ToLowerInvariant());
    }
    
    [Fact]
    public void ResolveExternalLink_WithFallback_UsesFallback()
    {
        // Arrange
        var service = CreateService();
        var uid = "UnknownExternal.Type";
        var fallback = "https://example.com/docs";
        
        // Act
        var result = service.ResolveExternalLink(uid, fallback);
        
        // Assert
        Assert.Equal(fallback, result);
    }
    
    [Fact]
    public void ResolveExternalLink_RelativeHrefFromYaml_PreservesRelativePath()
    {
        // Arrange - YAML references can have relative paths for cross-repo docs
        var service = CreateService();
        var uid = "SadConsole.ICellSurface";
        var relativeHref = "SadConsole.ICellSurface.html";
        service.RecordExternalReference(uid, relativeHref);
        
        // Act
        var result = service.ResolveExternalLink(uid, null);
        
        // Assert - Relative path preserved (might be used for cross-linking)
        Assert.Equal(relativeHref, result);
    }
    
    #endregion
    
    #region Error Handling Tests
    
    [Fact]
    public void GetOutputInfo_UnknownUid_ThrowsException()
    {
        // Arrange
        var service = CreateService();
        var unknownUid = "SadConsole.UnknownType";
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.GetOutputInfo(unknownUid));
    }
    
    [Fact]
    public void ResolveInternalLink_UnknownUid_ThrowsException()
    {
        // Arrange
        var service = CreateService();
        var currentFile = @"C:\output\SadConsole\ColoredGlyph.md";
        var unknownUid = "SadConsole.UnknownType";
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => service.ResolveInternalLink(currentFile, unknownUid));
    }
    
    #endregion
    
    #region Helper Methods
    
    private LinkResolutionService CreateService()
    {
        return new LinkResolutionService();
    }
    
    #endregion
}
