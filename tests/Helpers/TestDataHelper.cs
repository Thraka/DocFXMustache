using System;
using System.IO;
using System.Reflection;

namespace DocFXMustache.Tests.Helpers;

/// <summary>
/// Helper class for accessing test fixtures and temporary directories
/// </summary>
public static class TestDataHelper
{
    private static readonly string _projectRoot = GetProjectRoot();

    /// <summary>
    /// Gets the root directory of the test project
    /// </summary>
    public static string GetProjectRoot()
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "");
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "DocFXMustache.Tests.csproj")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new InvalidOperationException("Could not find test project root");
    }

    /// <summary>
    /// Gets the fixtures directory containing test YAML files
    /// </summary>
    public static string GetFixturesDirectory()
    {
        return Path.Combine(_projectRoot, "Fixtures");
    }

    /// <summary>
    /// Gets the full path to a fixture file by name
    /// </summary>
    public static string GetFixturePath(string filename)
    {
        var path = Path.Combine(GetFixturesDirectory(), filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Fixture file not found: {filename}", path);
        return path;
    }

    /// <summary>
    /// Reads the contents of a fixture file as a string
    /// </summary>
    public static string LoadFixture(string filename)
    {
        return File.ReadAllText(GetFixturePath(filename));
    }

    /// <summary>
    /// Creates a temporary directory for test output
    /// </summary>
    public static string CreateTempDirectory(string? testName = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DocFXMustacheTests");
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        var testDir = Path.Combine(tempDir, testName ?? Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        return testDir;
    }

    /// <summary>
    /// Cleans up a temporary directory
    /// </summary>
    public static void CleanupTempDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
