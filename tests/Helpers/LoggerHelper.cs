using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DocFXMustache.Tests.Helpers;

/// <summary>
/// Helper class for creating loggers in unit tests
/// </summary>
public static class LoggerHelper
{
    /// <summary>
    /// Creates a null logger for testing (no output)
    /// </summary>
    public static ILogger<T> CreateNullLogger<T>()
    {
        return NullLogger<T>.Instance;
    }

    /// <summary>
    /// Creates a console logger for testing (with output)
    /// </summary>
    public static ILogger<T> CreateConsoleLogger<T>(bool verbose = false)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information)
                .AddConsole();
        });
        
        return factory.CreateLogger<T>();
    }
}
