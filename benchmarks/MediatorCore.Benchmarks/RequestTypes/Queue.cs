using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.ThrottlingQueue;

namespace MediatorCore.Benchmarks.RequestTypes;

public class QueueOptions : IQueueOptions
{
    public int? Capacity => default;

    public MaxCapacityBehaviors? MaxCapacityBehavior => default;
}

public record QueueMessage(int Id) : IQueueMessage;
public class QueueHandler : IQueueHandler<QueueMessage, QueueOptions>
{
    public async Task HandleAsync(QueueMessage message)
    {
        await Task.Delay(30);
    }

    public Task? HandleExceptionAsync(QueueMessage messages, Exception exception, int retries, Func<Task> retry)
    {
        return default;
    }
}

