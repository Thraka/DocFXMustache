using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DocFXMustache.Models;
using Stubble.Core;
using Stubble.Core.Builders;

namespace DocFXMustache.Services;

/// <summary>
/// Service for processing xref tags in generated markdown content (Pass 2).
/// Resolves UIDs using Pass 1 mappings and renders links using Mustache templates.
/// </summary>
public partial class XrefProcessingService
{
    private readonly LinkResolutionService _linkResolver;
    private readonly string _linkTemplate;
    
    // Regex to match xref tags: <xref href="UID" ...>Display Name</xref>
    [GeneratedRegex(@"<xref\s+href=""([^""]+)""[^>]*>([^<]*)</xref>", RegexOptions.IgnoreCase)]
    private static partial Regex XrefPattern();
    
    public XrefProcessingService(LinkResolutionService linkResolver, string templateDirectory)
    {
        _linkResolver = linkResolver ?? throw new ArgumentNullException(nameof(linkResolver));
        
        // Load link.mustache template
        var linkTemplatePath = Path.Combine(templateDirectory, "link.mustache");
        if (!File.Exists(linkTemplatePath))
        {
            throw new FileNotFoundException($"link.mustache template not found at: {linkTemplatePath}");
        }
        
        _linkTemplate = File.ReadAllText(linkTemplatePath);
    }
    
    #region XRef Tag Extraction
    
    /// <summary>
    /// Extracts all UIDs from xref tags in content.
    /// </summary>
    /// <param name="content">The content to parse</param>
    /// <returns>List of UIDs found in xref tags</returns>
    public List<string> ExtractXrefTags(string content)
    {
        var uids = new List<string>();
        var matches = XrefPattern().Matches(content);
        
        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                uids.Add(match.Groups[1].Value);
            }
        }
        
        return uids;
    }
    
    #endregion
    
    #region XRef Processing
    
    /// <summary>
    /// Processes all xref tags in content, replacing them with rendered links (Pass 2).
    /// </summary>
    /// <param name="content">The content with xref tags</param>
    /// <param name="currentFilePath">The current file path (for relative path calculation)</param>
    /// <returns>Content with xrefs replaced by links</returns>
    public string ProcessXrefs(string content, string currentFilePath)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }
        
        // First, decode HTML entities in the content to handle xref tags from YAML
        content = WebUtility.HtmlDecode(content);
        
        return XrefPattern().Replace(content, match =>
        {
            if (!match.Success || match.Groups.Count < 3)
            {
                return match.Value; // Keep original if malformed
            }
            
            var uid = match.Groups[1].Value;
            var displayName = match.Groups[2].Value;
            
            try
            {
                // Create LinkInfo and render through template
                var linkInfo = CreateLinkInfo(uid, currentFilePath);
                // Use display name from xref tag if available, otherwise extract from UID
                if (!string.IsNullOrEmpty(displayName))
                {
                    linkInfo.DisplayName = displayName;
                }
                return RenderLink(linkInfo);
            }
            catch (KeyNotFoundException)
            {
                // UID not found - use display name from tag or extract from UID
                var fallbackDisplayName = !string.IsNullOrEmpty(displayName) ? displayName : ExtractDisplayName(uid);
                return $"[{fallbackDisplayName}](#unknown-reference)";
            }
        });
    }
    
    #endregion
    
    #region Link Info Creation
    
    /// <summary>
    /// Creates a LinkInfo object for a UID, resolving paths and determining link type.
    /// </summary>
    /// <param name="uid">The UID to create LinkInfo for</param>
    /// <param name="currentFilePath">The current file path</param>
    /// <returns>A LinkInfo object ready for template rendering</returns>
    public LinkInfo CreateLinkInfo(string uid, string currentFilePath)
    {
        var isExternal = _linkResolver.IsExternalReference(uid);
        
        if (isExternal)
        {
            return new LinkInfo
            {
                Uid = uid,
                DisplayName = ExtractDisplayName(uid),
                RelativePath = _linkResolver.ResolveExternalLink(uid, null),
                IsExternal = true
            };
        }
        
        // Internal link - resolve using Pass 1 mappings
        var relativePath = _linkResolver.ResolveInternalLink(currentFilePath, uid);
        
        return new LinkInfo
        {
            Uid = uid,
            DisplayName = ExtractDisplayName(uid),
            RelativePath = relativePath,
            IsExternal = false
        };
    }
    
    #endregion
    
    #region Display Name Extraction
    
    /// <summary>
    /// Extracts a user-friendly display name from a UID.
    /// </summary>
    /// <param name="uid">The UID to extract from</param>
    /// <returns>The display name (last segment of UID)</returns>
    public string ExtractDisplayName(string uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
            return uid;
        }
        
        // Remove generic arity markers (e.g., `1 in List`1)
        var cleanUid = uid.Split('`')[0];
        
        // Get last segment after dot
        var lastDot = cleanUid.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < cleanUid.Length - 1)
        {
            return cleanUid.Substring(lastDot + 1);
        }
        
        return cleanUid;
    }
    
    #endregion
    
    #region Template Rendering
    
    /// <summary>
    /// Renders a link using the link.mustache template.
    /// </summary>
    /// <param name="linkInfo">The link information to render</param>
    /// <returns>The rendered link (typically markdown)</returns>
    public string RenderLink(LinkInfo linkInfo)
    {
        // Render using Mustache template
        // Template context uses lowercase property names for Mustache
        var context = new
        {
            uid = linkInfo.Uid,
            displayName = linkInfo.DisplayName,
            relativePath = linkInfo.RelativePath,
            isExternal = linkInfo.IsExternal
        };
        
        var renderer = new StubbleBuilder().Build();
        return renderer.Render(_linkTemplate, context).Trim();
    }
    
    #endregion
}
