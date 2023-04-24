using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async void HandleParallelNotificationMessage<TMessage>(TMessage message)
        where TMessage : IParallelNotificationMessage
    {
        var handlers = serviceProvider
            .GetServices<IParallelNotificationHandler<TMessage>>();

        await Task
            .WhenAll(handlers.Select(handler => handler!.Handle(message)));
    }
}
