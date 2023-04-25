using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task HandleParallelNotificationMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IParallelNotificationMessage
    {
        var handlers = serviceProvider
            .GetServices<IParallelNotificationHandler<TMessage>>();

        await Task
            .WhenAll(handlers.Select(handler => handler!.Handle(message, cancellationToken)));
    }
}
