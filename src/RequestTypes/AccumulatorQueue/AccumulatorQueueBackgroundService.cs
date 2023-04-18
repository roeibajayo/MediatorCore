using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.AccumulatorQueue;

internal interface IQueueBackgroundService<TMessage> where TMessage : IAccumulatorQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class AccumulatorQueueBackgroundService<TMessage, TOptions> :
    IntervalBackgroundService,
    IQueueBackgroundService<TMessage>
    where TMessage : IAccumulatorQueueMessage
    where TOptions : class, IAccumulatorQueueOptions
{
    private readonly ConcurrentQueue<TMessage> queue;
    private readonly IServiceProvider serviceProvider;
    private readonly TOptions options;

    public AccumulatorQueueBackgroundService(IServiceProvider serviceProvider, ILogger<TMessage> logger) :
        this(serviceProvider, logger, GetOptions())
    {
    }

    public AccumulatorQueueBackgroundService(IServiceProvider serviceProvider, ILogger logger, TOptions options) :
        base(logger, options.MsInterval)
    {
        queue = new ConcurrentQueue<TMessage>();
        this.serviceProvider = serviceProvider;
        this.options = Activator.CreateInstance<TOptions>();
    }

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        var list = new List<TMessage>(queue.Count);
        while (!cancellationToken.IsCancellationRequested &&
            (options.MaxItemsOnDequeue is null || options.MaxItemsOnDequeue < list.Count) &&
            queue.TryDequeue(out var item))
        {
            list.Add(item);
        }

        if (list.Count == 0)
            return;

        await using var scope = serviceProvider.CreateAsyncScope();
        var handlerInstance = scope.ServiceProvider.GetService<IBaseAccumulatorQueue<TMessage>>();
        await handlerInstance.HandleAsync(list);
    }

    public void Enqueue(TMessage item)
    {
        if (options.MaxItemsStored is not null)
        {
            var currentItems = queue.Count;

            if (options.MaxItemsStored == currentItems)
            {
                if (options.MaxItemsBehavior is null ||
                    options.MaxItemsBehavior == MaxItemsStoredBehaviors.ThrowExceptionOnAdd)
                    throw new MaxItemsOnQueueException();

                if (options.MaxItemsBehavior == MaxItemsStoredBehaviors.DiscardEnqueues)
                    return;

                if (options.MaxItemsBehavior == MaxItemsStoredBehaviors.ForceProcess)
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
