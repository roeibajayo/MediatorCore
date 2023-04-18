using MediatorCore.RequestTypes.Notification;
using MediatR;

namespace MediatorCore.Benchmarks.RequestTypes;

public record SimpleParallelNotificationMessage(int Id) : 
    IParallelNotificationMessage,
    INotification;

public class SimpleParallelNotification1Handler : 
    IParallelNotificationHandler<SimpleParallelNotificationMessage>,
    INotificationHandler<SimpleParallelNotificationMessage>
{
    public Task Handle(SimpleParallelNotificationMessage notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task HandleAsync(SimpleParallelNotificationMessage message)
    {
        return Task.CompletedTask;
    }
}

public class SimpleParallelNotification2Handler : 
    IParallelNotificationHandler<SimpleParallelNotificationMessage>,
    INotificationHandler<SimpleParallelNotificationMessage>
{
    public Task Handle(SimpleParallelNotificationMessage notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task HandleAsync(SimpleParallelNotificationMessage message)
    {
        return Task.CompletedTask;
    }
}