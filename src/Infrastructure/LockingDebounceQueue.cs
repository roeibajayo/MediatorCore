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
    private CancellationTokenSource? cancellationTokenSource;
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

        waitingLocker?.Release();
    }

    internal void Enqueue(T item)
    {
        lock (lockObject)
        {
            lastItem = item;
        }

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();
        DelayTask(cancellationTokenSource.Token);
    }

    private async void DelayTask(CancellationToken cancellationToken)
    {
        await Task.Delay(ms, cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
        {
            waitingLocker.Release();
            cancellationTokenSource?.Dispose();
        }
    }

    internal async Task<(bool Success, T? Item)> TryDequeueAsync(CancellationToken cancellationToken)
    {
        while (running)
        {
            if (lastItem is not null)
            {
                lock (lockObject)
                {
                    var result = lastItem;
                    lastItem = null;
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