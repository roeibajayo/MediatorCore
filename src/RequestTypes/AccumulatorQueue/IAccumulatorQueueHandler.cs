namespace MediatorCore.RequestTypes.AccumulatorQueue;

public interface IBaseAccumulatorQueue<TMessage>
    where TMessage : IAccumulatorQueueMessage
{
    Task HandleAsync(IEnumerable<TMessage> items);

    Task? HandleException(IEnumerable<TMessage> items,
        Exception exception,
        int reties, Func<Task> retry);
}

public interface IAccumulatorQueueHandler<TMessage, TOptions> :
    IBaseAccumulatorQueue<TMessage>
    where TMessage : IAccumulatorQueueMessage
    where TOptions : class, IAccumulatorQueueOptions, new()
{
}