using MediatorCore.Publisher;
using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class ParallelNotification : BaseUnitTest
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(10_000)]
    public async Task PublishBubbleMessage_ReturnNoErrorsAndDequeue(int counts)
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;
        var id = "1_" + Guid.NewGuid();

        //Act
        for (var i = 0; i < counts; i++)
        {
            publisher.Publish(new ParallelNotificationMessage(id));
        }

        //Assert
        for (var i = 0; i < 20; i++)
        {
            if (ReceivedDebugs(logger, "ParallelNotification1Message: " + id) == counts &&
                ReceivedDebugs(logger, "ParallelNotification2Message: " + id) == counts)
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