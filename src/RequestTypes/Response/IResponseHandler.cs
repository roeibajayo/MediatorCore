namespace MediatorCore.RequestTypes.Response;

public interface IResponseHandler<TMessage, TResponse>
    where TMessage : IResponseMessage<TResponse>
{
    Task<TResponse> HandleAsync(TMessage message, CancellationToken cancellationToken);
}
