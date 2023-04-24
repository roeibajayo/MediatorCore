using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async void HandleBubblingNotificationMessage<TMessage>(TMessage message)
        where TMessage : IBubblingNotificationMessage
    {
        var handlers = serviceProvider.GetServices<IBaseBubblingNotification<TMessage>>();
        foreach (var handler in handlers)
        {
            if (!await handler!.HandleAsync(message))
            {
                break;
            }
        }
    }
}
