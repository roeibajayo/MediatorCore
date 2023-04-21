using MediatorCore.Publisher;
using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class ParallelNotification : BaseUnitTest
{
    [Fact]
    public async Task PublishBubbleMessage_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;
        var id = "1_" + Guid.NewGuid();

        //Act
        publisher.Publish(new ParallelNotificationMessage(id));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "ParallelNotification1Message: " + id) == 1 &&
                ReceivedDebugs(logger, "ParallelNotification2Message: " + id) == 1)
            {
                return;
            }

            await Task.Delay(200);
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

    public async Task HandleAsync(ParallelNotificationMessage message)
    {
        await Task.Delay(1000);
        logger.LogDebug("ParallelNotification1Message: " + message.Id);
    }
}
public class ParallelNotification2Handler : IParallelNotificationHandler<ParallelNotificationMessage>
{
    public readonly ILogger logger;

    public ParallelNotification2Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public async Task HandleAsync(ParallelNotificationMessage message)
    {
        await Task.Delay(1000);
        logger.LogDebug("ParallelNotification2Message: " + message.Id);
    }
}