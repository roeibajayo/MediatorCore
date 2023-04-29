using MediatorCore.RequestTypes.AccumulatorQueue;

namespace MediatorCore.RequestTypes.AccumulatorQueue
{
    public interface IBaseAccumulatorQueue<TMessage>
        where TMessage : IAccumulatorQueueMessage
    {
        Task HandleAsync(IEnumerable<TMessage> messages);

        Task? HandleExceptionAsync(IEnumerable<TMessage> messages,
            Exception exception,
            int retries, Func<Task> retry);
    }
}

namespace MediatorCore
{
    public interface IAccumulatorQueueHandler<TMessage, TOptions> :
        IBaseAccumulatorQueue<TMessage>
        where TMessage : IAccumulatorQueueMessage
        where TOptions : class, IAccumulatorQueueOptions, new()
    {
    }
}