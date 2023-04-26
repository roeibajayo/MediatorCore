using MediatorCore.RequestTypes.Response;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task<TResponse> HandleResponseMessageAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken)
    {
        if (message is null)
        {
            throw new ArgumentNullException("message for Response handler can't be null");
        }

        DependencyInjection.responseHandlers.TryGetValue(message.GetType(),
            out var handler);

        if (handler is null)
        {
            throw new ArgumentNullException("no registered handler for request " + message.GetType().Name);
        }

        return await ((BaseResponseHandlerWrapper<TResponse>)handler)
            .HandleAsync(message, serviceProvider, cancellationToken);
    }
}
