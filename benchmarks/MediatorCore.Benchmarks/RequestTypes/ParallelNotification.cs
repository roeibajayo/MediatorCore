using MediatorCore.RequestTypes.Notification;
using MediatR;
using System.Threading;

namespace MediatorCore.Benchmarks.RequestTypes;

public record SimpleParallelNotificationMessage(int Id) : 
    IParallelNotificationMessage,
    INotification;

public class SimpleParallelNotification1Handler : 
    IParallelNotificationHandler<SimpleParallelNotificationMessage>,
    INotificationHandler<SimpleParallelNotificationMessage>
{
    public async Task Handle(SimpleParallelNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    public async Task HandleAsync(SimpleParallelNotificationMessage message)
    {
        await Task.Delay(1000);
    }
}

public class SimpleParallelNotification2Handler : 
    IParallelNotificationHandler<SimpleParallelNotificationMessage>,
    INotificationHandler<SimpleParallelNotificationMessage>
{
    public async Task Handle(SimpleParallelNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }

    public async Task HandleAsync(SimpleParallelNotificationMessage message)
    {
        await Task.Delay(1000);
    }
}