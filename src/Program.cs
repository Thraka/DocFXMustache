using System.CommandLine;
using DocFXMustache.Services;

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

        // Template directory option
        var templateOption = new Option<DirectoryInfo>(
            aliases: ["-t", "--template"],
            description: "Directory containing Mustache template files")
        {
            IsRequired = true
        };

        // Output format option
        var formatOption = new Option<string>(
            aliases: ["-f", "--format"],
            description: "Output format (md or mdx)")
        {
            IsRequired = true
        };
        formatOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(formatOption);
            if (value != "md" && value != "mdx")
            {
                result.ErrorMessage = "Format must be either 'md' or 'mdx'";
            }
        });

        // File grouping strategy option
        var groupingOption = new Option<string>(
            aliases: ["-g", "--grouping"],
            description: "File grouping strategy: flat, namespace, assembly-namespace, assembly-flat",
            getDefaultValue: () => "flat");
        groupingOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(groupingOption);
            var validValues = new[] { "flat", "namespace", "assembly-namespace", "assembly-flat" };
            if (!validValues.Contains(value))
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

        // Add options to root command
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(templateOption);
        rootCommand.AddOption(formatOption);
        rootCommand.AddOption(groupingOption);
        rootCommand.AddOption(dryRunOption);
        rootCommand.AddOption(verboseOption);
        rootCommand.AddOption(forceOption);

        // Set the handler
        rootCommand.SetHandler(async (input, output, template, format, grouping, dryRun, verbose, force) =>
        {
            await ProcessCommand(input, output, template, format, grouping, dryRun, verbose, force);
        },
        inputOption, outputOption, templateOption, formatOption, groupingOption, dryRunOption, verboseOption, forceOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ProcessCommand(
        DirectoryInfo input,
        DirectoryInfo output,
        DirectoryInfo template,
        string format,
        string grouping,
        bool dryRun,
        bool verbose,
        bool force)
    {
        Console.WriteLine("DocFX Mustache - Processing...");
        Console.WriteLine($"Input Directory: {input.FullName}");
        Console.WriteLine($"Output Directory: {output.FullName}");
        Console.WriteLine($"Template Directory: {template.FullName}");
        Console.WriteLine($"Output Format: {format}");
        Console.WriteLine($"Grouping Strategy: {grouping}");
        Console.WriteLine($"Dry Run: {dryRun}");
        Console.WriteLine($"Verbose: {verbose}");
        Console.WriteLine($"Force Overwrite: {force}");

        // Validate input directory exists
        if (!input.Exists)
        {
            Console.Error.WriteLine($"Error: Input directory '{input.FullName}' does not exist.");
            Environment.Exit(1);
        }

        // Validate template directory exists
        if (!template.Exists)
        {
            Console.Error.WriteLine($"Error: Template directory '{template.FullName}' does not exist.");
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

        // Initialize services
        var parsingService = new MetadataParsingService();
        var discoveryService = new DiscoveryService(parsingService);

        try
        {
            // Phase 2 - Pass 1: Discovery
            Console.WriteLine("\n🔍 Phase 2 - Pass 1: Discovery");
            Console.WriteLine("Building UID mappings and file structure...");
            
            var uidMappings = await discoveryService.BuildUidMappingsAsync(input.FullName, grouping);
            
            if (verbose)
            {
                Console.WriteLine($"✅ Discovery completed:");
                Console.WriteLine($"   - Total UIDs: {uidMappings.TotalUids}");
                Console.WriteLine($"   - Assemblies: {string.Join(", ", uidMappings.Assemblies)}");
                Console.WriteLine($"   - Namespaces: {string.Join(", ", uidMappings.Namespaces)}");
            }

            if (dryRun)
            {
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
            Console.Error.WriteLine($"❌ Error during processing: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            Environment.Exit(1);
        }

        Console.WriteLine("\n✅ Command line parsing completed successfully!");
        Console.WriteLine("📝 Ready to implement core processing logic...");

        await Task.CompletedTask; // Placeholder for async operations
    }
}
