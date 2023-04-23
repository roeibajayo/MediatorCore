namespace MediatorCore.RequestTypes.FireAndForget;

public interface IFireAndForgetHandler<TMessage>
    where TMessage : IFireAndForgetMessage
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
