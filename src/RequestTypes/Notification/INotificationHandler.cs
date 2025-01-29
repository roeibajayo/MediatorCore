using MediatorCore.RequestTypes.Notification;

namespace MediatorCore;

public interface INotificationHandler<TMessage> :
    IBaseNotificationHandler
    where TMessage : INotificationMessage
{
    Task IBaseNotificationHandler.Handle(object? message, CancellationToken cancellationToken)
    {
        if (message is null)
            return Task.CompletedTask;

        return HandleAsync((TMessage)message, cancellationToken);
    }

    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}