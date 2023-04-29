using BenchmarkDotNet.Attributes;
using MediatorCore.Benchmarks.RequestTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.Benchmarks;

[MemoryDiagnoser(false)]

//BenchmarkDotNet cause memory leak, read here: https://github.com/dotnet/BenchmarkDotNet/issues/368
[WarmupCount(3)]
[IterationCount(3)]
public class Benchmark
{
    private IServiceProvider? rootServiceProvider;
    private CancellationTokenSource? cancellationToken;

    private readonly SimpleResponseMessage request = new(1);
    private readonly SimpleNotificationMessage simpleNotification = new(1);
    private readonly LongRunningNotificationMessage longRunningNotification = new(1);
    private readonly QueueMessage queue = new(1);
    private readonly StackMessage stack = new(1);

    private IServiceScope? scopedServiceProvider;
    private IPublisher? mediatorCorePublisher;
    private MediatR.IPublisher? mediatrPublisher;
    private MediatR.ISender? mediatrSender;

    [GlobalSetup]
    public void Setup()
    {
        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddMediatorCore<Benchmark>();
        serviceBuilder.AddMediatR(s => s.RegisterServicesFromAssemblyContaining<Benchmark>());
        rootServiceProvider = serviceBuilder.BuildServiceProvider();

        cancellationToken = new CancellationTokenSource();

        var services = rootServiceProvider.GetServices<IHostedService>();
        foreach (var service in services)
        {
            service.StartAsync(cancellationToken.Token);
        }

        scopedServiceProvider = rootServiceProvider.CreateScope();
        mediatorCorePublisher = scopedServiceProvider.ServiceProvider.GetService<IPublisher>()!;
        mediatrPublisher = scopedServiceProvider.ServiceProvider.GetService<MediatR.IPublisher>()!;
        mediatrSender = scopedServiceProvider.ServiceProvider.GetService<MediatR.ISender>()!;
    }

    [Benchmark]
    public async Task<SimpleResponse> Response_MediatorCore()
    {
        return await mediatorCorePublisher!
            .GetResponseAsync(request);
    }

    [Benchmark]
    public async Task<SimpleResponse> Response_MediatR()
    {
        return await mediatrSender!
            .Send(request);
    }

    [Benchmark]
    public void Notification_Simple_MediatorCore()
    {
        mediatorCorePublisher!
            .Publish(simpleNotification);
    }

    [Benchmark]
    public void Notification_Simple_MediatR()
    {
        mediatrPublisher!
            .Publish(simpleNotification);
    }

    [Benchmark]
    public void Notification_LongRunning_MediatorCore()
    {
        mediatorCorePublisher!
            .Publish(longRunningNotification);
    }

    [Benchmark]
    public void Notification_LongRunning_MediatR()
    {
        mediatrPublisher!
            .Publish(longRunningNotification);
    }

    [Benchmark]
    public void InsertToQueue()
    {
        mediatorCorePublisher!
            .Publish(queue);
    }

    [Benchmark]
    public void InsertToStack()
    {
        mediatorCorePublisher!
            .Publish(stack);
    }
}
