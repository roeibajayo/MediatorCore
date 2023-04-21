using BenchmarkDotNet.Attributes;
using MediatorCore.Benchmarks.RequestTypes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.Benchmarks;

[MemoryDiagnoser(false)]
public class Benchmark
{
    private IServiceProvider? serviceProvider;
    private CancellationTokenSource? cancellationToken;
    private IServiceProvider? scopedServiceProvider;
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
            Task.Run(() =>
            {
                service.StartAsync(cancellationToken.Token);
            });
        }

        scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
    }

    [Benchmark]
    public async Task<SimpleResponse> Response_MediatorCore()
    {
        var publihser = scopedServiceProvider!.GetService<Publisher.IPublisher>();
        return await publihser!.GetResponseAsync(request);
    }

    [Benchmark]
    public async Task<SimpleResponse> Response_MediatR()
    {
        var publihser = scopedServiceProvider!.GetService<ISender>();
        return await publihser!.Send(request);
    }

    [Benchmark]
    public void ParallelNotification_Simple_MediatorCore()
    {
        var publihser = scopedServiceProvider!.GetService<Publisher.IPublisher>();
        publihser!.Publish(simpleParallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_Simple_MediatR()
    {
        var publihser = scopedServiceProvider!.GetService<IPublisher>();
        publihser!.Publish(simpleParallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_LongRunning_MediatorCore()
    {
        var publihser = scopedServiceProvider!.GetService<Publisher.IPublisher>();
        publihser!.Publish(longRunningParallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_LongRunning_MediatR()
    {
        var publihser = scopedServiceProvider!.GetService<IPublisher>();
        publihser!.Publish(longRunningParallelNotification);
    }

    ~Benchmark()
    {
        cancellationToken!.Cancel();

        var services = serviceProvider!.GetServices<IHostedService>();
        foreach (var service in services)
        {
            Task.Run(() =>
            {
                service.StopAsync(cancellationToken.Token);
            });
        }
    }
}
