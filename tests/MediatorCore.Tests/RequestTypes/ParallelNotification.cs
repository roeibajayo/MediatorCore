using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class ParallelNotification : BaseUnitTest
{
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task PublishBubbleMessage_ReturnNoErrorsAndDequeue(int counts)
    {
        //Arrange
        var publisher = ServiceProvider.GetService<IPublisher>()!;
        var logger = ServiceProvider.GetService<ILogger>()!;
        var id = "1_" + Guid.NewGuid();

        //Act
        for (var i = 0; i < counts; i++)
        {
            await publisher.PublishAsync(new ParallelNotificationMessage(id));
        }

        //Assert
        if (ReceivedDebugs(logger, "ParallelNotification1Message: " + id) == counts &&
            ReceivedDebugs(logger, "ParallelNotification2Message: " + id) == counts)
        {
            return;
        }
        throw new Exception("No dequeue executed");
    }
}

public record ParallelNotificationMessage(string Id) : IParallelNotificationMessage;
public class ParallelNotification1Handler : IParallelNotificationHandler<ParallelNotificationMessage>
{
    public readonly ILogger logger;

    public ParallelNotification1Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(ParallelNotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("ParallelNotification1Message: " + message.Id);
        return Task.CompletedTask;
    }
}
public class ParallelNotification2Handler : IParallelNotificationHandler<ParallelNotificationMessage>
{
    public readonly ILogger logger;

    public ParallelNotification2Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(ParallelNotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("ParallelNotification2Message: " + message.Id);
        return Task.CompletedTask;
    }
}