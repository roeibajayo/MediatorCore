using MediatorCore.Exceptions;
using MediatorCore.RequestTypes.ThrottlingQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.Stack;

internal interface IStackBackgroundService<TMessage>
    where TMessage : IStackMessage
{
    void Push(TMessage item);
}
internal sealed class StackBackgroundService<TMessage, TOptions> :
    IStackBackgroundService<TMessage>,
    IHostedService
    where TMessage : IStackMessage
    where TOptions : IStackOptions, new()
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly TOptions options;
    private readonly ConcurrentStack<TMessage> stack = new();
    private readonly object locker = new();
    private bool running = false;

    public StackBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }
    public StackBackgroundService(IServiceScopeFactory serviceScopeFactory, TOptions options)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.options = options;
    }

    public void Push(TMessage message)
    {
        if (options.MaxMessagesStored is not null)
        {
            var currentItems = stack.Count;

            if (options.MaxMessagesStored == currentItems)
            {
                if (options.MaxMessagesStoredBehavior is null ||
                    options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.ThrowExceptionOnEnqueue)
                    MaxItemsOnQueueException.Throw();

                if (options.MaxMessagesStoredBehavior == MaxMessagesStoredBehaviors.DiscardEnqueues)
                    return;
            }
        }

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
        var handler = scope.ServiceProvider.GetService<IBaseStackHandler<TMessage>>();
        await ProcessItem(handler!, 0, item);
    }

    private async Task ProcessItem(IBaseStackHandler<TMessage> handler, int retries, TMessage item)
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

    private static TOptions GetOptions()
    {
        var options = Activator.CreateInstance<TOptions>();
        return options;
    }
}
