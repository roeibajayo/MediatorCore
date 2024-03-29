using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace MediatorCore.RequestTypes.Stack;

internal interface IStackBackgroundService<TMessage>
    where TMessage : IStackMessage
{
    ValueTask PushAsync(TMessage item, CancellationToken cancellationToken);
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

    public async ValueTask PushAsync(TMessage message, CancellationToken cancellationToken)
    {
        if (options.Capacity is not null)
        {
            var currentMessages = stack.Count;

            if (options.Capacity == currentMessages)
            {
                if (options.MaxCapacityBehavior is null ||
                    options.MaxCapacityBehavior == MaxCapacityBehaviors.Wait)
                {
                    while (!cancellationToken.IsCancellationRequested && stack.Count == options.Capacity)
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                }

                if (options.MaxCapacityBehavior == MaxCapacityBehaviors.DropMessage)
                    return;
            }
        }

        stack.Push(message);
        TryProcessMessage();
    }

    internal async void TryProcessMessage()
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

        await ProcessMessage(item);
    }

    private async Task ProcessMessage(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseStackHandler<TMessage>>();
        await ProcessMessage(handler!, 0, item);
    }

    private async Task ProcessMessage(IBaseStackHandler<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(item, ex, retries,
                () => ProcessMessage(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
        finally
        {
            lock (locker)
            {
                running = false;
            }

            TryProcessMessage();
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

        if (options.Capacity is not null && options.Capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(options.Capacity));

        return options;
    }
}
