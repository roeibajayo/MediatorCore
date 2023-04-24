using MediatorCore.RequestTypes.AccumulatorQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class AccumulatorQueue : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleAccumulatorQueueMessage(0));
        publisher.Publish(new SimpleAccumulatorQueueMessage(1));
        publisher.Publish(new SimpleAccumulatorQueueMessage(2));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleAccumulatorQueueMessage: 0") == 1 &&
                ReceivedDebugs(logger, "SimpleAccumulatorQueueMessage: 1") == 1 &&
                ReceivedDebugs(logger, "SimpleAccumulatorQueueMessage: 2") == 1 &&
                ReceivedDebugs(logger, "SimpleAccumulatorQueueCount: 3") == 1)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No dequeue executed");
    }
}

public class SimpleAccumulatorQueueOptions :
    IAccumulatorQueueOptions
{
    public int MsInterval => 500;
    public int? MaxItemsOnDequeue => null;
    public int? MaxItemsStored => null;
    public MaxItemsStoredBehaviors? MaxItemsBehavior => null;
}
public record SimpleAccumulatorQueueMessage(int Id) : IAccumulatorQueueMessage;
public class SimpleAccumulatorQueueMessageHandler :
    IAccumulatorQueueHandler<SimpleAccumulatorQueueMessage, SimpleAccumulatorQueueOptions>
{
    public readonly ILogger logger;

    public SimpleAccumulatorQueueMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(IEnumerable<SimpleAccumulatorQueueMessage> messages)
    {
        logger.LogDebug("SimpleAccumulatorQueueCount: " + messages.Count());

        foreach (var message in messages)
        {
            logger.LogDebug("SimpleAccumulatorQueueMessage: " + message.Id);
        }
        return Task.CompletedTask;
    }

    public Task? HandleException(IEnumerable<SimpleAccumulatorQueueMessage> items, Exception exception, int reties, Func<Task> retry)
    {
        throw new NotImplementedException();
    }
}