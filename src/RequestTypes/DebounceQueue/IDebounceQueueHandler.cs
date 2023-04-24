namespace MediatorCore.RequestTypes.DebounceQueue;

public interface IBaseDebounceQueue<TMessage>
    where TMessage : IDebounceQueueMessage
{
    Task HandleAsync(TMessage items);

    Task? HandleException(TMessage message,
        Exception exception,
        int reties, Func<Task> retry);
}

public interface IDebounceQueueHandler<TMessage, TOptions> :
    IBaseDebounceQueue<TMessage>
    where TMessage : IDebounceQueueMessage
    where TOptions : class, IDebounceQueueOptions, new()
{
}