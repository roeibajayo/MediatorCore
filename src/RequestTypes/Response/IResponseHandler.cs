﻿namespace MediatorCore.RequestTypes.Response;

public interface IResponseHandler<TMessage, TResponse>
    where TMessage : notnull, IResponseMessage<TResponse>
{
    Task<TResponse> HandleAsync(TMessage message, CancellationToken cancellationToken);
}
