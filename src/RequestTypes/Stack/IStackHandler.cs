namespace MediatorCore.RequestTypes.Stack;

public interface IStackHandler<TMessage>
    where TMessage : IStackMessage
{
    Task HandleAsync(TMessage message);

    Task? HandleExceptionAsync(TMessage message,
        Exception exception,
        int retries, Func<Task> retry);
}
