using MediatorCore.Exceptions;
using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.AccumulatorQueue;

internal interface IAccumulatorQueueBackgroundService<TMessage>
    where TMessage :
    IAccumulatorQueueMessage
{
    ValueTask EnqueueAsync(TMessage item, CancellationToken cancellationToken);
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
            (options.AccumulationCapacity is null || options.AccumulationCapacity < messages.Count) &&
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
        await AccumulatorQueueBackgroundService<TMessage, TOptions>.ProcessMessage(handler!, 0, messages);
    }

    private static async Task ProcessMessage(IBaseAccumulatorQueue<TMessage> handler, int retries, IEnumerable<TMessage> messages)
    {
        try
        {
            await handler!.HandleAsync(messages);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(messages, ex, retries,
                () => AccumulatorQueueBackgroundService<TMessage, TOptions>.ProcessMessage(handler, retries + 1, messages));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    public async ValueTask EnqueueAsync(TMessage item, CancellationToken cancellationToken)
    {
        if (options.TotalCapacity is not null)
        {
            var currentMessages = queue.Count;

            if (options.TotalCapacity == currentMessages)
            {
                if (options.MaxTotalCapacityBehavior is null ||
                    options.MaxTotalCapacityBehavior == MaxCapacityBehaviors.Wait)
                {
                    while (!cancellationToken.IsCancellationRequested && queue.Count == options.TotalCapacity)
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                }

                if (options.MaxTotalCapacityBehavior == MaxCapacityBehaviors.DropMessage)
                    return;
            }
        }

        queue.Enqueue(item);
    }

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();

        if (options.AccumulationCapacity is not null && options.AccumulationCapacity < 1)
            throw new ArgumentOutOfRangeException(nameof(options.AccumulationCapacity));

        if (options.TotalCapacity is not null && options.TotalCapacity < 1)
            throw new ArgumentOutOfRangeException(nameof(options.TotalCapacity));

        if (options.MsInterval < 100)
            throw new ArgumentException("MsInternal must be aleast 100ms.", nameof(options.MsInterval));

        return options;
    }
}
