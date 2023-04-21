using MediatorCore.BackgroundServices;
using MediatorCore.Infrastructure;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.Publisher;

internal record TaskRunnerMessage(object Message, CancellationToken CancellationToken) : IQueueMessage;
internal class TaskRunnerBackgroundService : IHostedService, IDisposable
{
    private readonly IServiceProvider serviceProvider;
    private readonly LockingQueue<TaskRunnerMessage> _queue = new();
    private readonly IDictionary<string, MethodInfo> _methodInfos = new Dictionary<string, MethodInfo>();
    private readonly IPublisher? publisher;
    private readonly GarbageCollectionQueueItem garbageCollectionQueueItem = new();
    private bool running = true;

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

        publisher = serviceProvider.GetService<IPublisher>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && running)
        {
            var result = await _queue.TryDequeueAsync(cancellationToken);
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
        Task.Factory.StartNew(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            //todo: use dic instead for using a scope in each handler
            var handlers = scope.ServiceProvider.GetServices<IFireAndForgetHandler<TMessage>>();
            foreach (var handler in handlers)
                await handler.HandleAsync(message, cancellationToken);
        });
    }

    private void HandleBubblingNotificationMessage<TMessage>(TMessage message)
        where TMessage : IBubblingNotificationMessage
    {
        Task.Factory.StartNew(async () =>
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
        Task.Factory.StartNew(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            //todo: use dic instead for using a scope in each handler
            var handlers = scope.ServiceProvider
                .GetServices(typeof(IParallelNotificationHandler<TMessage>));
            await Task.WhenAll(handlers
                .Select(handler => (handler as IParallelNotificationHandler<TMessage>)!.HandleAsync(message)));
        })
            .ContinueWith((t) => publisher!.Publish(garbageCollectionQueueItem))
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        running = false;
    }
}