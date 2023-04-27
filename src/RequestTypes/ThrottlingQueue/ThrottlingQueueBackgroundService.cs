using MediatorCore.Exceptions;
using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

internal interface IThrottlingQueueBackgroundService<TMessage>
    where TMessage :
    IThrottlingQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class ThrottlingQueueBackgroundService<TMessage, TOptions> :
    IThrottlingQueueBackgroundService<TMessage>,
    IHostedService
    where TMessage : IThrottlingQueueMessage
    where TOptions : IThrottlingQueueOptions, new()
{
    private readonly LockingThrottlingQueue<TMessage> queue;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly TOptions options;
    private bool running = true;

    public ThrottlingQueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }

    public ThrottlingQueueBackgroundService(IServiceScopeFactory serviceScopeFactory, TOptions options)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.options = options;
        queue = new(options.ThrottlingTimeSpans);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && running)
        {
            var (success, items) = await queue.TryDequeueAsync(cancellationToken);

            if (!success)
                continue;

            ProcessItems(items);
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
        var handler = scope.ServiceProvider.GetService<IBaseThrottlingQueueHandler<TMessage>>();
        await ProcessItem(handler!, 0, items);
    }

    private async Task ProcessItem(IBaseThrottlingQueueHandler<TMessage> handler, int retries, IEnumerable<TMessage> items)
    {
        try
        {
            await handler!.HandleAsync(items);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(items, ex, retries,
                () => ProcessItem(handler, retries + 1, items));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    public void Enqueue(TMessage item)
    {
        if (options.MaxMessagesStored is not null)
        {
            var currentItems = queue.Count;

            if (options.MaxMessagesStored == currentItems)
            {
                if (options.MaxMessagesStoredBehavior is null ||
                    options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.ThrowExceptionOnEnqueue)
                    MaxMessagesOnQueueException.Throw();

                if (options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.DiscardEnqueues)
                    return;
            }
        }

        queue.Enqueue(item);
    }

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();
        return options;
    }
}
