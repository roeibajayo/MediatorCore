using MediatorCore.Exceptions;
using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.AccumulatorQueue;

internal interface IAccumulatorQueueBackgroundService<TMessage>
    where TMessage :
    IAccumulatorQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class AccumulatorQueueBackgroundService<TMessage, TOptions> :
    IntervalBackgroundService,
    IAccumulatorQueueBackgroundService<TMessage>
    where TMessage : IAccumulatorQueueMessage
    where TOptions : IAccumulatorQueueOptions, new()
{
    private readonly ConcurrentQueue<TMessage> queue;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly TOptions options;

    public AccumulatorQueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }

    public AccumulatorQueueBackgroundService(IServiceScopeFactory serviceScopeFactory, TOptions options) :
        base(options.MsInterval)
    {
        queue = new ConcurrentQueue<TMessage>();
        this.serviceScopeFactory = serviceScopeFactory;
        this.options = options;
    }

    protected override Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        if (queue.IsEmpty)
            return Task.CompletedTask;

        var messages = new List<TMessage>(queue.Count);
        while (!cancellationToken.IsCancellationRequested &&
            (options.MaxMessagesOnDequeue is null || options.MaxMessagesOnDequeue < messages.Count) &&
            queue.TryDequeue(out var item))
        {
            messages.Add(item);
        }

        if (messages.Count == 0)
            return Task.CompletedTask;

        ProcessMessages(messages);
        return Task.CompletedTask;
    }

    private async void ProcessMessages(IEnumerable<TMessage> messages)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseAccumulatorQueue<TMessage>>();
        await ProcessMessage(handler!, 0, messages);
    }

    private async Task ProcessMessage(IBaseAccumulatorQueue<TMessage> handler, int retries, IEnumerable<TMessage> messages)
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

                if (options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.ForceProcess)
                    OnExecuteAsync(CancellationToken.None).Start();
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
