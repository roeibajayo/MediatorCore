using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

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

            ProcessItem(messageResult.Item);
        }
    }

    private async void ProcessItem(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IStackHandler<TMessage>>();
        await ProcessItem(handler, 0, item);
    }
    private async Task ProcessItem(IStackHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleException(item, ex, retries,
                () => ProcessItem(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        stack.Dispose();
        return Task.CompletedTask;
    }
}
