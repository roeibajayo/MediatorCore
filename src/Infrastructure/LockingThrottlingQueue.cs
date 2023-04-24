using System.Collections.Concurrent;

namespace MediatorCore.Infrastructure;

/// <summary>
/// ThrottlingQueue that represents a queue that enforces a throttling policy on the items it holds. 
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class LockingThrottlingQueue<T> : IDisposable
{
    private readonly ThrottlingTimeSpan[] dateParts;

    private bool running = true;
    private readonly ConcurrentQueue<T> queue = new();
    private readonly Queue<DateTimeOffset> executes = new();
    private SemaphoreSlim waitingLocker;
    private DateTimeOffset? waitingUntil;

    private readonly object locker = new();

    internal LockingThrottlingQueue(ThrottlingTimeSpan[] dateParts)
    {
        if (dateParts == null || dateParts.Length == 0)
            throw new ArgumentOutOfRangeException(null, nameof(dateParts));

        foreach (var unit in dateParts)
            if (unit == null || unit.TimeSpan.TotalSeconds <= 0 || unit.Executes <= 0)
                throw new ArgumentOutOfRangeException(null, nameof(dateParts));

        this.dateParts = dateParts;

        waitingLocker = new SemaphoreSlim(1);
        waitingLocker.Wait();
    }

    /// <summary>
    /// This method adds a single item to the queue.
    /// </summary>
    /// <param name="item"></param>
    internal void Enqueue(T item)
    {
        queue.Enqueue(item);

        ReleaseIfRunning();
    }
    /// <summary>
    /// This method adds multiple items to the queue at once.
    /// </summary>
    /// <param name="items"></param>
    internal void Enqueue(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            queue.Enqueue(item);
        }

        ReleaseIfRunning();
    }
    internal async Task<(bool Success, IEnumerable<T> Items)> TryDequeueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            waitingUntil = null;

            if (!AnyInQueue())
            {
                //waiting for first items
                await WaitForNextLoopAsync(cancellationToken);
            }

            var dequeuedItems = TryDequeue();
            var dequeuedItemsCount = dequeuedItems.Count();


            if (dequeuedItemsCount == 0)
            {
                var now = DateTimeOffset.Now;

                if (!AnyInQueue())
                    continue; //released not in time, maybe async task released the running

                //cant dequeue, max items
                var next = GetNextExecution();
                if (next != null)
                {
                    var until = next.Value - now;
                    if (waitingUntil == null || next > waitingUntil)
                    {
                        waitingUntil = next;
                    }
                    else
                    {
                        await WaitForNextLoopAsync(cancellationToken);
                        continue;
                    }

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(until + TimeSpan.FromMilliseconds(20), cancellationToken);

                        if (!cancellationToken.IsCancellationRequested)
                            waitingLocker.Release();
                    });

                    await WaitForNextLoopAsync(cancellationToken);
                }
            }
            else
            {
                if (!cancellationToken.IsCancellationRequested)
                    return (true, dequeuedItems);
            }
        }

        return (false, Enumerable.Empty<T>());
    }
    /// <summary>
    /// Removes all items from the queue and clears the history of items that have been dequeued.
    /// </summary>
    internal void Clear()
    {
        queue.Clear();
        executes.Clear();
    }

    private int CountCurrentDequeue(ThrottlingTimeSpan unit)
    {
        var from = unit.GetLastStart();
        return CountDequeue(from);
    }
    private async Task WaitForNextLoopAsync(CancellationToken cancellationToken)
    {
        if (waitingLocker.CurrentCount == 1)
            waitingLocker.Wait(); //reset count

        await waitingLocker.WaitAsync(cancellationToken);
    }
    private void ReleaseIfRunning()
    {
        if (!running || waitingUntil is not null)
            return;

        waitingLocker.Release();
    }
    private DateTimeOffset? GetNextExecution()
    {
        var now = DateTimeOffset.Now;
        var lastExecuted = GetLastExecute();

        var shortestDate = dateParts
            .Where(x => (lastExecuted != null || x.Fixed) && // ignore not fixed if not last execute
                (x.Fixed ? x.GetLastStart(now) : lastExecuted) + x.TimeSpan > now)
            .MinBy(x => x.Fixed ?
                (now - x.GetLastStart(now) + x.TimeSpan).TotalMilliseconds :
                x.TimeSpan.TotalMilliseconds);

        DateTimeOffset? next = null;
        if (shortestDate!.Fixed)
        {
            next = shortestDate.GetLastStart(now) + shortestDate.TimeSpan;
        }
        else if (lastExecuted != null)
        {
            next = lastExecuted + shortestDate.TimeSpan;
        }
        return next;
    }
    private int CountDequeue(DateTimeOffset from)
    {
        return dateParts.Length == 1 ?
            executes.Count : //because all archived executes already removed
            executes.Count(x => x > from);
    }
    private List<T> DequeueItems(int count)
    {
        var items = new List<T>(count);
        for (var i = 0; i < count; i++)
        {
            if (!queue.TryDequeue(out var item))
                break;
            items.Add(item);
        }
        return items;
    }
    /// <summary>
    /// This method attempts to dequeue items from the queue according to the throttling policy.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<T> TryDequeue()
    {
        ClearOldCompletedFromQueue();
        var items = TryDequeueItems();
        var now = DateTimeOffset.Now;
        var count = items.Count();
        for (var i = 0; i < count; i++)
            executes.Enqueue(now);
        return items;
    }
    private IEnumerable<T> TryDequeueItems()
    {
        var completed = -1;
        var count = -1;
        foreach (var unit in dateParts)
        {
            var unitCompleted = CountCurrentDequeue(unit);
            completed += unitCompleted;
            var unitCount = unit.Executes - unitCompleted;

            if (count == -1 || unitCount < count)
            {
                count = unitCount;
            }
        }

        var items = count <= 0 ?
            Enumerable.Empty<T>() :
            DequeueItems(count);
        return items;
    }
    private bool AnyInQueue() =>
        !queue.IsEmpty;
    private DateTimeOffset? GetLastExecute() =>
        executes.Count == 0 ? null : executes.Peek();
    private void ClearOldCompletedFromQueue()
    {
        lock (locker)
        {
            var now = DateTimeOffset.Now;
            var removeBefore = dateParts.Min(unit => unit.GetLastStart(now));
            while (executes.Count != 0 && removeBefore > executes.Peek())
            {
                executes.Dequeue();
            }
        }
    }


    public void Dispose()
    {
        Clear();
        running = false;
        waitingLocker.Release();
        waitingLocker.Dispose();
    }
}
