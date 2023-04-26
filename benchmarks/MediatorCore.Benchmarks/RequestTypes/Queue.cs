using MediatorCore.RequestTypes.Queue;

namespace MediatorCore.Benchmarks.RequestTypes;

public record QueueMessage(int Id) : IQueueMessage;
public class QueueHandler : IQueueHandler<QueueMessage>
{
    public async Task HandleAsync(QueueMessage message)
    {
        await Task.Delay(30);
    }

    public Task? HandleException(QueueMessage message, Exception exception, int retries, Func<Task> retry)
    {
        return default;
    }
}

