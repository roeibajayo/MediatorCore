using MediatR;

namespace MediatorCore.Benchmarks.RequestTypes;

public record SimpleNotificationMessage(int Id) :
    INotificationMessage,
    INotification;

public class SimpleNotification1Handler :
    INotificationHandler<SimpleNotificationMessage>,
    MediatR.INotificationHandler<SimpleNotificationMessage>
{
    public Task Handle(SimpleNotificationMessage notification, CancellationToken cancellationToken)
    {
        var result = notification.Id * 2;
        return Task.CompletedTask;
    }

    public Task HandleAsync(SimpleNotificationMessage message, CancellationToken cancellationToken)
    {
        var result = message.Id * 2; 
        return Task.CompletedTask;
    }
}

public class SimpleNotification2Handler :
    INotificationHandler<SimpleNotificationMessage>,
    MediatR.INotificationHandler<SimpleNotificationMessage>
{
    public async Task Handle(SimpleNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }

    public async Task HandleAsync(SimpleNotificationMessage message, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }
}


public record LongRunningNotificationMessage(int Id) :
    INotificationMessage,
    INotification;

public class LongRunningNotification1Handler :
    INotificationHandler<LongRunningNotificationMessage>,
    MediatR.INotificationHandler<LongRunningNotificationMessage>
{
    public async Task Handle(LongRunningNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    public async Task HandleAsync(LongRunningNotificationMessage message, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }
}

public class LongRunningNotification2Handler :
    INotificationHandler<LongRunningNotificationMessage>, 
    MediatR.INotificationHandler<LongRunningNotificationMessage>
{
    public async Task Handle(LongRunningNotificationMessage notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }

    public async Task HandleAsync(LongRunningNotificationMessage message, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
    }
}