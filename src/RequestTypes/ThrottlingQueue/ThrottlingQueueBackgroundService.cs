using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

internal interface IThrottlingQueueBackgroundService<TMessage>
    where TMessage :
    IThrottlingQueueMessage
{
    ValueTask EnqueueAsync(TMessage item, CancellationToken cancellationToken);
}
internal sealed class ThrottlingQueueBackgroundService<TMessage, TOptions>(IServiceScopeFactory serviceScopeFactory, TOptions options) :
    IThrottlingQueueBackgroundService<TMessage>,
    IHostedService
    where TMessage : IThrottlingQueueMessage
    where TOptions : IThrottlingQueueOptions, new()
{
    private readonly LockingThrottlingQueue<TMessage> queue = new(options.ThrottlingTimeSpans);

    public ThrottlingQueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var (success, messages) = await queue.TryDequeueAsync(cancellationToken);

            if (!success)
                continue;

            ProcessMessages(messages);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Dispose();
        return Task.CompletedTask;
    }

    private async void ProcessMessages(IEnumerable<TMessage> messages)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseThrottlingQueueHandler<TMessage>>();
        await ProcessMessage(handler!, 0, messages);
    }

    private static async Task ProcessMessage(IBaseThrottlingQueueHandler<TMessage> handler, int retries, IEnumerable<TMessage> messages)
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

    public async ValueTask EnqueueAsync(TMessage item, CancellationToken cancellationToken)
    {
        if (options.Capacity is not null)
        {
            var currentMessages = queue.Count;

            if (options.Capacity == currentMessages)
            {
                if (options.MaxCapacityBehavior is null ||
                    options.MaxCapacityBehavior == MaxCapacityBehaviors.Wait)
                {
                    while (!cancellationToken.IsCancellationRequested && queue.Count == options.Capacity)
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                }

                if (options.MaxCapacityBehavior == MaxCapacityBehaviors.DropMessage)
                    return;
            }
        }

        queue.Enqueue(item);
    }

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();

        if (options.ThrottlingTimeSpans is null || options.ThrottlingTimeSpans.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(options.ThrottlingTimeSpans));

        if (options.Capacity is not null && options.Capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(options.Capacity));

        return options;
    }
}
