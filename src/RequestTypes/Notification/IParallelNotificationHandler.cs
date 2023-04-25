namespace MediatorCore.RequestTypes.Notification;

public interface IBaseParallelNotificationHandler
{
    Task Handle(object? message, CancellationToken cancellationToken);
}
public interface IParallelNotificationHandler<TMessage> :
    IBaseParallelNotificationHandler
    where TMessage : IParallelNotificationMessage
{
    Task IBaseParallelNotificationHandler.Handle(object? message, CancellationToken cancellationToken) =>
        HandleAsync(message is null ? default : (TMessage)message, cancellationToken);
    Task HandleAsync(TMessage? message, CancellationToken cancellationToken);
}
