using System.CommandLine;
using System.Text.Json;
using DocFXMustache.Models;
using DocFXMustache.Services;
using Microsoft.Extensions.Logging;

namespace DocFXMustache;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("DocFX Mustache - Transform DocFX API metadata into Markdown/MDX files using Mustache templates");

        // Input directory option
        var inputOption = new Option<DirectoryInfo>(
            aliases: ["-i", "--input"],
            description: "Input directory containing DocFX YAML metadata files")
        {
            IsRequired = true
        };

        // Output directory option
        var outputOption = new Option<DirectoryInfo>(
            aliases: ["-o", "--output"],
            description: "Output directory for generated Markdown/MDX files")
        {
            IsRequired = true
        };

        // Templates directory option
        var templatesOption = new Option<DirectoryInfo>(
            aliases: ["-t", "--templates"],
            description: "Directory containing Mustache template files")
        {
            IsRequired = true
        };

        // Output format option
        var formatOption = new Option<string?>(
            aliases: ["-f", "--format"],
            description: "Output format (md or mdx). If not specified, uses template default.")
        {
            IsRequired = false
        };
        formatOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(formatOption);
            if (value != null && value != "md" && value != "mdx")
            {
                result.ErrorMessage = "Format must be either 'md' or 'mdx'";
            }
        });

        // File grouping strategy option
        var groupingOption = new Option<string?>(
            aliases: ["-g", "--grouping"],
            description: "File grouping strategy: flat, namespace, assembly-namespace, assembly-flat. If not specified, uses template default.");
        groupingOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(groupingOption);
            var validValues = new[] { "flat", "namespace", "assembly-namespace", "assembly-flat" };
            if (value != null && !validValues.Contains(value))
            {
                result.ErrorMessage = $"Grouping must be one of: {string.Join(", ", validValues)}";
            }
        });

        // Dry run option
        var dryRunOption = new Option<bool>(
            aliases: ["--dry-run"],
            description: "Preview the file structure without generating files",
            getDefaultValue: () => false);

        // Verbose option
        var verboseOption = new Option<bool>(
            aliases: ["-v", "--verbose"],
            description: "Enable verbose logging",
            getDefaultValue: () => false);

        // Force overwrite option
        var forceOption = new Option<bool>(
            aliases: ["--force"],
            description: "Overwrite existing output files without prompting",
            getDefaultValue: () => false);

        // Filename case control option
        var caseOption = new Option<string?>(
            aliases: ["--case"],
            description: "Filename case: uppercase, lowercase, or mixed. If not specified, uses template default.");
        caseOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(caseOption);
            var validValues = new[] { "uppercase", "lowercase", "mixed" };
            if (value != null && !validValues.Contains(value))
            {
                result.ErrorMessage = $"Case must be one of: {string.Join(", ", validValues)}";
            }
        });

        // Add options to root command
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(templatesOption);
        rootCommand.AddOption(formatOption);
        rootCommand.AddOption(groupingOption);
        rootCommand.AddOption(dryRunOption);
        rootCommand.AddOption(verboseOption);
        rootCommand.AddOption(forceOption);
        rootCommand.AddOption(caseOption);

        // Set the handler
        rootCommand.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var templates = context.ParseResult.GetValueForOption(templatesOption)!;
            var format = context.ParseResult.GetValueForOption(formatOption);
            var grouping = context.ParseResult.GetValueForOption(groupingOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var force = context.ParseResult.GetValueForOption(forceOption);
            var caseControl = context.ParseResult.GetValueForOption(caseOption);
            
            await ProcessCommand(input, output, templates, format, grouping, dryRun, verbose, force, caseControl);
        });

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ProcessCommand(
        DirectoryInfo input,
        DirectoryInfo output,
        DirectoryInfo templates,
        string? format,
        string? grouping,
        bool dryRun,
        bool verbose,
        bool force,
        string? caseControl)
    {
        // Load template configuration to get defaults
        var templateConfig = LoadTemplateConfiguration(templates.FullName);
        
        // Resolve final values: CLI overrides template defaults
        var finalFormat = format ?? templateConfig.OutputFormat;
        var finalGrouping = grouping ?? templateConfig.FileGrouping;
        var finalCaseControl = caseControl ?? templateConfig.FilenameCase;

        Console.WriteLine("DocFX Mustache - Processing...");
        Console.WriteLine($"Input Directory: {input.FullName}");
        Console.WriteLine($"Output Directory: {output.FullName}");
        Console.WriteLine($"Templates Directory: {templates.FullName}");
        Console.WriteLine($"Template: {templateConfig.Name} v{templateConfig.Version}");
        Console.WriteLine($"Output Format: {finalFormat}{(format == null ? " (from template)" : " (overridden)")}");
        Console.WriteLine($"Grouping Strategy: {finalGrouping}{(grouping == null ? " (from template)" : " (overridden)")}");
        Console.WriteLine($"Filename Case: {finalCaseControl}{(caseControl == null ? " (from template)" : " (overridden)")}");
        Console.WriteLine($"Dry Run: {dryRun}");
        Console.WriteLine($"Verbose: {verbose}");
        Console.WriteLine($"Force Overwrite: {force}");

        // Validate input directory exists
        if (!input.Exists)
        {
            Console.Error.WriteLine($"Error: Input directory '{input.FullName}' does not exist.");
            Environment.Exit(1);
        }

        // Validate templates directory exists
        if (!templates.Exists)
        {
            Console.Error.WriteLine($"Error: Templates directory '{templates.FullName}' does not exist.");
            Environment.Exit(1);
        }

        // Create output directory if it doesn't exist (unless dry run)
        if (!dryRun && !output.Exists)
        {
            if (verbose)
                Console.WriteLine($"Creating output directory: {output.FullName}");
            output.Create();
        }

        if (verbose)
        {
            Console.WriteLine("\n=== Verbose Mode Enabled ===");
            Console.WriteLine($"Scanning for YAML files in: {input.FullName}");
        }

        // Initialize logging
        using var loggerFactory = Services.LoggerFactory.Create(verbose);
        var parsingLogger = loggerFactory.CreateLogger<MetadataParsingService>();
        var discoveryLogger = loggerFactory.CreateLogger<DiscoveryService>();
        var programLogger = loggerFactory.CreateLogger<Program>();

        programLogger.LogInformation("DocFX Mustache - Starting processing");
        programLogger.LogInformation("Input: {InputDirectory}", input.FullName);
        programLogger.LogInformation("Output: {OutputDirectory}", output.FullName);
        programLogger.LogInformation("Templates: {TemplatesDirectory}", templates.FullName);
        programLogger.LogInformation("Format: {Format}, Grouping: {Grouping}, Case: {Case}", 
            finalFormat, finalGrouping, finalCaseControl);

        // Initialize services
        var parsingService = new MetadataParsingService(parsingLogger);
        var discoveryService = new DiscoveryService(parsingService, discoveryLogger);

        try
        {
            // Phase 2 - Pass 1: Discovery
            programLogger.LogInformation("Starting Phase 2 - Pass 1: Discovery");
            Console.WriteLine("\n🔍 Phase 2 - Pass 1: Discovery");
            Console.WriteLine("Building UID mappings and file structure...");
            
            var uidMappings = await discoveryService.BuildUidMappingsAsync(input.FullName, finalGrouping, finalCaseControl);
            
            programLogger.LogInformation("Discovery phase completed successfully");
            
            if (verbose)
            {
                Console.WriteLine($"✅ Discovery completed:");
                Console.WriteLine($"   - Total UIDs: {uidMappings.TotalUids}");
                Console.WriteLine($"   - Assemblies: {string.Join(", ", uidMappings.Assemblies)}");
                Console.WriteLine($"   - Namespaces: {string.Join(", ", uidMappings.Namespaces)}");
            }

            if (dryRun)
            {
                programLogger.LogInformation("Dry run mode - showing planned output structure");
                Console.WriteLine("\n📋 Dry Run - Planned Output Structure:");
                foreach (var mapping in uidMappings.UidToFilePath.Take(10)) // Show first 10
                {
                    Console.WriteLine($"   {mapping.Key} → {mapping.Value}");
                }
                if (uidMappings.UidToFilePath.Count > 10)
                {
                    Console.WriteLine($"   ... and {uidMappings.UidToFilePath.Count - 10} more files");
                }
            }
            else
            {
                programLogger.LogInformation("Phase 3 not yet implemented - generation phase pending");
                Console.WriteLine("\n📝 Phase 2 - Pass 2: Generation");
                Console.WriteLine("(Not yet implemented - Phase 3 coming next)");
                
                // TODO: Implement Pass 2
                // 1. XRef processing using UID mappings
                // 2. Mustache template processing
                // 3. File generation
            }
        }
        catch (Exception ex)
        {
            programLogger.LogError(ex, "Error during processing");
            Console.Error.WriteLine($"❌ Error during processing: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            Environment.Exit(1);
        }

        programLogger.LogInformation("Processing completed successfully");
        Console.WriteLine("\n✅ Command line parsing completed successfully!");
        Console.WriteLine("📝 Ready to implement core processing logic...");

        await Task.CompletedTask; // Placeholder for async operations
    }

    /// <summary>
    /// Load template configuration from template.json in the specified directory
    /// </summary>
    private static TemplateConfiguration LoadTemplateConfiguration(string templateDirectory)
    {
        var configPath = Path.Combine(templateDirectory, "template.json");

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Warning: template.json not found at {configPath}, using default configuration");
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
                Console.WriteLine("Warning: Failed to deserialize template.json, using default configuration");
                return new TemplateConfiguration();
            }

            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error loading template configuration from {configPath}: {ex.Message}");
            Console.WriteLine("Using default configuration");
            return new TemplateConfiguration();
        }
    }
}
