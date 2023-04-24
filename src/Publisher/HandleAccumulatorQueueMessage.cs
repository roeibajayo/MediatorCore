using MediatorCore.RequestTypes.AccumulatorQueue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private void HandleAccumulatorQueueMessage<TMessage>(TMessage message)
        where TMessage : IAccumulatorQueueMessage
    {
        var services = serviceProvider.GetServices<IAccumulatorQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }
}
