using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class Stack : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDeStack()
    {
        //Arrange
        var publisher = ServiceProvider.GetService<IPublisher>()!;
        var logger = ServiceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleStackMessage(1));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleStack: 1") > 0)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No pop executed");
    }
}

public class SimpleStackHandlerOptions : IStackOptions
{
    public int? Capacity => default;

    public MaxCapacityBehaviors? MaxCapacityBehavior => default;
}

public record SimpleStackMessage(int Id) : IStackMessage;
public class SimpleStackMessageHandler : IStackHandler<SimpleStackMessage, SimpleStackHandlerOptions>
{
    public readonly ILogger logger;

    public SimpleStackMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleStackMessage message)
    {
        logger.LogDebug("SimpleStack: " + message.Id);
        return Task.CompletedTask;
    }

    public Task? HandleExceptionAsync(SimpleStackMessage message, Exception exception, int retries, Func<Task> retry)
    {
        throw new NotImplementedException();
    }
}