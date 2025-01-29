namespace MediatorCore.RequestTypes.Notification;

public interface IBaseNotificationHandler
{
    Task Handle(object? message, CancellationToken cancellationToken);
}
