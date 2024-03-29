using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async ValueTask HandleStackMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IStackMessage
    {
        var services = serviceProvider.GetServices<IStackBackgroundService<TMessage>>();
        foreach (var service in services)
            await service.PushAsync(message, cancellationToken);
    }
}
