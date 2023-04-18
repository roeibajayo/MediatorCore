using BenchmarkDotNet.Attributes;
using MediatorCore.Benchmarks.RequestTypes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.Benchmarks;

[MemoryDiagnoser(false)]
public class Benchmark
{
    private IServiceProvider? scopedServiceProvider;
    private SimpleResponseMessage request = new(1);
    private SimpleParallelNotificationMessage parallelNotification = new(1);

    [GlobalSetup]
    public void Setup()
    {
        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddMediatorCore<Benchmark>();
        serviceBuilder.AddMediatR(s => s.RegisterServicesFromAssemblyContaining<Benchmark>());
        var serviceProvider = serviceBuilder.BuildServiceProvider();

        var services = serviceProvider.GetServices<IHostedService>();
        foreach (var service in services)
        {
            Task.Run(() =>
            {
                service.StartAsync(CancellationToken.None);
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
    public void ParallelNotification_MediatorCore()
    {
        var publihser = scopedServiceProvider!.GetService<Publisher.IPublisher>();
        publihser!.Publish(parallelNotification);
    }

    [Benchmark]
    public void ParallelNotification_MediatR()
    {
        var publihser = scopedServiceProvider!.GetService<IPublisher>();
        publihser!.Publish(parallelNotification);
    }
}
