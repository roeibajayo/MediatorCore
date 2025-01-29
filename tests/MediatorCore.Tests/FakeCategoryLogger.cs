using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests;

public class FakeCategoryLogger<T> : ILogger<T>
{
    private readonly ILogger logger;

    public FakeCategoryLogger(ILogger logger)
    {
        this.logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) =>
        logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        logger.Log(logLevel, eventId, state, exception, formatter);
}