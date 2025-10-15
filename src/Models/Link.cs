using System;
using DocFXMustache.Models.Yaml;

namespace DocFXMustache.Models;

public readonly record struct Link(bool IsExternalLink, string Href)
{
    public bool IsEmpty
    {
        get { return this == Empty || this == default; }
    }

    public static Link Empty
    {
        get { return new Link(false, string.Empty); }
    }

    public static Link FromReference(in Reference reference)
    {
        // For now, simplified logic - we'll enhance this in Phase 3
        bool isExternalLink = reference.Href?.StartsWith("http") ?? false;
        string href = reference.Href ?? string.Empty;
        if (!isExternalLink)
        {
            href = href.ToLowerInvariant();
        }
        return new Link(isExternalLink, href);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToString("../");
    }

    public string ToString(string baseLocalPath)
    {
        if (IsExternalLink)
        {
            return Href;
        }

        return $"{baseLocalPath}{Href}";
    }
}