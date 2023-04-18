namespace MediatorCore.RequestTypes.Notification;

public interface IParallelNotificationHandler<TMessage> 
    where TMessage : IParallelNotificationMessage
{
    Task HandleAsync(TMessage message);
}
