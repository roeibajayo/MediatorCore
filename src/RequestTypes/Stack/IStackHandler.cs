namespace MediatorCore.RequestTypes.Stack;

public interface IStackHandler<TMessage>
    where TMessage : IStackMessage
{
    Task HandleAsync(TMessage message);
}
