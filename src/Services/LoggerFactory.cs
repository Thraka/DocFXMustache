using Microsoft.Extensions.Logging;

namespace DocFXMustache.Services;

/// <summary>
/// Factory for creating and configuring loggers for the application.
/// </summary>
public static class LoggerFactory
{
    /// <summary>
    /// Creates a logger factory with console output configured based on verbosity.
    /// </summary>
    /// <param name="isVerbose">If true, sets log level to Debug; otherwise, Information.</param>
    /// <returns>A configured ILoggerFactory instance.</returns>
    public static ILoggerFactory Create(bool isVerbose = false)
    {
        var logLevel = isVerbose ? LogLevel.Debug : LogLevel.Information;
        
        return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(logLevel)
                .AddConsole(options =>
                {
                    options.FormatterName = "simple";
                })
                .AddDebug();
        });
    }

    /// <summary>
    /// Creates a logger of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <param name="factory">The logger factory to use.</param>
    /// <returns>A configured ILogger instance.</returns>
    public static ILogger<T> CreateLogger<T>(ILoggerFactory factory)
    {
        return factory.CreateLogger<T>();
    }
}
