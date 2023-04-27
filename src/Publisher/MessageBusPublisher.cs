using MediatorCore.Exceptions;
using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.DebounceQueue;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Request;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using MediatorCore.RequestTypes.ThrottlingQueue;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private readonly IServiceProvider serviceProvider;

    public MessageBusPublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        dynamic handler = this;
        var found = false;

        if (message is IAccumulatorQueueMessage)
        {
            handler.HandleAccumulatorQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IQueueMessage)
        {
            handler.HandleQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IStackMessage)
        {
            handler.HandleStackMessage<TMessage>(message);
            found = true;
        }

        if (message is IDebounceQueueMessage)
        {
            handler.HandleDebounceQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IRequestMessage)
        {
            handler.HandleRequestMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IBubblingNotificationMessage)
        {
            handler.HandleBubblingNotificationMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IParallelNotificationMessage)
        {
            handler.HandleParallelNotificationMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IThrottlingQueueMessage)
        {
            handler.HandleThrottlingQueueMessage<TMessage>(message);
            found = true;
        }

        if (!found)
        {
            NoRegisteredHandlerException.Throw<TMessage>();
        }
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        dynamic handler = this;
        var found = false;

        if (message is IAccumulatorQueueMessage)
        {
            handler.HandleAccumulatorQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IQueueMessage)
        {
            handler.HandleQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IStackMessage)
        {
            handler.HandleStackMessage<TMessage>(message);
            found = true;
        }

        if (message is IDebounceQueueMessage)
        {
            handler.HandleDebounceQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IRequestMessage)
        {
            await handler.HandleRequestMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IBubblingNotificationMessage)
        {
            await handler.HandleBubblingNotificationMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IParallelNotificationMessage)
        {
            await handler.HandleParallelNotificationMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IThrottlingQueueMessage)
        {
            handler.HandleThrottlingQueueMessage<TMessage>(message);
            found = true;
        }

        if (!found)
        {
            NoRegisteredHandlerException.Throw<TMessage>();
        }
    }

    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken = default) =>
        await HandleResponseMessageAsync(message, cancellationToken);

}
