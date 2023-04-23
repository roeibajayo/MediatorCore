using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.AccumulatorQueue;

internal interface IAccumulatorQueueBackgroundService<TMessage> where TMessage : IAccumulatorQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class AccumulatorQueueBackgroundService<TMessage, TOptions> :
    IntervalBackgroundService,
    IAccumulatorQueueBackgroundService<TMessage>
    where TMessage : IAccumulatorQueueMessage
    where TOptions : class, IAccumulatorQueueOptions
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
        this.options = Activator.CreateInstance<TOptions>();
    }

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        var items = new List<TMessage>(queue.Count);
        while (!cancellationToken.IsCancellationRequested &&
            (options.MaxItemsOnDequeue is null || options.MaxItemsOnDequeue < items.Count) &&
            queue.TryDequeue(out var item))
        {
            items.Add(item);
        }

        if (items.Count == 0)
            return;

        ProcessItems(items);
    }

    private async void ProcessItems(IEnumerable<TMessage> items)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handlerInstance = scope.ServiceProvider.GetService<IBaseAccumulatorQueue<TMessage>>();
        await handlerInstance.HandleAsync(items);
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
