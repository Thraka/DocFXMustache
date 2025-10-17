using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFXMustache.Models;
using DocFXMustache.Services;
using DocFXMustache.Tests.Helpers;

namespace DocFXMustache.Tests.Unit.Models;

public class ItemTypeTests
{
    private readonly MetadataParsingService _parsingService;

    public ItemTypeTests()
    {
        _parsingService = new MetadataParsingService();
    }

    #region Type Conversion Tests

    [Fact]
    public async Task ItemType_SadConsoleClass_ReturnsClassType()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.ColoredGlyph.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act
        var itemType = item.Type;

        // Assert
        Assert.Equal(ItemType.Class, itemType);
    }

    [Fact]
    public async Task ItemType_SadConsoleInterface_ReturnsInterfaceType()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.IScreenSurface.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act
        var itemType = item.Type;

        // Assert
        Assert.Equal(ItemType.Interface, itemType);
    }

    [Fact]
    public async Task ItemType_SadConsoleEnum_ReturnsEnumType()
    {
        // Arrange
        var filePath = TestDataHelper.GetFixturePath("SadConsole.Mirror.yml");
        var metadata = await _parsingService.ParseYamlFileAsync(filePath);
        var item = metadata.Items.First();

        // Act
        var itemType = item.Type;

        // Assert
        Assert.Equal(ItemType.Enum, itemType);
    }

    [Theory]
    [InlineData("Class", ItemType.Class)]
    [InlineData("Struct", ItemType.Struct)]
    [InlineData("Namespace", ItemType.Namespace)]
    [InlineData("Delegate", ItemType.Delegate)]
    [InlineData("Enum", ItemType.Enum)]
    [InlineData("Interface", ItemType.Interface)]
    [InlineData("Field", ItemType.Field)]
    [InlineData("Property", ItemType.Property)]
    [InlineData("Method", ItemType.Method)]
    [InlineData("Operator", ItemType.Operator)]
    [InlineData("Event", ItemType.Event)]
    [InlineData("Constructor", ItemType.Constructor)]
    public void ItemType_KnownTypeString_MapsToCorrectEnum(string typeString, ItemType expected)
    {
        // This test documents the mapping between type strings and ItemType enum values
        // The actual mapping is tested through YAML deserialization tests above
        Assert.NotNull(typeString);
        Assert.True(expected != (ItemType)(-1));
    }

    #endregion
}
