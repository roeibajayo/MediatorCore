using System.Collections.Concurrent;

namespace MediatorCore.Infrastructure;

/// <summary>
/// Thread-safe LIFO queue that lock the TryPop method if no elements in the queue
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class LockingStack<T> : ConcurrentStack<T>
{
    private readonly SemaphoreSlim waitingLocker;

    internal LockingStack()
    {
        waitingLocker = new SemaphoreSlim(1);
        waitingLocker.Wait();
    }

    internal void Push(IEnumerable<T> items)
    {
        foreach (var item in items)
            base.Push(item);

        waitingLocker.Release();
    }
    internal new void Push(T item)
    {
        base.Push(item);

        waitingLocker.Release();
    }

    internal new bool TryPop(out T item)
    {
        var result = TryPopAsync(CancellationToken.None).Result;
        item = result.Item;
        return result.Success;
    }
    internal async Task<(bool Success, T Item)> TryPopAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (base.TryPop(out var item))
            {
                return (true, item);
            }

            await waitingLocker.WaitAsync(cancellationToken);
        }
    }
}
