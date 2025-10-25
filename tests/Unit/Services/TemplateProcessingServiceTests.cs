using System;
using System.IO;
using DocFXMustache.Models;
using DocFXMustache.Services;
using DocFXMustache.Tests.Helpers;
using Xunit;

namespace DocFXMustache.Tests.Unit.Services;

public class TemplateProcessingServiceTests
{
    private readonly string _testTemplateDir;

    public TemplateProcessingServiceTests()
    {
        // Use the actual basic template directory
        _testTemplateDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "templates", "basic"
        );
        _testTemplateDir = Path.GetFullPath(_testTemplateDir);
    }

    [Fact]
    public void Constructor_LoadsTemplateConfiguration()
    {
        // Arrange & Act
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        // Assert
        Assert.NotNull(service.Configuration);
        Assert.Equal("basic", service.Configuration.Name);
        Assert.True(service.Configuration.CombineMembers);
        Assert.True(service.Configuration.GenerateIndexFiles);
    }

    [Fact]
    public void Constructor_ThrowsOnNullDirectory()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TemplateProcessingService(null!, logger));
    }

    [Fact]
    public void Constructor_ThrowsOnNullLogger()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TemplateProcessingService(_testTemplateDir, null!));
    }

    [Fact]
    public void Constructor_UsesDefaultConfigWhenTemplateJsonMissing()
    {
        // Arrange - Create temp directory without template.json
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a minimal template file
            File.WriteAllText(Path.Combine(tempDir, "class.mustache"), "# {{name}}");

            var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();

            // Act
            var service = new TemplateProcessingService(tempDir, logger);

            // Assert
            Assert.NotNull(service.Configuration);
            Assert.Equal("default", service.Configuration.Name); // Default value
            Assert.True(service.Configuration.CombineMembers); // Default value
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RenderType_RendersClassTemplateSuccessfully()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        var typeDoc = new TypeDocumentation(
            uid: "TestNamespace.TestClass",
            name: "TestClass",
            fullName: "TestNamespace.TestClass",
            type: ItemType.Class,
            summary: "A test class for template rendering",
            link: new Link(false, "TestClass.md")
        );

        // Act
        var result = service.RenderType(typeDoc);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# TestClass", result);
        Assert.Contains("A test class for template rendering", result);
        Assert.Contains("**Namespace:** TestNamespace", result);
        Assert.Contains("**Type:** class", result);
    }

    [Fact]
    public void RenderType_PreservesXrefTags()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        var inheritance = new[]
        {
            new TypeReferenceDocumentation(
                uid: "System.Object",
                name: "Object",
                link: new Link(true, "https://docs.microsoft.com/dotnet/api/system.object"))
        };

        var typeDoc = new TypeDocumentation(
            uid: "TestNamespace.TestClass",
            name: "TestClass",
            fullName: "TestNamespace.TestClass",
            type: ItemType.Class,
            summary: "A test class",
            link: new Link(false, "TestClass.md"),
            inheritance: inheritance
        );

        // Act
        var result = service.RenderType(typeDoc);

        // Assert
        Assert.Contains("<xref uid=\"System.Object\">", result);
    }

    [Fact]
    public void RenderType_HandlesTypeWithMembers()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        var typeDoc = new TypeDocumentation(
            uid: "TestNamespace.TestClass",
            name: "TestClass",
            fullName: "TestNamespace.TestClass",
            type: ItemType.Class,
            summary: "A test class with members",
            link: new Link(false, "TestClass.md")
        );

        // Add a property
        var property = new TypeDocumentation(
            uid: "TestNamespace.TestClass.MyProperty",
            name: "MyProperty",
            fullName: "TestNamespace.TestClass.MyProperty",
            type: ItemType.Property,
            summary: "A test property",
            link: new Link(false, "TestClass.md#myproperty")
        );
        typeDoc.AddProperty(property);

        // Act
        var result = service.RenderType(typeDoc);

        // Assert
        Assert.Contains("## Properties", result);
        Assert.Contains("MyProperty", result);
        Assert.Contains("A test property", result);
    }

    [Fact]
    public void RenderType_ThrowsOnNullType()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.RenderType(null!));
    }

    [Fact]
    public void RenderMember_RendersMemberTemplate()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        var member = new TypeDocumentation(
            uid: "TestNamespace.TestClass.MyMethod",
            name: "MyMethod",
            fullName: "TestNamespace.TestClass.MyMethod",
            type: ItemType.Method,
            summary: "A test method",
            link: new Link(false, "TestClass.MyMethod.md")
        );

        // Act
        var result = service.RenderMember(member);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# MyMethod", result);
        Assert.Contains("A test method", result);
        Assert.Contains("**Namespace:** TestNamespace.TestClass", result);
    }

    [Fact]
    public void RenderMember_ThrowsOnNullMember()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.RenderMember(null!));
    }

    [Fact]
    public void RenderType_HandlesInterfaceType()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        var typeDoc = new TypeDocumentation(
            uid: "TestNamespace.ITestInterface",
            name: "ITestInterface",
            fullName: "TestNamespace.ITestInterface",
            type: ItemType.Interface,
            summary: "A test interface",
            link: new Link(false, "ITestInterface.md")
        );

        // Act
        var result = service.RenderType(typeDoc);

        // Assert
        Assert.Contains("# ITestInterface", result);
        Assert.Contains("A test interface", result);
        Assert.Contains("**Type:** Interface", result);
    }

    [Fact]
    public void RenderType_HandlesEnumType()
    {
        // Arrange
        var logger = LoggerHelper.CreateNullLogger<TemplateProcessingService>();
        var service = new TemplateProcessingService(_testTemplateDir, logger);

        var typeDoc = new TypeDocumentation(
            uid: "TestNamespace.TestEnum",
            name: "TestEnum",
            fullName: "TestNamespace.TestEnum",
            type: ItemType.Enum,
            summary: "A test enumeration",
            link: new Link(false, "TestEnum.md")
        );

        // Add enum values as fields
        typeDoc.AddField(new TypeDocumentation(
            uid: "TestNamespace.TestEnum.Value1",
            name: "Value1",
            fullName: "TestNamespace.TestEnum.Value1",
            type: ItemType.Field,
            summary: "First value",
            link: new Link(false, "TestEnum.md#value1")
        ));

        // Act
        var result = service.RenderType(typeDoc);

        // Assert
        Assert.Contains("# TestEnum", result);
        Assert.Contains("A test enumeration", result);
        Assert.Contains("**Type:** Enum", result);
        Assert.Contains("## Values", result);
        Assert.Contains("Value1", result);
    }
}
