using MediatorCore.RequestTypes.Response;

namespace MediatorCore
{
    public interface IPublisher
    {
        Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
            CancellationToken cancellationToken = default);

        void Publish<T>(T message, CancellationToken cancellationToken = default);
    }
}