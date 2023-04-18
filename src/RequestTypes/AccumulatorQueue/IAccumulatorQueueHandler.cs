namespace MediatorCore.RequestTypes.AccumulatorQueue;

public interface IBaseAccumulatorQueue<TMessage>
    where TMessage : IAccumulatorQueueMessage
{
    Task HandleAsync(IEnumerable<TMessage> items);
}

public interface IAccumulatorQueueHandler<TMessage, TOptions> :
    IBaseAccumulatorQueue<TMessage>
    where TMessage : IAccumulatorQueueMessage
    where TOptions : class, IAccumulatorQueueOptions, new()
{
}