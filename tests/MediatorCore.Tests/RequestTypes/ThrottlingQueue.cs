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
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;

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
    public ThrottlingTimeSpan[] ThrottlingTimeSpans =>
        new[] { new ThrottlingTimeSpan(TimeSpan.FromMilliseconds(500), 1) };
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

    public Task HandleAsync(IEnumerable<SimpleThrottlingQueueMessage> items)
    {
        foreach (var message in items)
            logger.LogDebug($"SimpleThrottlingQueueMessage: " + message.Id);

        return Task.CompletedTask;
    }

    public Task? HandleException(IEnumerable<SimpleThrottlingQueueMessage> items, Exception exception, int retries, Func<Task> retry)
    {
        throw new NotImplementedException();
    }
}