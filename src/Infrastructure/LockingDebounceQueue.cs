namespace MediatorCore.Infrastructure;

/// <summary>
/// Thread-safe LIFO debounce queue that lock the TryPop method if no elements in the queue
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class LockingDebounceQueue<T> : IDisposable
    where T : class
{
    private readonly int ms;
    private readonly SemaphoreSlim waitingLocker;
    private T? lastItem = null;
    private bool running = true;
    private Task? delayTask = null;
    private readonly object lockObject = new();

    internal LockingDebounceQueue(int ms)
    {
        waitingLocker = new SemaphoreSlim(1);
        waitingLocker.Wait();
        this.ms = ms;
    }

    public void Dispose()
    {
        running = false;

        waitingLocker.Release();
    }

    internal void Enqueue(T item)
    {
        lastItem = item;

        lock (lockObject)
        {
            delayTask ??= Task.Delay(ms)
                    .ContinueWith((t) => waitingLocker.Release());
        }
    }

    internal async Task<(bool Success, T? Item)> TryDequeueAsync(CancellationToken cancellationToken)
    {
        while (running)
        {
            lock (lockObject)
            {
                if (lastItem is not null)
                {
                    var result = lastItem;
                    lastItem = null;
                    delayTask = null;
                    return (true, result);
                }
            }

            await waitingLocker.WaitAsync(cancellationToken);
        }

        return (false, default);
    }

    ~LockingDebounceQueue()
    {
        Dispose();
    }
}