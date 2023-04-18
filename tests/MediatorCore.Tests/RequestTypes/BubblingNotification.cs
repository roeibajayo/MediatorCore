using MediatorCore.Publisher;
using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class BubblingNotification : BaseUnitTest
{
    [Fact]
    public async Task PublishBubbleMessage_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;
        var id = "1_" + Guid.NewGuid();

        //Act
        publisher.Publish(new BubblingNotificationMessage(id, true));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "BubblingNotification1Message: " + id) == 1 &&
                ReceivedDebugs(logger, "BubblingNotification2Message: " + id) == 1)
            {
                return;
            }

            await Task.Delay(200);
        }
        throw new Exception("No dequeue executed");
    }

    [Fact]
    public async Task PublishNotBubbleMessage_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;
        var id = "2_" + Guid.NewGuid();

        //Act
        publisher.Publish(new BubblingNotificationMessage(id, false));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "BubblingNotification1Message: " + id) == 1 &&
                ReceivedDebugs(logger, "BubblingNotification2Message: " + id) == 0)
            {
                return;
            }

            await Task.Delay(200);
        }
        throw new Exception("No dequeue executed");
    }
}

public class BubblingNotification1Options
    : IBubblingNotificationOptions
{
    public int Sort => 1;
}
public class BubblingNotification2Options
    : IBubblingNotificationOptions
{
    public int Sort => 2;
}
public record BubblingNotificationMessage(string Id, bool Bubble) : IBubblingNotificationMessage;
public class BubblingNotification1Handler : IBubblingNotificationHandler<BubblingNotificationMessage, BubblingNotification1Options>
{
    public readonly ILogger logger;

    public BubblingNotification1Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task<bool> HandleAsync(BubblingNotificationMessage message)
    {
        logger.LogDebug("BubblingNotification1Message: " + message.Id);
        return Task.FromResult(message.Bubble);
    }
}
public class BubblingNotification2Handler : IBubblingNotificationHandler<BubblingNotificationMessage, BubblingNotification2Options>
{
    public readonly ILogger logger;

    public BubblingNotification2Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task<bool> HandleAsync(BubblingNotificationMessage message)
    {
        logger.LogDebug("BubblingNotification2Message: " + message.Id);
        return Task.FromResult(true);
    }
}