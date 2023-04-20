﻿using MediatorCore.Infrastructure;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.Publisher;

internal record TaskRunnerMessage(object Message, CancellationToken CancellationToken) : IQueueMessage;
internal class TaskRunnerBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly LockingQueue<TaskRunnerMessage> _queue = new();
    private readonly IDictionary<string, MethodInfo> _methodInfos = new Dictionary<string, MethodInfo>();

    public TaskRunnerBackgroundService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;

        var handlers = GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.Name.StartsWith("Handle") && x.Name.EndsWith("Message"));

        foreach (var handler in handlers)
        {
            _methodInfos[handler.Name] = handler;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await _queue.TryDequeueAsync(stoppingToken);
            if (result.Success)
            {
                Handle(result.Item);
            }
        }
    }

    internal void Enqueue(TaskRunnerMessage message) =>
        _queue.Enqueue(message);

    private void Handle(TaskRunnerMessage message)
    {
        if (message.Message is IFireAndForgetMessage)
        {
            var method = _methodInfos["HandleFireAndForgetMessage"]
                .MakeGenericMethod(message.Message.GetType());
            method.Invoke(this, new[] { message.Message, message.CancellationToken });
            return;
        }

        if (message.Message is IBubblingNotificationMessage)
        {
            var method = _methodInfos["HandleBubblingNotificationMessage"]
                .MakeGenericMethod(message.Message.GetType());
            method.Invoke(this, new[] { message.Message });
            return;
        }

        if (message.Message is IParallelNotificationMessage)
        {
            var method = _methodInfos["HandleParallelNotificationMessage"]
                .MakeGenericMethod(message.Message.GetType());
            method.Invoke(this, new[] { message.Message });
            return;
        }

        throw new NotSupportedException();
    }


    private void HandleFireAndForgetMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IFireAndForgetMessage
    {
        Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IFireAndForgetHandler<TMessage>>();
            foreach (var handler in handlers)
                await handler.HandleAsync(message, cancellationToken);
        });
    }

    private void HandleBubblingNotificationMessage<TMessage>(TMessage message)
        where TMessage : IBubblingNotificationMessage
    {
        Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var handlers = RequestTypes.Notification.DependencyInjection._bubblingHandlers[typeof(TMessage)]
                .Select(handlerType => scope.ServiceProvider.GetService(handlerType)!);
            foreach (var handler in handlers.Select(handler => handler! as IBaseBubblingNotification<TMessage>))
            {
                if (!await handler!.HandleAsync(message))
                {
                    break;
                }
            }
        });
    }

    private void HandleParallelNotificationMessage<TMessage>(TMessage message)
        where TMessage : IParallelNotificationMessage
    {
        Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider
                .GetServices(typeof(IParallelNotificationHandler<TMessage>));
            await Task.WhenAll(handlers
                .Select(handler => (handler as IParallelNotificationHandler<TMessage>)!.HandleAsync(message)));
        });
    }
}
