using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private void HandleStackMessage<TMessage>(TMessage message)
        where TMessage : IStackMessage
    {
        var services = serviceProvider.GetServices<IStackBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Push(message);
    }
}
