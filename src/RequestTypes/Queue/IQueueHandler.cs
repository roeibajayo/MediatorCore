namespace MediatorCore.RequestTypes.Queue;

public interface IQueueHandler<TMessage>
    where TMessage : IQueueMessage
{
    Task HandleAsync(TMessage message);
}
