using MediatorCore.Exceptions;
using MediatorCore.RequestTypes.Response;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task<TResponse> HandleResponseMessageAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken)
    {
        MessageIsNullException.ThrowIfNull(message);

        DependencyInjection.responseHandlers.TryGetValue(message.GetType(),
            out var handler);

        NoRegisteredHandlerException.ThrowIfNull(handler, message.GetType());

        return await ((BaseResponseHandlerWrapper<TResponse>)handler)
            .HandleAsync(message, serviceProvider, cancellationToken);
    }
}
