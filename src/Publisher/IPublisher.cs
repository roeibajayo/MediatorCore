﻿using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;

namespace MediatorCore.Publisher
{
    public interface IPublisher
    {
        Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message);
        Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
            CancellationToken cancellationToken);

        void Publish<T>(T message) =>
            Publish(message, CancellationToken.None);
        void Publish<T>(T message, CancellationToken cancellationToken);

        //MediatR support
        Task<TResponse> Send<TResponse>(IResponseMessage<TResponse> message) =>
            GetResponseAsync(message);
        Task<TResponse> Send<TResponse>(IResponseMessage<TResponse> message,
            CancellationToken cancellationToken) =>
            GetResponseAsync(message, cancellationToken);
    }
}