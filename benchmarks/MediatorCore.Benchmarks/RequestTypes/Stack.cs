namespace MediatorCore.Benchmarks.RequestTypes;

public record StackMessage(int Id) : IStackMessage;
public class StackHandler : IStackHandler<StackMessage>
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

