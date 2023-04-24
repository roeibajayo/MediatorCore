using MediatorCore.RequestTypes.FireAndForget;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async void HandleFireAndForgetMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IFireAndForgetMessage
    {
        var handlers = serviceProvider
            .GetServices<IFireAndForgetHandler<TMessage>>();

        await Task.WhenAll(
            handlers.Select(handler => HandleFireAndForgetMessage(handler, 0, message, cancellationToken)));
    }

    private async Task HandleFireAndForgetMessage<TMessage>(IFireAndForgetHandler<TMessage> handler,
        int retries, TMessage message, CancellationToken cancellationToken)
        where TMessage : IFireAndForgetMessage
    {
        try
        {
            await handler!.HandleAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleException(message, ex, retries,
                () => HandleFireAndForgetMessage(handler, retries + 1, message, cancellationToken),
                cancellationToken);

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }
}
