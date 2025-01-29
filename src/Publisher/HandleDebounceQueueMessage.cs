using MediatorCore.RequestTypes.DebounceQueue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private void HandleDebounceQueueMessage<TMessage>(TMessage message)
        where TMessage : IDebounceQueueMessage
    {
        var services = serviceProvider
            .GetServices<IDebounceQueueBackgroundService<TMessage>>();

        foreach (var service in services)
            service.Enqueue(message);
    }
}
