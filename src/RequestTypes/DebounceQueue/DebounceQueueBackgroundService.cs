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
    private readonly IServiceProvider serviceProvider;
    private readonly LockingDebounceQueue<TMessage> queue;
    private bool running = true;

    public DebounceQueueBackgroundService(IServiceProvider serviceProvider) :
        this(serviceProvider, GetOptions())
    {
    }

    public DebounceQueueBackgroundService(IServiceProvider serviceProvider, TOptions options)
    {
        this.serviceProvider = serviceProvider;
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

            _ = Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetService(typeof(IBaseDebounceQueue<TMessage>))
                    as IBaseDebounceQueue<TMessage>;
                await handler.HandleAsync(messageResult.Item);
            })
                .ConfigureAwait(false);
        }
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
