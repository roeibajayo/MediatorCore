namespace MediatorCore.RequestTypes.Notification;

public interface IBaseParallelNotificationHandler
{
    Task Handle(object message);
}
public interface IParallelNotificationHandler<TMessage> :
    IBaseParallelNotificationHandler
    where TMessage : IParallelNotificationMessage
{
    Task IBaseParallelNotificationHandler.Handle(object message) =>
        HandleAsync((TMessage)message);
    Task HandleAsync(TMessage message);
}
