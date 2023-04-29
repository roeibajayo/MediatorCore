using MediatorCore.RequestTypes.BubblingNotification;

namespace MediatorCore.RequestTypes.BubblingNotification
{
    public interface IBaseBubblingNotification<TMessage>
        where TMessage : IBubblingNotificationMessage
    {
        /// <returns>Return false to stop bubbling, or true to continue bubbling</returns>
        Task<bool> HandleAsync(TMessage message, CancellationToken cancellationToken);
    }
}

namespace MediatorCore
{
    public interface IBubblingNotificationHandler<TMessage, TBubblingNotificationOptions> :
        IBaseBubblingNotification<TMessage>
        where TMessage : IBubblingNotificationMessage
        where TBubblingNotificationOptions : IBubblingNotificationOptions, new()
    {
    }
}
