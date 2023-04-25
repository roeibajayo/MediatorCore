using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task HandleBubblingNotificationMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IBubblingNotificationMessage
    {
        var handlers = serviceProvider.GetServices<IBaseBubblingNotification<TMessage>>();
        foreach (var handler in handlers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (!await handler!.HandleAsync(message, cancellationToken))
            {
                break;
            }
        }
    }
}
