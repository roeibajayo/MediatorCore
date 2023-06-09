﻿using MediatorCore.RequestTypes.Queue;

namespace MediatorCore.RequestTypes.Queue
{
    public interface IBaseQueueHandler<TMessage>
        where TMessage : IQueueMessage
    {
        Task HandleAsync(TMessage message);

        Task? HandleExceptionAsync(TMessage messages,
            Exception exception,
            int retries, Func<Task> retry);
    }
}

namespace MediatorCore
{
    public interface IQueueHandler<TMessage, TOptions> :
        IBaseQueueHandler<TMessage>
        where TMessage : IQueueMessage
        where TOptions : IQueueOptions, new()
    {
    }
}