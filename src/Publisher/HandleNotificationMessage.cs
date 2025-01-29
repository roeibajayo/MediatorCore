using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private async Task HandleNotificationMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : INotificationMessage
    {
        var handlers = serviceProvider
            .GetServices<INotificationHandler<TMessage>>();

        await Task
            .WhenAll(handlers.Select(handler => handler!.Handle(message, cancellationToken)))
            .WaitAsync(cancellationToken);
    }
}
