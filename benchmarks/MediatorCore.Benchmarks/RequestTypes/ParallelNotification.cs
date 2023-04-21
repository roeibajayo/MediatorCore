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
        var result = notification.Id * 2;
        return Task.CompletedTask;
    }

    public Task HandleAsync(SimpleParallelNotificationMessage message)
    {
        var result = message.Id * 2; 
        return Task.CompletedTask;
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


public record LongRunningParallelNotificationMessage(int Id) :
    IParallelNotificationMessage,
    INotification;

public class LongRunningParallelNotification1Handler :
    IParallelNotificationHandler<LongRunningParallelNotificationMessage>,
    INotificationHandler<LongRunningParallelNotificationMessage>
{
    public async Task Handle(LongRunningParallelNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    public async Task HandleAsync(LongRunningParallelNotificationMessage message)
    {
        await Task.Delay(1000);
    }
}

public class LongRunningParallelNotification2Handler :
    IParallelNotificationHandler<LongRunningParallelNotificationMessage>,
    INotificationHandler<LongRunningParallelNotificationMessage>
{
    public async Task Handle(LongRunningParallelNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }

    public async Task HandleAsync(LongRunningParallelNotificationMessage message)
    {
        await Task.Delay(1000);
    }
}