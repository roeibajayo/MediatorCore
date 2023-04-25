using MediatorCore.RequestTypes.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Tests.RequestTypes;

public class Request : BaseUnitTest
{
    [Fact]
    public async Task BasicAdd_ReturnNoErrorsAndDequeue()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;

        //Act
        await publisher.PublishAsync(new SimpleRequestMessage(1));

        //Assert
        if (ReceivedDebugs(logger, "SimpleRequestMessage: 1") == 1)
        {
            return;
        }
        if (ReceivedDebugs(logger, "SimpleRequestMessage2: 1") == 1)
        {
            return;
        }
        throw new Exception("No dequeue executed");
    }

    [Fact]
    public async Task HandleException_3Retries_ReturnNoErrors()
    {
        //Arrange
        var publisher = serviceProvider.GetService<IPublisher>()!;
        var logger = serviceProvider.GetService<ILogger>()!;

        //Act
        await publisher.PublishAsync(new ExceptionRequestMessage(1));

        //Assert
        if (ReceivedDebugs(logger, "ExceptionRequestMessageHandler") == 3)
        {
            return;
        }
        throw new Exception("No dequeue executed");
    }
}



public record SimpleRequestMessage(int Id) : IRequestMessage;
public class SimpleRequestMessageHandler : IRequestHandler<SimpleRequestMessage>
{
    public readonly ILogger logger;

    public SimpleRequestMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleRequestMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("SimpleRequestMessage: " + message.Id);
        return Task.CompletedTask;
    }

    public Task? HandleException(SimpleRequestMessage message, Exception exception, int reties,
        Func<Task> retry, CancellationToken cancellationToken)
    {
        return default;
    }
}
public class SimpleRequestMessageHandler2 : IRequestHandler<SimpleRequestMessage>
{
    public readonly ILogger logger;

    public SimpleRequestMessageHandler2(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(SimpleRequestMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("SimpleRequestMessage2: " + message.Id);
        return Task.CompletedTask;
    }

    public Task? HandleException(SimpleRequestMessage message, Exception exception, int reties,
        Func<Task> retry, CancellationToken cancellationToken)
    {
        return default;
    }
}

public record ExceptionRequestMessage(int Id) : IRequestMessage;
public class ExceptionRequestMessageHandler : IRequestHandler<ExceptionRequestMessage>
{
    public readonly ILogger logger;

    public ExceptionRequestMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(ExceptionRequestMessage message, CancellationToken cancellationToken)
    {
        throw new Exception("my text here");
    }

    public async Task HandleException(ExceptionRequestMessage message,
        Exception exception,
        int reties, Func<Task> retry,
        CancellationToken cancellationToken)
    {
        if (reties == 3)
            return;

        logger.LogDebug($"#{reties} ExceptionRequestMessageHandler: " + exception.Message);
        await retry();
    }
}