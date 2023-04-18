using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MediatorCore.Tests;

public class BaseUnitTest
{
    private static IServiceProvider? _serviceProvider;
    private static object _lock = new object();

    protected IServiceProvider serviceProvider =>
        _serviceProvider ?? GenerateServiceProvider();

    protected IServiceProvider GenerateServiceProvider()
    {
        lock (_lock)
        {
            if (_serviceProvider is not null)
                return _serviceProvider;

            var serviceBuilder = new ServiceCollection();
            serviceBuilder.AddMediatorCore<BaseUnitTest>();
            var logger = Substitute.For<ILogger>();
            serviceBuilder.AddSingleton(logger);
            serviceBuilder.AddTransient(typeof(ILogger<>), typeof(FakeCategoryLogger<>));

            var serviceProvider = serviceBuilder.BuildServiceProvider();

            var services = serviceProvider.GetServices<IHostedService>();
            foreach (var service in services)
            {
                Task.Run(() =>
                {
                    service.StartAsync(CancellationToken.None);
                });
            }

            _serviceProvider = serviceProvider;
            return serviceProvider;
        }
    }

    protected int ReceivedWarnings(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Warning) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
    protected int ReceivedErrors(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Error) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
    protected int ReceivedInformations(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Information) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
    protected int ReceivedDebugs(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Debug) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
}

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