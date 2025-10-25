using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocFXMustache.Models;
using Microsoft.Extensions.Logging;
using Stubble.Core.Builders;
using Stubble.Core.Settings;

namespace DocFXMustache.Services;

/// <summary>
/// Service responsible for Pass 1 template processing.
/// Loads templates, renders Mustache templates while preserving &lt;xref&gt; tags,
/// and manages template configuration (combineMembers mode, etc.)
/// </summary>
public sealed class TemplateProcessingService
{
    private readonly ILogger<TemplateProcessingService> _logger;
    private readonly string _templateDirectory;
    private readonly TemplateConfiguration _config;
    private readonly Dictionary<string, string> _templateCache;
    private readonly Stubble.Core.StubbleVisitorRenderer _renderer;

    /// <summary>
    /// Gets the loaded template configuration
    /// </summary>
    public TemplateConfiguration Configuration => _config;

    public TemplateProcessingService(string templateDirectory, ILogger<TemplateProcessingService> logger)
    {
        _templateDirectory = templateDirectory ?? throw new ArgumentNullException(nameof(templateDirectory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _templateCache = new Dictionary<string, string>();

        // Load template configuration
        _config = LoadTemplateConfiguration(templateDirectory);

        // Initialize Stubble renderer with settings
        _renderer = new StubbleBuilder()
            .Configure(settings =>
            {
                // Preserve HTML-like tags (including <xref>)
                settings.SetIgnoreCaseOnKeyLookup(true);
                settings.SetMaxRecursionDepth(256);
            })
            .Build();

        _logger.LogInformation("Template processing service initialized with template: {TemplateName} (CombineMembers: {CombineMembers})",
            _config.Name, _config.CombineMembers);
    }

    /// <summary>
    /// Render a type documentation using the appropriate template
    /// </summary>
    public string RenderType(TypeDocumentation type, object? viewModel = null)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        var templateName = _config.Templates.GetTemplateForType(type.Type);
        var template = LoadTemplate(templateName);

        // Use provided view model or create default from TypeDocumentation
        var model = viewModel ?? CreateViewModel(type);

        _logger.LogDebug("Rendering type {TypeName} using template {TemplateName}", type.FullName, templateName);

        try
        {
            var rendered = _renderer.Render(template, model);
            return rendered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateName} for type {TypeName}", templateName, type.FullName);
            throw;
        }
    }

    /// <summary>
    /// Render a member documentation using the member template (when combineMembers = false)
    /// </summary>
    public string RenderMember(TypeDocumentation member, object? viewModel = null)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));

        var template = LoadTemplate(_config.Templates.Member);
        var model = viewModel ?? CreateViewModel(member);

        _logger.LogDebug("Rendering member {MemberName} using member template", member.FullName);

        try
        {
            var rendered = _renderer.Render(template, model);
            return rendered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering member template for {MemberName}", member.FullName);
            throw;
        }
    }

    /// <summary>
    /// Load a template file from the template directory
    /// </summary>
    private string LoadTemplate(string templateFileName)
    {
        if (_templateCache.TryGetValue(templateFileName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var templatePath = Path.Combine(_templateDirectory, templateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        _logger.LogDebug("Loading template from {TemplatePath}", templatePath);

        var templateContent = File.ReadAllText(templatePath);
        _templateCache[templateFileName] = templateContent;

        return templateContent;
    }

    /// <summary>
    /// Load template configuration from template.json
    /// </summary>
    private TemplateConfiguration LoadTemplateConfiguration(string templateDirectory)
    {
        var configPath = Path.Combine(templateDirectory, "template.json");

        if (!File.Exists(configPath))
        {
            _logger.LogWarning("template.json not found at {ConfigPath}, using default configuration", configPath);
            return new TemplateConfiguration();
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<TemplateConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (config == null)
            {
                _logger.LogWarning("Failed to deserialize template.json, using default configuration");
                return new TemplateConfiguration();
            }

            _logger.LogInformation("Loaded template configuration: {TemplateName} v{Version}", config.Name, config.Version);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template configuration from {ConfigPath}, using defaults", configPath);
            return new TemplateConfiguration();
        }
    }

    /// <summary>
    /// Create a view model from TypeDocumentation that's Mustache-friendly
    /// </summary>
    private object CreateViewModel(TypeDocumentation type)
    {
        // TODO: This will be enhanced with proper view model helpers
        // For now, return the TypeDocumentation directly
        // Mustache can access public properties
        return new
        {
            uid = type.Uid,
            name = type.Name,
            fullName = type.FullName,
            type = type.Type.ToString().ToLowerInvariant(),
            summary = type.Summary,
            remarks = type.Remarks,
            syntax = type.Syntax,
            @namespace = ExtractNamespace(type.FullName),
            
            // Collection helpers (add hasXxx flags for Mustache conditionals)
            hasInheritance = type.Inheritance.Length > 0,
            inheritance = type.Inheritance,
            
            hasImplements = type.Implements.Length > 0,
            implements = type.Implements,
            
            hasConstructors = type.Constructors.Count > 0,
            constructors = type.Constructors,
            
            hasProperties = type.Properties.Count > 0,
            properties = type.Properties,
            
            hasMethods = type.Methods.Count > 0,
            methods = type.Methods,
            
            hasEvents = type.Events.Count > 0,
            events = type.Events,
            
            hasFields = type.Fields.Count > 0,
            fields = type.Fields,
            
            hasParameters = type.Parameters.Length > 0,
            parameters = type.Parameters,
            
            hasReturns = type.Returns != null,
            returns = type.Returns,
            
            hasTypeParameters = type.TypeParameters.Length > 0,
            typeParameters = type.TypeParameters,
            
            hasExceptions = type.Exceptions.Length > 0,
            exceptions = type.Exceptions,
            
            hasAttributes = type.Attributes.Length > 0,
            attributes = type.Attributes
        };
    }

    /// <summary>
    /// Extract namespace from full type name
    /// </summary>
    private static string ExtractNamespace(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot > 0 ? fullName.Substring(0, lastDot) : string.Empty;
    }
}
