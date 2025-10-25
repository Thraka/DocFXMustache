using System;
using System.Reflection;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Tests.Unit.Services;

/// <summary>
/// Factory for creating test Item instances using reflection to bypass private init setters
/// </summary>
public static class ItemTestFactory
{
    public static Item Create(string uid, string name, string typeString, string @namespace, string[] assemblies, string summary)
    {
        // Create an instance using reflection
        var item = (Item)Activator.CreateInstance(typeof(Item), nonPublic: true)!;
        
        // Set properties using reflection
        SetPrivateProperty(item, nameof(Item.Uid), uid);
        SetPrivateProperty(item, nameof(Item.Name), name);
        SetPrivateProperty(item, "TypeString", typeString);
        SetPrivateProperty(item, nameof(Item.Namespace), @namespace);
        SetPrivateProperty(item, nameof(Item.Assemblies), assemblies);
        SetPrivateProperty(item, nameof(Item.Summary), summary);
        
        return item;
    }
    
    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.SetMethod != null)
        {
            // For properties with private setters
            property.SetValue(obj, value);
        }
        else if (property != null)
        {
            // For properties with init-only setters, we need to use backing fields
            var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            backingField?.SetValue(obj, value);
        }
    }
}