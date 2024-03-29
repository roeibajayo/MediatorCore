using MediatorCore.RequestTypes.ThrottlingQueue;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async ValueTask HandleThrottlingQueueMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IThrottlingQueueMessage
    {
        var services = serviceProvider.GetServices<IThrottlingQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            await service.EnqueueAsync(message, cancellationToken);
    }
}
