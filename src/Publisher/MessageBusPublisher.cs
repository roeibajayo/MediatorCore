﻿using MediatorCore.Exceptions;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private readonly IServiceProvider serviceProvider;

    public MessageBusPublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public bool TryPublish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
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

        if (message is INotificationMessage)
        {
            handler.HandleNotificationMessage<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IThrottlingQueueMessage)
        {
            handler.HandleThrottlingQueueMessage<TMessage>(message);
            found = true;
        }

        return found;
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

        if (message is INotificationMessage)
        {
            await handler.HandleNotificationMessage<TMessage>(message, cancellationToken);
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
