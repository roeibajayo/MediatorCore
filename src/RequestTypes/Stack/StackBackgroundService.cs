using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.Stack;

internal sealed class StackBackgroundService<TMessage> :
    BackgroundService
    where TMessage : IStackMessage
{
    private readonly IServiceProvider serviceProvider;
    private readonly LockingStack<TMessage> queue = new();

    public StackBackgroundService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    internal void Push(TMessage message)
    {
        queue.Push(message);
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messageResult = await queue.TryPopAsync(cancellationToken);

            if (!messageResult.Success)
                continue;

            await using var scope = serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetService(typeof(IStackHandler<TMessage>))
                as IStackHandler<TMessage>;
            await handler.HandleAsync(messageResult.Item);
        }
    }
}
