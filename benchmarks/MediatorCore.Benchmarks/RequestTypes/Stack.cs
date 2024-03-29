namespace MediatorCore.Benchmarks.RequestTypes;

public class StackOptions : IStackOptions
{
    public int? Capacity => default;

    public MaxCapacityBehaviors? MaxCapacityBehavior => default;
}

public record StackMessage(int Id) : IStackMessage;
public class StackHandler : IStackHandler<StackMessage, StackOptions>
{
    public async Task HandleAsync(StackMessage message)
    {
        await Task.Delay(30);
    }

    public Task? HandleExceptionAsync(StackMessage message, Exception exception, int retries, Func<Task> retry)
    {
        return default;
    }
}

