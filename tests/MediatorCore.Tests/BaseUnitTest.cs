using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MediatorCore.Tests;

public class BaseUnitTest
{
    private static IServiceProvider? _serviceProvider;
    private static readonly object _lock = new();

    protected static IServiceProvider ServiceProvider
    {
        get
        {
            lock (_lock)
            {
                _serviceProvider ??= GenerateServiceProvider(serviceBuilder => serviceBuilder.AddMediatorCore<BaseUnitTest>());
                return _serviceProvider;
            }
        }
    }

    protected static IServiceProvider GenerateServiceProvider(Action<ServiceCollection>? serviceCollection = null)
    {
        var serviceBuilder = new ServiceCollection();
        serviceCollection?.Invoke(serviceBuilder);
        var logger = Substitute.For<ILogger>();
        serviceBuilder.AddSingleton(logger);
        serviceBuilder.AddTransient(typeof(ILogger<>), typeof(FakeCategoryLogger<>));

        var serviceProvider = serviceBuilder.BuildServiceProvider();

        var services = serviceProvider.GetServices<IHostedService>();
        foreach (var service in services)
        {
            service.StartAsync(CancellationToken.None);
        }

        return serviceProvider;
    }

    protected static int ReceivedWarnings(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Warning) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
    protected static int ReceivedErrors(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Error) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
    protected static int ReceivedInformations(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Information) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
    protected static int ReceivedDebugs(ILogger logger, string contains)
    {
        return logger
            .ReceivedCalls()
            .Select(call => call.GetArguments())
            .Count(callArguments => ((LogLevel)callArguments[0]).Equals(LogLevel.Debug) &&
                                    ((IReadOnlyList<KeyValuePair<string, object>>)callArguments[2])
                                        .Last().Value.ToString().Contains(contains));
    }
}
