using MediatorCore.RequestTypes.Request;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task HandleRequestMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IRequestMessage
    {
        var handlers = serviceProvider
            .GetServices<IRequestHandler<TMessage>>();

        await Task.WhenAll(
            handlers.Select(handler => HandleRequestMessage(handler, 0, message, cancellationToken)));
    }

    private async Task HandleRequestMessage<TMessage>(IRequestHandler<TMessage> handler,
        int retries, TMessage message, CancellationToken cancellationToken)
        where TMessage : IRequestMessage
    {
        try
        {
            await handler!.HandleAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            var exceptionHandler = handler!.HandleException(message, ex, retries,
                () => HandleRequestMessage(handler, retries + 1, message, cancellationToken),
                cancellationToken);

            if (exceptionHandler is not null)
                await exceptionHandler;
        }
    }
}
