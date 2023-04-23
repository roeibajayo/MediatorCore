using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.DebounceQueue;

internal interface IDebounceQueueBackgroundService<TMessage> where TMessage : IDebounceQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class DebounceQueueBackgroundService<TMessage, TOptions> :
    IHostedService,
    IDebounceQueueBackgroundService<TMessage>
    where TMessage : class, IDebounceQueueMessage
    where TOptions : class, IDebounceQueueOptions
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly LockingDebounceQueue<TMessage> queue;
    private bool running = true;

    public DebounceQueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }

    public DebounceQueueBackgroundService(IServiceScopeFactory servicScopeFactory, TOptions options)
    {
        this.serviceScopeFactory = servicScopeFactory;
        queue = new LockingDebounceQueue<TMessage>(options.DebounceMs);
    }

    public void Enqueue(TMessage message)
    {
        queue.Enqueue(message);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && running)
        {
            var messageResult = await queue.TryDequeueAsync(cancellationToken);

            if (!messageResult.Success)
                continue;

            ProcessItem(messageResult.Item);
        }
    }

    private async void ProcessItem(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseDebounceQueue<TMessage>>();
        await handler.HandleAsync(item);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        queue.Dispose();
        return Task.CompletedTask;
    }

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();
        return options;
    }

}
