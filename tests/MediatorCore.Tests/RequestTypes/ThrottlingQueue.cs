using MediatorCore.RequestTypes.ThrottlingQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class ThrottlingQueue : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = ServiceProvider.GetService<IPublisher>()!;
        var logger = ServiceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleThrottlingQueueMessage(0));
        publisher.Publish(new SimpleThrottlingQueueMessage(1));
        publisher.Publish(new SimpleThrottlingQueueMessage(2));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 0") == 1 &&
                ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 1") == 0 &&
                ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 2") == 0)
            {
                break;
            }

            await Task.Delay(100);
        }
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 0") == 1 &&
                ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 1") == 1 &&
                ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 2") == 0)
            {
                break;
            }

            await Task.Delay(100);
        }
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 0") == 1 &&
                ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 1") == 1 &&
                ReceivedDebugs(logger, "SimpleThrottlingQueueMessage: 2") == 1)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No dequeue executed");
    }
}

public class SimpleThrottlingQueueOptions :
    IThrottlingQueueOptions
{
    public ThrottlingWindow[] ThrottlingTimeSpans =>
        new[] { new ThrottlingWindow(TimeSpan.FromMilliseconds(500), 1) };

    public int? Capacity => default;

    public MaxCapacityBehaviors? MaxCapacityBehavior => default;
}
public record SimpleThrottlingQueueMessage(int Id) : IThrottlingQueueMessage;
public class SimpleThrottlingQueueMessageHandler :
    IThrottlingQueueHandler<SimpleThrottlingQueueMessage, SimpleThrottlingQueueOptions>
{
    public readonly ILogger logger;

    public SimpleThrottlingQueueMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(IEnumerable<SimpleThrottlingQueueMessage> messages)
    {
        foreach (var message in messages)
            logger.LogDebug($"SimpleThrottlingQueueMessage: " + message.Id);

        return Task.CompletedTask;
    }

    public Task? HandleExceptionAsync(IEnumerable<SimpleThrottlingQueueMessage> messages, Exception exception, int retries, Func<Task> retry)
    {
        throw new NotImplementedException();
    }
}