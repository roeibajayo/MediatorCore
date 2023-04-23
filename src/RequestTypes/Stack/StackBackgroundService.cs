using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.Stack;

internal sealed class StackBackgroundService<TMessage> :
    IHostedService
    where TMessage : IStackMessage
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly LockingStack<TMessage> stack = new();
    private bool running = true;

    public StackBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    internal void Push(TMessage message)
    {
        stack.Push(message);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && running)
        {
            var messageResult = await stack.TryPopAsync(cancellationToken);

            if (!messageResult.Success)
                continue;

            _ = Task.Run(async () =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetService(typeof(IStackHandler<TMessage>))
                    as IStackHandler<TMessage>;
                await handler.HandleAsync(messageResult.Item);
            })
                .ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        stack.Dispose();
        return Task.CompletedTask;
    }
}
