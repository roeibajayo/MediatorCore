using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.Queue;

internal sealed class QueueBackgroundService<TMessage> :
    IHostedService
    where TMessage : IQueueMessage
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ConcurrentQueue<TMessage> queue = new();
    private readonly object locker = new();
    private bool running = false;

    public QueueBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    internal void Enqueue(TMessage message)
    {
        queue.Enqueue(message);
        TryProcessItem();
    }

    internal async void TryProcessItem()
    {
        TMessage item;

        lock (locker)
        {
            if (running)
                return;

            if (!queue.TryDequeue(out item))
            {
                return;
            }

            running = true;
        }

        await ProcessItem(item);
    }

    private async Task ProcessItem(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IQueueHandler<TMessage>>();
        await ProcessItem(handler!, 0, item);
    }

    private async Task ProcessItem(IQueueHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(item, ex, retries,
                () => ProcessItem(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
        finally
        {
            lock (locker)
            {
                running = false;
            }

            TryProcessItem();
        }
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
