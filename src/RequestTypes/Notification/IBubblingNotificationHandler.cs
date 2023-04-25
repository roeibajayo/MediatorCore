namespace MediatorCore.RequestTypes.Notification;

public interface IBaseBubblingNotification<TMessage>
    where TMessage : IBubblingNotificationMessage
{
    Task<bool> HandleAsync(TMessage message, CancellationToken cancellationToken);
}

public interface IBubblingNotificationHandler<TMessage, TBubblingNotificationOptions> :
    IBaseBubblingNotification<TMessage>
    where TMessage : IBubblingNotificationMessage
    where TBubblingNotificationOptions : IBubblingNotificationOptions, new()
{

}
