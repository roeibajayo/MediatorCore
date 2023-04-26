namespace MediatorCore.RequestTypes.Queue;

public interface IQueueHandler<TMessage>
    where TMessage : IQueueMessage
{
    Task HandleAsync(TMessage message);

    Task? HandleExceptionAsync(TMessage items,
        Exception exception,
        int retries, Func<Task> retry);
}
