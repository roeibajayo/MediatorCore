﻿using MediatorCore.RequestTypes.Stack;

namespace MediatorCore.RequestTypes.Stack
{
    public interface IBaseStackHandler<TMessage>
        where TMessage : IStackMessage
    {
        Task HandleAsync(TMessage message);

        Task? HandleExceptionAsync(TMessage message,
            Exception exception,
            int retries, Func<Task> retry);
    }
}

namespace MediatorCore
{
    public interface IStackHandler<TMessage, TOptions> :
        IBaseStackHandler<TMessage>
        where TMessage : IStackMessage
        where TOptions : StackOptions, new()
    {
    }

    public interface IStackHandler<TMessage> : IStackHandler<TMessage, DefaultStackOptions>
        where TMessage : IStackMessage
    {
    }
}