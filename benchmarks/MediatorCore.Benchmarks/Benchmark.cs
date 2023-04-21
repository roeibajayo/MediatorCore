using BenchmarkDotNet.Attributes;
using MediatorCore.Benchmarks.RequestTypes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.Benchmarks;

[MemoryDiagnoser(false)]
[ExceptionDiagnoser]
public class Benchmark
{
    private IServiceProvider? serviceProvider;
    private CancellationTokenSource? cancellationToken;
    private IServiceProvider? scopedServiceProvider;
    private Publisher.IPublisher? mediatorCorePublihser;
    private IPublisher? mediatrPublihser;
    private ISender? mediatrSender;
    private SimpleResponseMessage request = new(1);
    private SimpleParallelNotificationMessage simpleParallelNotification = new(1);
    private LongRunningParallelNotificationMessage longRunningParallelNotification = new(1);

    [GlobalSetup]
    public void Setup()
    {
        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddMediatorCore<Benchmark>();
        serviceBuilder.AddMediatR(s => s.RegisterServicesFromAssemblyContaining<Benchmark>());
        serviceProvider = serviceBuilder.BuildServiceProvider();

        cancellationToken = new CancellationTokenSource();

        var services = serviceProvider.GetServices<IHostedService>();
        foreach (var service in services)
        {
            service.StartAsync(cancellationToken.Token);
        }

        scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
        mediatorCorePublihser = scopedServiceProvider!.GetService<Publisher.IPublisher>();
        mediatrPublihser = scopedServiceProvider!.GetService<IPublisher>();
        mediatrSender = scopedServiceProvider!.GetService<ISender>();
    }

    //[Benchmark]
    //public async Task<SimpleResponse> Response_MediatorCore()
    //{
    //    return await mediatorCorePublihser!.GetResponseAsync(request);
    //}

    //[Benchmark]
    //public async Task<SimpleResponse> Response_MediatR()
    //{
    //    return await mediatrSender!.Send(request);
    //}

    [Benchmark(Description = "ParallelNotification_Simple")]
    [BenchmarkCategory("MediatorCore")]
    public void ParallelNotification_Simple1()
    {
        mediatorCorePublihser!.Publish(simpleParallelNotification);
    }

    //[Benchmark(Description = "ParallelNotification_Simple")]
    //[BenchmarkCategory("MediatR")]
    //public void ParallelNotification_Simple2()
    //{
    //    mediatrPublihser!.Publish(simpleParallelNotification);
    //}

    //[Benchmark]
    //public void ParallelNotification_LongRunning_MediatorCore()
    //{
    //    var publihser = scopedServiceProvider!.GetService<Publisher.IPublisher>();
    //    publihser!.Publish(longRunningParallelNotification);
    //}

    //[Benchmark]
    //public void ParallelNotification_LongRunning_MediatR()
    //{
    //    var publihser = scopedServiceProvider!.GetService<IPublisher>();
    //    publihser!.Publish(longRunningParallelNotification);
    //}

    [GlobalCleanup]
    public void Dispose()
    {
        cancellationToken!.Cancel();

        var services = serviceProvider!.GetServices<IHostedService>();
        foreach (var service in services)
        {
            service.StopAsync(cancellationToken.Token);
        }

        cancellationToken.Dispose();
    }
}
