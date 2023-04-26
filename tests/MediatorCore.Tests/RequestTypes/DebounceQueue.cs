using MediatorCore.RequestTypes.DebounceQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class DebounceQueue : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = ServiceProvider.GetService<IPublisher>()!;
        var logger = ServiceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleDebounceQueueMessage(0));
        await Task.Delay(200);
        publisher.Publish(new SimpleDebounceQueueMessage(1));
        await Task.Delay(200);
        publisher.Publish(new SimpleDebounceQueueMessage(2));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleDebounceQueueMessage: 0") == 0 &&
                ReceivedDebugs(logger, "SimpleDebounceQueueMessage: 1") == 0 &&
                ReceivedDebugs(logger, "SimpleDebounceQueueMessage: 2") == 1)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No dequeue executed");
    }
}

public class SimpleDebounceQueueOptions :
    IDebounceQueueOptions
{
    public int DebounceMs => 300;
}
public record SimpleDebounceQueueMessage(int Id) : IDebounceQueueMessage;
public class SimpleDebounceQueueMessageHandler :
    IDebounceQueueHandler<SimpleDebounceQueueMessage, SimpleDebounceQueueOptions>
{
    public readonly ILogger logger;

    public SimpleDebounceQueueMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleDebounceQueueMessage message)
    {
        logger.LogDebug("SimpleDebounceQueueMessage: " + message.Id);
        return Task.CompletedTask;
    }

    public Task? HandleExceptionAsync(SimpleDebounceQueueMessage message, Exception exception, int retries, Func<Task> retry)
    {
        throw new NotImplementedException();
    }
}