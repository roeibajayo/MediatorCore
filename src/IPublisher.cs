using MediatorCore.RequestTypes.Response;

namespace MediatorCore
{
    public interface IPublisher
    {
        /// <summary>
        /// Asynchronously send a message to a single response handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="message">Request message</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send a message to multiple handlers
        /// </summary>
        /// <param name="message">Request message</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        void Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send a message to multiple handlers and wait for all handlers to complete
        /// </summary>
        /// <param name="message">Request message</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
    }
}