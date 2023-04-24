using MediatorCore.Infrastructure;
using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.Queue;

internal sealed class QueueBackgroundService<TMessage> :
    IHostedService
    where TMessage : IQueueMessage
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly LockingQueue<TMessage> queue = new();
    private bool running = true;

    public QueueBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    internal void Enqueue(TMessage message)
    {
        queue.Enqueue(message);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && running)
        {
            var messageResult = await queue.TryDequeueAsync(cancellationToken);

            if (!messageResult.Success)
                continue;

            ProcessItem(messageResult);
        }
    }

    private async void ProcessItem((bool Success, TMessage Item) messageResult)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IQueueHandler<TMessage>>();
        ProcessItem(handler, 0, messageResult.Item);
    }

    private async Task ProcessItem(IQueueHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleException(item, ex, retries,
                () => ProcessItem(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        queue.Dispose();
        return Task.CompletedTask;
    }
}
