using System.Collections.Concurrent;

namespace MediatorCore.Infrastructure;

/// <summary>
/// Thread-safe FIFO queue that lock the TryDequeue method if no elements in the queue
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class LockingQueue<T> : ConcurrentQueue<T>, IDisposable
{
    private readonly SemaphoreSlim waitingLocker;
    private bool running = true;

    internal LockingQueue()
    {
        waitingLocker = new SemaphoreSlim(1);
        waitingLocker.Wait();
    }

    public void Dispose()
    {
        running = false;
        waitingLocker.Release();
    }

    internal void Enqueue(IEnumerable<T> items)
    {
        foreach (var item in items)
            base.Enqueue(item);

        waitingLocker.Release();
    }
    internal new void Enqueue(T item)
    {
        base.Enqueue(item);

        waitingLocker.Release();
    }

    internal new bool TryDequeue(out T? item)
    {
        var result = TryDequeueAsync(CancellationToken.None).Result;
        item = result.Item;
        return result.Success;
    }
    internal async Task<(bool Success, T? Item)> TryDequeueAsync(CancellationToken cancellationToken)
    {
        while (running)
        {
            if (base.TryDequeue(out var item))
            {
                return (true, item);
            }

            await waitingLocker.WaitAsync(cancellationToken);
        }

        return (false, default);
    }

    ~LockingQueue()
    {
        Dispose();
    }
}