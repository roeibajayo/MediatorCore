using BenchmarkDotNet.Attributes;
using MediatorCore.Benchmarks.RequestTypes;
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

    [Benchmark]
    public void ParallelNotification_Simple_MediatorCore()
    {
        rootServiceProvider!
            .GetService<MediatorCore.Publisher.IPublisher>()!
            .Publish(simpleParallelNotification);
    }

    //[Benchmark]
    //public void ParallelNotification_Simple_MediatR()
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

        var services = rootServiceProvider!.GetServices<IHostedService>();
        foreach (var service in services)
        {
            service.StopAsync(cancellationToken.Token);
        }

        cancellationToken.Dispose();
    }
}
