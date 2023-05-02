using MediatorCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.Queue;

internal interface IQueueBackgroundService<TMessage>
    where TMessage : IQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class QueueBackgroundService<TMessage, TOptions> :
    IQueueBackgroundService<TMessage>,
    IHostedService
    where TMessage : IQueueMessage
    where TOptions : IQueueOptions
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly TOptions options;
    private readonly ConcurrentQueue<TMessage> queue = new();
    private readonly object locker = new();
    private bool running = false;

    public QueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }
    public QueueBackgroundService(IServiceScopeFactory serviceScopeFactory, TOptions options)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.options = options;
    }

    public void Enqueue(TMessage message)
    {
        if (options.MaxMessagesStored is not null)
        {
            var currentMessages = queue.Count;

            if (options.MaxMessagesStored == currentMessages)
            {
                if (options.MaxMessagesStoredBehavior is null ||
                    options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.ThrowExceptionOnEnqueue)
                    MaxMessagesOnQueueException.Throw();

                if (options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.DiscardEnqueues)
                    return;
            }
        }

        queue.Enqueue(message);
        TryProcessMessage();
    }

    internal async void TryProcessMessage()
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

        await ProcessMessage(item);
    }

    private async Task ProcessMessage(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseQueueHandler<TMessage>>();
        await ProcessMessage(handler!, 0, item);
    }

    private async Task ProcessMessage(IBaseQueueHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(item, ex, retries,
                () => ProcessMessage(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
        finally
        {
            lock (locker)
            {
                running = false;
            }

            TryProcessMessage();
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

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();

        if (options.MaxMessagesStored is not null && options.MaxMessagesStored < 1)
            throw new ArgumentOutOfRangeException(nameof(options.MaxMessagesStored));

        return options;
    }
}
