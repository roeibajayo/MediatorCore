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
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleQueueMessage(1));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleQueue: 1") > 0)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No dequeue executed");
    }
}

public record SimpleQueueMessage(int Id) : IQueueMessage;
public class SimpleQueueMessageHandler : IQueueHandler<SimpleQueueMessage>
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
}