﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.DebounceQueue;

internal interface IDebounceQueueBackgroundService<TMessage>
    where TMessage :
    IDebounceQueueMessage
{
    void Enqueue(TMessage item);
}
internal sealed class DebounceQueueBackgroundService<TMessage, TOptions>(IServiceScopeFactory serviceScopeFactory, TOptions options) :
    IHostedService,
    IDebounceQueueBackgroundService<TMessage>
    where TMessage : class, IDebounceQueueMessage
    where TOptions : IDebounceQueueOptions, new()
{
    private readonly int debounceMs = options.DebounceMs;
    private readonly object lockObject = new();
    private TMessage? lastItem;
    private CancellationTokenSource? cancellationTokenSource;

    public DebounceQueueBackgroundService(IServiceScopeFactory serviceScopeFactory) :
        this(serviceScopeFactory, GetOptions())
    {
    }

    public void Enqueue(TMessage item)
    {
        lock (lockObject)
        {
            lastItem = item;

            //cancel current waiting
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            //create new waiting task
            cancellationTokenSource = new CancellationTokenSource();
        }

        DelayTask(cancellationTokenSource.Token);
    }

    private async void DelayTask(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(debounceMs, cancellationToken);
        }
        catch (TaskCanceledException)
        {
        }

        lock (lockObject)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;

                if (lastItem is not null)
                    ProcessMessage(lastItem);
            }
        }
    }

    private async void ProcessMessage(TMessage item)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBaseDebounceQueue<TMessage>>();
        await DebounceQueueBackgroundService<TMessage, TOptions>.ProcessMessage(handler!, 0, item);
    }

    private static async Task ProcessMessage(IBaseDebounceQueue<TMessage> handler, int retries, TMessage item)
    {
        try
        {
            await handler!.HandleAsync(item);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleExceptionAsync(item, ex, retries,
                () => DebounceQueueBackgroundService<TMessage, TOptions>.ProcessMessage(handler, retries + 1, item));

            if (exceptionHandler is not null)
                await exceptionHandler;
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
