using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace MediatorCore.RequestTypes.Queue;

internal interface IQueueBackgroundService<TMessage>
    where TMessage : IQueueMessage
{
    ValueTask EnqueueAsync(TMessage item, CancellationToken cancellationToken);
}
internal sealed class QueueBackgroundService<TMessage, TOptions> :
    BackgroundService,
    IQueueBackgroundService<TMessage>
    where TMessage : IQueueMessage
    where TOptions : QueueOptions
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly Channel<TMessage> queue;

    public QueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }
    public QueueBackgroundService(IServiceScopeFactory serviceScopeFactory, TOptions options)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.queue = options.Capacity is null ?
            Channel.CreateUnbounded<TMessage>() :
            Channel.CreateBounded<TMessage>(new BoundedChannelOptions(options.Capacity.Value)
            {
                FullMode = options.MaxCapacityBehavior == MaxCapacityBehaviors.DropMessage ?
                    BoundedChannelFullMode.DropWrite :
                    BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = true,
            });
    }

    public async ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken)
    {
        await queue.Writer.WriteAsync(message, cancellationToken);
    }

    private async Task ProcessMessage(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IBaseQueueHandler<TMessage>>();
        await ProcessMessage(handler, 0, item);
    }

    private static async Task ProcessMessage(IBaseQueueHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler.HandleExceptionAsync(item, ex, retries,
                () => ProcessMessage(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in queue.Reader.ReadAllAsync(cancellationToken))
        {
            await ProcessMessage(item);
        }
    }

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();

        if (options.Capacity is not null && options.Capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(options.Capacity));

        return options;
    }

}
