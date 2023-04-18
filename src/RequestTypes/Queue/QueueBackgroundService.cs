﻿using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.Queue;

internal sealed class QueueBackgroundService<TMessage> : 
    BackgroundService 
    where TMessage : IQueueMessage
{
    private readonly IServiceProvider serviceProvider;
    private readonly LockingQueue<TMessage> queue = new();

    public QueueBackgroundService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    internal void Enqueue(TMessage message)
    {
        queue.Enqueue(message);
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messageResult = await queue.TryDequeueAsync(cancellationToken);

            if (!messageResult.Success)
                continue;

            await using var scope = serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetService(typeof(IQueueHandler<TMessage>))
                as IQueueHandler<TMessage>;
            await handler.HandleAsync(messageResult.Item);
        }
    }
}
