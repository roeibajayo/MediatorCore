namespace MediatorCore.RequestTypes.ThrottlingQueue;

public interface IBaseThrottlingQueue<TMessage>
    where TMessage : IThrottlingQueueMessage
{
    Task HandleAsync(IEnumerable<TMessage> items);

    Task? HandleExceptionAsync(IEnumerable<TMessage> items,
        Exception exception,
        int retries, Func<Task> retry);
}

public interface IThrottlingQueueHandler<TMessage, TOptions> :
    IBaseThrottlingQueue<TMessage>
    where TMessage : IThrottlingQueueMessage
    where TOptions : class, IThrottlingQueueOptions, new()
{
}