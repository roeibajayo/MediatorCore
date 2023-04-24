namespace MediatorCore.RequestTypes.FireAndForget;

public interface IFireAndForgetHandler<TMessage>
    where TMessage : IFireAndForgetMessage
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);

    Task? HandleException(TMessage message,
        Exception exception,
        int reties, Func<Task> retry,
        CancellationToken cancellationToken);
}
