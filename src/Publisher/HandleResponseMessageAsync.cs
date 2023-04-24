using MediatorCore.RequestTypes.Response;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task<TResponse> HandleResponseMessageAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken)
    {
        RequestTypes.Response.DependencyInjection.responseHandlers.TryGetValue(message.GetType(),
            out var handler);

        return await ((BaseResponseHandlerWrapper<TResponse>)handler!)
            .HandleAsync(message, serviceProvider, cancellationToken);
    }
}
