using MediatorCore.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher(
    IServiceScopeFactory serviceScopeFactory,
    IServiceProvider serviceProvider)
    : IPublisher
{
    public void Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        _ = Task.Run(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var handler = new MessageBusPublisher(serviceScopeFactory, serviceProvider);
            await handler.PublishAsync(message, cancellationToken);
        }, cancellationToken);
    }

    public async Task<bool> TryPublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
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

        return found;
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        var found = await TryPublishAsync(message, cancellationToken);

        if (!found)
        {
            NoRegisteredHandlerException.Throw<TMessage>();
        }
    }

    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken = default) =>
        await HandleResponseMessageAsync(message, cancellationToken);

}
