using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.DebounceQueue;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal sealed class MessageBusPublisher : IPublisher
{
    private readonly IServiceProvider serviceProvider;

    public MessageBusPublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        dynamic handler = this;

        if (message is IAccumulatorQueueMessage)
        {
            handler.HandleAccumulatorQueueMessage<TMessage>(message);
            return;
        }

        if (message is IQueueMessage)
        {
            handler.HandleQueueMessage<TMessage>(message);
            return;
        }

        if (message is IStackMessage)
        {
            handler.HandleStackMessage<TMessage>(message);
            return;
        }

        if (message is IDebounceQueueMessage)
        {
            handler.HandleDebounceQueueMessage<TMessage>(message);
            return;
        }

        if (message is IFireAndForgetMessage)
        {
            handler.HandleFireAndForget<TMessage>(message);
            return;
        }

        if (message is IBubblingNotificationMessage)
        {
            handler.HandleBubblingNotifications<TMessage>(message);
            return;
        }

        if (message is IParallelNotificationMessage)
        {
            handler.HandleParallelNotifications<TMessage>(message);
            return;
        }

        throw new NotSupportedException();
    }

    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message) =>
        await GetResponseAsync(message, CancellationToken.None);
    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken) =>
        await HandleResponseMessageAsync(message, cancellationToken);

    private void HandleAccumulatorQueueMessage<TMessage>(TMessage message)
        where TMessage : IAccumulatorQueueMessage
    {
        var services = serviceProvider.GetServices<IAccumulatorQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }

    private void HandleDebounceQueueMessage<TMessage>(TMessage message)
        where TMessage : IDebounceQueueMessage
    {
        var services = serviceProvider.GetServices<IDebounceQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }

    private void HandleQueueMessage<TMessage>(TMessage message)
        where TMessage : IQueueMessage
    {
        var services = serviceProvider.GetServices<QueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }

    private void HandleStackMessage<TMessage>(TMessage message)
        where TMessage : IStackMessage
    {
        var services = serviceProvider.GetServices<StackBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Push(message);
    }

    private async void HandleFireAndForget<TMessage>(TMessage message)
        where TMessage : IFireAndForgetMessage
    {
        var handlers = serviceProvider
            .GetServices<IFireAndForgetHandler<TMessage>>();

        await Task
            .WhenAll(handlers.Select(x => x.HandleAsync(message, default)));
    }

    private async void HandleBubblingNotifications<TMessage>(TMessage message)
        where TMessage : IBubblingNotificationMessage
    {
        var handlers = RequestTypes.Notification.DependencyInjection._bubblingHandlers[typeof(TMessage)]
            .Select(handlerType => serviceProvider.GetService(handlerType)!);
        foreach (var handler in handlers.Select(handler => handler! as IBaseBubblingNotification<TMessage>))
        {
            if (!await handler!.HandleAsync(message))
            {
                break;
            }
        }
    }

    private async void HandleParallelNotifications<TMessage>(TMessage message)
        where TMessage : IParallelNotificationMessage
    {
        var handlers = serviceProvider
            .GetServices<IParallelNotificationHandler<TMessage>>();

        await Task
            .WhenAll(handlers.Select(handler => handler!.Handle(message)));
    }

    private async Task<TResponse> HandleResponseMessageAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken)
    {
        RequestTypes.Response.DependencyInjection.responseHandlers.TryGetValue(message.GetType(),
            out var handler);

        return await ((BaseResponseHandlerWrapper<TResponse>)handler!)
            .HandleAsync(message, serviceProvider, cancellationToken);
    }
}
