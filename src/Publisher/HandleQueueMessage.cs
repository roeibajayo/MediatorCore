using MediatorCore.RequestTypes.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private void HandleQueueMessage<TMessage>(TMessage message)
        where TMessage : IQueueMessage
    {
        var services = serviceProvider.GetServices<IQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }
}
