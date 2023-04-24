namespace MediatorCore.RequestTypes.ThrottlingQueue;

public interface IBaseThrottlingQueue<TMessage>
    where TMessage : IThrottlingQueueMessage
{
    Task HandleAsync(IEnumerable<TMessage> items);

    Task? HandleException(IEnumerable<TMessage> items,
        Exception exception,
        int reties, Func<Task> retry);
}

public interface IThrottlingQueueHandler<TMessage, TOptions> :
    IBaseThrottlingQueue<TMessage>
    where TMessage : IThrottlingQueueMessage
    where TOptions : class, IThrottlingQueueOptions, new()
{
}