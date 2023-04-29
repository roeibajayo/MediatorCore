using MediatorCore.RequestTypes.Notification;

namespace MediatorCore.RequestTypes.Notification
{
    public interface IBaseNotificationHandler
    {
        Task Handle(object? message, CancellationToken cancellationToken);
    }
}

namespace MediatorCore
{
    public interface INotificationHandler<TMessage> :
        IBaseNotificationHandler
        where TMessage : INotificationMessage
    {
        Task IBaseNotificationHandler.Handle(object? message, CancellationToken cancellationToken) =>
            HandleAsync(message is null ? default : (TMessage)message, cancellationToken);
        Task HandleAsync(TMessage? message, CancellationToken cancellationToken);
    }
}