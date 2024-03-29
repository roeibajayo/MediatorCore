using MediatorCore.RequestTypes.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class Queue : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = ServiceProvider.GetService<IPublisher>()!;
        var logger = ServiceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleQueueMessage(1));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleQueue: 1") == 1)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No dequeue executed");
    }
}

public class SimpleQueueHandlerOptions : IQueueOptions
{
    public int? Capacity => default;

    public MaxCapacityBehaviors? MaxCapacityBehavior => default;
}

public record SimpleQueueMessage(int Id) : IQueueMessage;
public class SimpleQueueMessageHandler : IQueueHandler<SimpleQueueMessage, SimpleQueueHandlerOptions>
{
    public readonly ILogger logger;

    public SimpleQueueMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleQueueMessage message)
    {
        logger.LogDebug("SimpleQueue: " + message.Id);
        return Task.CompletedTask;
    }

    public Task? HandleExceptionAsync(SimpleQueueMessage message, Exception exception, int retries, Func<Task> retry)
    {
        throw new NotImplementedException();
    }
}