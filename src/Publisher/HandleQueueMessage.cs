using MediatorCore.RequestTypes.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async ValueTask HandleQueueMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IQueueMessage
    {
        var services = serviceProvider.GetServices<IQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            await service.EnqueueAsync(message, cancellationToken);
    }
}
