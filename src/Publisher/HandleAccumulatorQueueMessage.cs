using MediatorCore.RequestTypes.AccumulatorQueue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async ValueTask HandleAccumulatorQueueMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IAccumulatorQueueMessage
    {
        var services = serviceProvider.GetServices<IAccumulatorQueueBackgroundService<TMessage>>();

        foreach (var service in services)
            await service.EnqueueAsync(message, cancellationToken);
    }
}
