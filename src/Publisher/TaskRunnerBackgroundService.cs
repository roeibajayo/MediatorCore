using MediatorCore.Infrastructure;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.Publisher;

internal record TaskRunnerMessage(object Message, CancellationToken CancellationToken) : IQueueMessage;
internal class TaskRunnerBackgroundService : IHostedService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly LockingQueue<TaskRunnerMessage> _queue = new();
    private readonly IDictionary<string, MethodInfo> _methodInfos = new Dictionary<string, MethodInfo>();
    internal static readonly IDictionary<Type, Type> _parallelHandlers = new Dictionary<Type, Type>();
    private bool running = true;

    public TaskRunnerBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;

        var handlers = GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.Name.StartsWith("Handle") && x.Name.EndsWith("Message"));

        foreach (var handler in handlers)
        {
            _methodInfos[handler.Name] = handler;
        }

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
            _ = Task.Run(async () =>
            {
                //todo: use dic instead for using a scope in each handler
                using var scope = serviceScopeFactory.CreateScope();
                var handlersType = _parallelHandlers[message.Message.GetType()];
                var handlers = scope.ServiceProvider
                    .GetServices(handlersType)
                    .Select(handler => handler as IBaseParallelNotificationHandler);
                var tasks = handlers
                    .Select(handler => handler!.Handle(message.Message));

                await Task
                    .WhenAll(tasks)
                    .ConfigureAwait(false);
            })
                .ConfigureAwait(false);
            return;
        }

        throw new NotSupportedException();
    }


    private void HandleFireAndForgetMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IFireAndForgetMessage
    {
        _ = Task.Run(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            //todo: use dic instead for using a scope in each handler
            var handlers = scope.ServiceProvider.GetServices<IFireAndForgetHandler<TMessage>>();
            foreach (var handler in handlers)
                await handler.HandleAsync(message, cancellationToken);
        });
    }

    private void HandleBubblingNotificationMessage<TMessage>(TMessage message)
        where TMessage : IBubblingNotificationMessage
    {
        _ = Task.Run(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
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