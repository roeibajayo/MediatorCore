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
            var (success, messages) = await queue.TryDequeueAsync(cancellationToken);

            if (!success)
                continue;

            ProcessMessages(messages);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        queue.Dispose();
        return Task.CompletedTask;
    }

    private async void ProcessMessages(IEnumerable<TMessage> messages)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseThrottlingQueueHandler<TMessage>>();
        await ProcessMessage(handler!, 0, messages);
    }

    private async Task ProcessMessage(IBaseThrottlingQueueHandler<TMessage> handler, int retries, IEnumerable<TMessage> messages)
    {
        try
        {
            await handler!.HandleAsync(messages);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(messages, ex, retries,
                () => ProcessMessage(handler, retries + 1, messages));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    public void Enqueue(TMessage item)
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

        queue.Enqueue(item);
    }

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();
        return options;
    }
}
