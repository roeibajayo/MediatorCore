using MediatorCore.Publisher;
using MediatorCore.RequestTypes.FireAndForget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class FireAndForget : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;

        //Act
        publisher.Publish(new SimpleFireAndForgetMessage(1));

        //Assert
        for (var i = 0; i < 10; i++)
        {
            if (ReceivedDebugs(logger, "SimpleFireAndForgetMessage: 1") == 1)
            {
                return;
            }
            if (ReceivedDebugs(logger, "SimpleFireAndForgetMessage2: 1") == 1)
            {
                return;
            }

            await Task.Delay(100);
        }
        throw new Exception("No dequeue executed");
    }
}

public record SimpleFireAndForgetMessage(int Id) : IFireAndForgetMessage;
public class SimpleFireAndForgetMessageHandler : IFireAndForgetHandler<SimpleFireAndForgetMessage>
{
    public readonly ILogger logger;

    public SimpleFireAndForgetMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleFireAndForgetMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("SimpleFireAndForgetMessage: " + message.Id);
        return Task.CompletedTask;
    }
}
public class SimpleFireAndForgetMessageHandler2 : IFireAndForgetHandler<SimpleFireAndForgetMessage>
{
    public readonly ILogger logger;

    public SimpleFireAndForgetMessageHandler2(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleFireAndForgetMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("SimpleFireAndForgetMessage2: " + message.Id);
        return Task.CompletedTask;
    }
}