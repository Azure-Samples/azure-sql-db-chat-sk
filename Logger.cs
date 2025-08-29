using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Collections.Concurrent;

namespace azure_sql_sk;

public class SpectreConsoleLogger(string categoryName) : ILogger
{
    private readonly string _categoryName = categoryName;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        // Let the framework handle the filtering - just return true
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message))
            return;

        var logLevelText = logLevel switch
        {
            LogLevel.Critical => "💥 CRIT",
            LogLevel.Error => "❌ ERROR",
            LogLevel.Warning => "⚠️  WARN",
            LogLevel.Information => "ℹ️  INFO",
            LogLevel.Debug => "🔍 DEBUG",
            LogLevel.Trace => "🔎 TRACE",
            _ => "📝 LOG"
        };

        var categoryName = _categoryName.Split('.').LastOrDefault() ?? _categoryName;
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        
        var logMessage = $"{timestamp} {logLevelText} [{categoryName}] {message}";
        
        if (exception != null)
        {
            logMessage += $"{Environment.NewLine}💀 {exception}";
        }

        AnsiConsole.MarkupLineInterpolated($"{logMessage}");
    }
}

public class SpectreConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, SpectreConsoleLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName) 
    {
        return _loggers.GetOrAdd(categoryName, name => new SpectreConsoleLogger(name));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
