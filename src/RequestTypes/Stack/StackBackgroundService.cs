using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.Stack;

internal sealed class StackBackgroundService<TMessage> :
    IHostedService
    where TMessage : IStackMessage
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ConcurrentStack<TMessage> stack = new();
    private readonly object locker = new();
    private bool running = false;

    public StackBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    internal void Push(TMessage message)
    {
        stack.Push(message);
        TryProcessItem();
    }

    internal async void TryProcessItem()
    {
        TMessage item;

        lock (locker)
        {
            if (running)
                return;

            if (!stack.TryPop(out item))
            {
                return;
            }

            running = true;
        }

        await ProcessItem(item);
    }

    private async Task ProcessItem(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IStackHandler<TMessage>>();
        await ProcessItem(handler!, 0, item);
    }

    private async Task ProcessItem(IStackHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(item, ex, retries,
                () => ProcessItem(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
        finally
        {
            lock (locker)
            {
                running = false;
            }

            TryProcessItem();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
