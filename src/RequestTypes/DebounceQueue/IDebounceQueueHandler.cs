﻿using MediatorCore.RequestTypes.DebounceQueue;

namespace MediatorCore.RequestTypes.DebounceQueue
{
    public interface IBaseDebounceQueue<TMessage>
        where TMessage : IDebounceQueueMessage
    {
        Task HandleAsync(TMessage messages);

        Task? HandleExceptionAsync(TMessage item,
            Exception exception,
            int retries, Func<Task> retry);
    }
}

namespace MediatorCore
{
    public interface IDebounceQueueHandler<TMessage, TOptions> :
        IBaseDebounceQueue<TMessage>
        where TMessage : IDebounceQueueMessage
        where TOptions : class, IDebounceQueueOptions, new()
    {
    }
}