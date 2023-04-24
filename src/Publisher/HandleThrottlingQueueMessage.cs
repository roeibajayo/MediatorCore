using MediatorCore.RequestTypes.ThrottlingQueue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private void HandleThrottlingQueueMessage<TMessage>(TMessage message)
        where TMessage : IThrottlingQueueMessage
    {
        var services = serviceProvider.GetServices<IThrottlingQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }
}
