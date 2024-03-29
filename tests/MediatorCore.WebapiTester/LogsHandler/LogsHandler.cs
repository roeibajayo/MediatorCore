namespace MediatorCore.WebapiTester.LogsHandler;
public class LogsHandler :
    IAccumulatorQueueHandler<LogMessage, LogsHandlerOptions>
{
    private readonly ILogger<LogsHandler> logger;

    public LogsHandler(ILogger<LogsHandler> logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(IEnumerable<LogMessage> messages)
    {
        foreach (var message in messages)
            logger.LogInformation(message.Message);
        return Task.CompletedTask;
    }

    public Task? HandleExceptionAsync(IEnumerable<LogMessage> messages, Exception exception, int retries, Func<Task> retry)
    {
        return default;
    }
}