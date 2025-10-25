using System;
using System.Collections.Generic;
using System.Linq;
using DocFXMustache.Models;
using DocFXMustache.Models.Yaml;
using Microsoft.Extensions.Logging;
using YamlTypeParameter = DocFXMustache.Models.Yaml.TypeParameter;

namespace DocFXMustache.Services;

/// <summary>
/// Service responsible for converting DocFX YAML Items to TypeDocumentation objects.
/// This bridges the gap between the raw YAML data and the template-ready documentation models.
/// </summary>
public class TypeDocumentationService
{
    private readonly ILogger<TypeDocumentationService> _logger;

    public TypeDocumentationService(ILogger<TypeDocumentationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Converts a DocFX YAML Item to a TypeDocumentation object.
    /// </summary>
    /// <param name="item">The YAML item to convert</param>
    /// <param name="references">The reference collection for resolving links</param>
    /// <param name="allItems">All items in the current root for finding children</param>
    /// <param name="outputFormat">Output file format (md or mdx)</param>
    /// <returns>A TypeDocumentation object ready for template processing</returns>
    public TypeDocumentation ConvertItem(Item item, Reference[] references, IList<Item> allItems, string outputFormat = "md")
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (string.IsNullOrEmpty(item.Uid)) throw new ArgumentException("Item must have a UID", nameof(item));

        _logger.LogDebug("Converting item {Uid} ({Type}) to TypeDocumentation", item.Uid, item.Type);

        // Create the main TypeDocumentation object
        var typeDoc = new TypeDocumentation(
            uid: item.Uid,
            name: item.Name ?? ExtractNameFromUid(item.Uid),
            fullName: item.FullName ?? item.Uid,
            type: item.Type,
            summary: FormatSummary(item.Summary),
            link: CreateLink(item.Uid, outputFormat),
            syntax: ExtractSyntax(item.Syntax),
            remarks: FormatSummary(item.Remarks),
            inheritance: GetReferencesArray(item.Inheritance, references),
            implements: GetReferencesArray(item.Implements, references),
            parameters: GetParameters(item, references),
            returns: GetReturn(item, references),
            typeParameters: GetTypeParameters(item, references),
            exceptions: GetExceptions(item, references),
            attributes: item.Attributes ?? Array.Empty<AttributeDoc>()
        );

        // Find and add child members
        FindAndAddChildren(typeDoc, allItems, references, outputFormat);

        _logger.LogDebug("Converted {Uid} with {ChildCount} children", 
            item.Uid, 
            typeDoc.Constructors.Count + typeDoc.Fields.Count + typeDoc.Properties.Count + 
            typeDoc.Methods.Count + typeDoc.Events.Count);

        return typeDoc;
    }

    /// <summary>
    /// Finds and adds child members to a TypeDocumentation object.
    /// </summary>
    private void FindAndAddChildren(TypeDocumentation parent, IList<Item> allItems, Reference[] references, string outputFormat = "md")
    {
        foreach (var item in allItems)
        {
            // Skip if the item is not a child of this type
            if (item.Parent != parent.Uid)
                continue;

            var child = ConvertItem(item, references, allItems, outputFormat);

            switch (child.Type)
            {
                case ItemType.Constructor:
                    parent.AddConstructor(child);
                    break;
                case ItemType.Field:
                    parent.AddField(child);
                    break;
                case ItemType.Property:
                    parent.AddProperty(child);
                    break;
                case ItemType.Method:
                case ItemType.Operator:
                    parent.AddMethod(child);
                    break;
                case ItemType.Event:
                    parent.AddEvent(child);
                    break;
                default:
                    _logger.LogDebug("Skipping child item {Uid} with type {Type}", item.Uid, item.Type);
                    break;
            }
        }
    }

    /// <summary>
    /// Converts an array of UID strings to TypeReferenceDocumentation array.
    /// </summary>
    private TypeReferenceDocumentation[] GetReferencesArray(string[]? original, Reference[] references)
    {
        if (original == null || original.Length == 0)
            return Array.Empty<TypeReferenceDocumentation>();

        var result = new TypeReferenceDocumentation[original.Length];
        for (int i = 0; i < original.Length; i++)
        {
            var uid = original[i];
            var name = uid;
            var link = Link.Empty;

            // Try to find the reference for a better name and link
            var reference = references.FirstOrDefault(r => r.Uid == uid);
            if (reference.Uid != null)
            {
                name = reference.Name ?? uid;
                link = new Link(IsExternalReference(reference.Href), reference.Href ?? string.Empty);
            }

            result[i] = new TypeReferenceDocumentation(uid, name, link);
        }

        return result;
    }

    /// <summary>
    /// Extracts parameter documentation from an item.
    /// </summary>
    private ParameterDocumentation[] GetParameters(Item item, Reference[] references)
    {
        if (item.Syntax?.Parameters == null || item.Syntax.Parameters.Length == 0)
            return Array.Empty<ParameterDocumentation>();

        var result = new ParameterDocumentation[item.Syntax.Parameters.Length];
        for (int i = 0; i < item.Syntax.Parameters.Length; i++)
        {
            var param = item.Syntax.Parameters[i];
            var type = CreateTypeReference(param.Type, references);
            result[i] = new ParameterDocumentation(param.Id, type, FormatSummary(param.Description));
        }

        return result;
    }

    /// <summary>
    /// Extracts return documentation from an item.
    /// </summary>
    private ReturnDocumentation? GetReturn(Item item, Reference[] references)
    {
        if (item.Syntax?.Returns == null)
            return null;

        var returns = item.Syntax.Returns.Value;
        var type = CreateTypeReference(returns.Type, references);
        return new ReturnDocumentation(type, FormatSummary(returns.Description));
    }

    /// <summary>
    /// Extracts type parameters from an item.
    /// </summary>
    private Models.TypeParameter[] GetTypeParameters(Item item, Reference[] references)
    {
        if (item.Syntax?.TypeParameters == null || item.Syntax.TypeParameters.Length == 0)
            return Array.Empty<Models.TypeParameter>();

        var result = new Models.TypeParameter[item.Syntax.TypeParameters.Length];
        for (int i = 0; i < item.Syntax.TypeParameters.Length; i++)
        {
            var typeParam = item.Syntax.TypeParameters[i];
            result[i] = new Models.TypeParameter(typeParam.Id, FormatSummary(typeParam.Description));
        }

        return result;
    }

    /// <summary>
    /// Extracts exception documentation from an item.
    /// </summary>
    private ExceptionDocumentation[] GetExceptions(Item item, Reference[] references)
    {
        if (item.Exceptions == null || item.Exceptions.Length == 0)
            return Array.Empty<ExceptionDocumentation>();

        var result = new ExceptionDocumentation[item.Exceptions.Length];
        for (int i = 0; i < item.Exceptions.Length; i++)
        {
            var exception = item.Exceptions[i];
            var type = CreateTypeReference(exception.Type, references);
            result[i] = new ExceptionDocumentation(type, FormatSummary(exception.Description));
        }

        return result;
    }

    /// <summary>
    /// Creates a TypeReferenceDocumentation for a given type string.
    /// </summary>
    private TypeReferenceDocumentation CreateTypeReference(string typeString, Reference[] references)
    {
        if (string.IsNullOrEmpty(typeString))
            return new TypeReferenceDocumentation(string.Empty, string.Empty, Link.Empty);

        var name = typeString;
        var link = Link.Empty;

        // Try to find the reference for a better name and link
        var reference = references.FirstOrDefault(r => r.Uid == typeString);
        if (reference.Uid != null)
        {
            name = reference.Name ?? typeString;
            link = new Link(IsExternalReference(reference.Href), reference.Href ?? string.Empty);
        }

        return new TypeReferenceDocumentation(typeString, name, link);
    }

    /// <summary>
    /// Creates a Link object for a UID (used for the main type's link).
    /// </summary>
    private Link CreateLink(string uid, string outputFormat = "md")
    {
        // For now, create internal links based on the UID
        // This will be enhanced with proper link resolution later
        var fileName = GetSafeFileName(uid) + $".{outputFormat}";
        return new Link(false, fileName);
    }

    /// <summary>
    /// Determines if a reference href is external.
    /// </summary>
    private bool IsExternalReference(string? href)
    {
        if (string.IsNullOrEmpty(href))
            return false;

        // External if it starts with http:// or https://
        return href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               href.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts syntax content from SyntaxContent.
    /// </summary>
    private string? ExtractSyntax(SyntaxContent? syntax)
    {
        return syntax?.Content?.Trim();
    }

    /// <summary>
    /// Formats summary text (placeholder for now).
    /// </summary>
    private string? FormatSummary(string? summary)
    {
        // TODO: This could be enhanced with proper markdown formatting
        return summary?.Trim();
    }

    /// <summary>
    /// Extracts a simple name from a UID.
    /// </summary>
    private string ExtractNameFromUid(string uid)
    {
        var lastDot = uid.LastIndexOf('.');
        return lastDot >= 0 && lastDot < uid.Length - 1 
            ? uid.Substring(lastDot + 1) 
            : uid;
    }

    /// <summary>
    /// Creates a safe filename from a UID.
    /// </summary>
    private string GetSafeFileName(string name)
    {
        // Remove or replace characters that are invalid in file names
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        var result = name;
        
        foreach (var c in invalidChars)
        {
            result = result.Replace(c, '-');
        }

        // Also handle some common problematic characters
        result = result.Replace('<', '-')
                      .Replace('>', '-')
                      .Replace('`', '-')
                      .Replace('(', '-')
                      .Replace(')', '-');

        return result.ToLowerInvariant();
    }
}