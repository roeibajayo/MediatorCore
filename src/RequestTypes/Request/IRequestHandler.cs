namespace MediatorCore.RequestTypes.Request;

public interface IRequestHandler<TMessage>
    where TMessage : IRequestMessage
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);

    Task? HandleExceptionAsync(TMessage message,
        Exception exception,
        int retries, Func<Task> retry,
        CancellationToken cancellationToken);
}
