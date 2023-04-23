using BenchmarkDotNet.Attributes;
using MediatorCore.Benchmarks.RequestTypes;
using MediatorCore.Publisher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.Benchmarks;

[MemoryDiagnoser(false)]
[ExceptionDiagnoser]
public class Benchmark
{
    private IServiceProvider? rootServiceProvider;
    private CancellationTokenSource? cancellationToken;

    private SimpleResponseMessage request = new(1);
    private SimpleParallelNotificationMessage simpleParallelNotification = new(1);
    private LongRunningParallelNotificationMessage longRunningParallelNotification = new(1);

    private IServiceScope scopedServiceProvider;
    private IPublisher mediatorCorePublisher;
    private MediatR.IPublisher mediatrPublisher;
    private MediatR.ISender mediatrSender;

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
    }

    [IterationSetup]
    public void IterationSetup()
    {
        scopedServiceProvider = rootServiceProvider.CreateScope();
        mediatorCorePublisher = scopedServiceProvider.ServiceProvider.GetService<MediatorCore.Publisher.IPublisher>()!;
        mediatrPublisher = scopedServiceProvider.ServiceProvider.GetService<MediatR.IPublisher>()!;
        mediatrSender = scopedServiceProvider.ServiceProvider.GetService<MediatR.ISender>()!;
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        scopedServiceProvider.Dispose();
        GC.Collect();
    }

    [GlobalCleanup]
    public void Dispose()
    {
        cancellationToken!.Cancel();

        var services = rootServiceProvider!.GetServices<IHostedService>();
        foreach (var service in services)
        {
            service.StopAsync(cancellationToken.Token);
        }

        cancellationToken.Dispose();
    }

    [Benchmark]
    public async Task<SimpleResponse> Response_MediatorCore()
    {
        return await mediatorCorePublisher
            .GetResponseAsync(request);
    }

    [Benchmark]
    public async Task<SimpleResponse> Response_MediatR()
    {
        return await mediatrSender.Send(request);
    }

    [Benchmark]
    public void ParallelNotification_Simple_MediatorCore()
    {
        mediatorCorePublisher
            .Publish(simpleParallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_Simple_MediatR()
    {
        mediatrPublisher
            .Publish(simpleParallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_LongRunning_MediatorCore()
    {
        mediatorCorePublisher
            .Publish(longRunningParallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_LongRunning_MediatR()
    {
        mediatrPublisher
            .Publish(longRunningParallelNotification);
    }
}
