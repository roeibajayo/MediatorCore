using MediatorCore.Exceptions;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher(IServiceProvider serviceProvider) : IPublisher
{
    public bool TryPublish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        dynamic handler = this;
        var found = false;

        if (message is IAccumulatorQueueMessage)
        {
            handler.HandleAccumulatorQueueMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IQueueMessage)
        {
            handler.HandleQueueMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IStackMessage)
        {
            handler.HandleStackMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IDebounceQueueMessage)
        {
            handler.HandleDebounceQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IRequestMessage)
        {
            handler.HandleRequestMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IBubblingNotificationMessage)
        {
            handler.HandleBubblingNotificationMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is INotificationMessage)
        {
            handler.HandleNotificationMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IThrottlingQueueMessage)
        {
            handler.HandleThrottlingQueueMessageAsync<TMessage>(message, cancellationToken);
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
            await handler.HandleAccumulatorQueueMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IQueueMessage)
        {
            await handler.HandleQueueMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IStackMessage)
        {
            await handler.HandleStackMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IDebounceQueueMessage)
        {
            handler.HandleDebounceQueueMessage<TMessage>(message);
            found = true;
        }

        if (message is IRequestMessage)
        {
            await handler.HandleRequestMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IBubblingNotificationMessage)
        {
            await handler.HandleBubblingNotificationMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is INotificationMessage)
        {
            await handler.HandleNotificationMessageAsync<TMessage>(message, cancellationToken);
            found = true;
        }

        if (message is IThrottlingQueueMessage)
        {
            await handler.HandleThrottlingQueueMessageAsync<TMessage>(message, cancellationToken);
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
