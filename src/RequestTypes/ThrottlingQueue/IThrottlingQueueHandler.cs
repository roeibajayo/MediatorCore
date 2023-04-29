using MediatorCore.RequestTypes.ThrottlingQueue;

namespace MediatorCore.RequestTypes.ThrottlingQueue
{
    public interface IBaseThrottlingQueueHandler<TMessage>
        where TMessage : IThrottlingQueueMessage
    {
        Task HandleAsync(IEnumerable<TMessage> messages);

        Task? HandleExceptionAsync(IEnumerable<TMessage> messages,
            Exception exception,
            int retries, Func<Task> retry);
    }
}

namespace MediatorCore
{
    public interface IThrottlingQueueHandler<TMessage, TOptions> :
        IBaseThrottlingQueueHandler<TMessage>
        where TMessage : IThrottlingQueueMessage
        where TOptions : class, IThrottlingQueueOptions, new()
    {
    }
}