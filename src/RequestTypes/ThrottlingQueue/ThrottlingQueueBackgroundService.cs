using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

internal interface IThrottlingQueueBackgroundService<TMessage> where TMessage : IThrottlingQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class ThrottlingQueueBackgroundService<TMessage, TOptions> :
    IThrottlingQueueBackgroundService<TMessage>,
    IHostedService
    where TMessage : IThrottlingQueueMessage
    where TOptions : class, IThrottlingQueueOptions
{
    private readonly LockingThrottlingQueue<TMessage> queue;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private bool running = true;

    public ThrottlingQueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }

    public ThrottlingQueueBackgroundService(IServiceScopeFactory serviceScopeFactory, TOptions options)
    {
        queue = new(options.ThrottlingTimeSpans);
        this.serviceScopeFactory = serviceScopeFactory;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && running)
        {
            var messageResult = await queue.TryDequeueAsync(cancellationToken);

            if (!messageResult.Success)
                continue;

            ProcessItems(messageResult.Items);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        queue.Dispose();
        return Task.CompletedTask;
    }

    private async void ProcessItems(IEnumerable<TMessage> items)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseThrottlingQueue<TMessage>>();
        await ProcessItem(handler, 0, items);
    }

    private async Task ProcessItem(IBaseThrottlingQueue<TMessage> handler, int retries, IEnumerable<TMessage> items)
    {
        try
        {
            await handler!.HandleAsync(items);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleException(items, ex, retries,
                () => ProcessItem(handler, retries + 1, items));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    public void Enqueue(TMessage item)
    {
        queue.Enqueue(item);
    }


    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();
        return options;
    }
}
