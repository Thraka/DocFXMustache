using System;
using System.Reflection;
using DocFXMustache.Models;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Tests.Helpers;

/// <summary>
/// Factory for creating test Item instances using reflection since Item has private init properties
/// </summary>
public static class ItemTestFactory
{
    /// <summary>
    /// Create a test Item with specified properties
    /// </summary>
    public static Item Create(
        string uid,
        string name,
        string typeString,
        string @namespace,
        string[] assemblies,
        string summary)
    {
        // Create an Item instance using reflection since it has private init properties
        var item = (Item)Activator.CreateInstance(typeof(Item), nonPublic: true)!;

        // Set properties using reflection
        SetProperty(item, nameof(Item.Uid), uid);
        SetProperty(item, nameof(Item.Name), name);
        SetProperty(item, nameof(Item.TypeString), typeString); // Use TypeString instead of Type
        SetProperty(item, nameof(Item.Namespace), @namespace);
        SetProperty(item, nameof(Item.Assemblies), assemblies);
        SetProperty(item, nameof(Item.Summary), summary);

        return item;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
        else
        {
            // If the property has a private setter or init-only setter, use the backing field
            var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
    }
}