using MediatorCore.RequestTypes.DebounceQueue;

namespace MediatorCore.BackgroundServices;

internal class GarbageCollectionQueueOptions : IDebounceQueueOptions
{
    public int DebounceMs => 5000;
}
internal record GarbageCollectionQueueItem() : IDebounceQueueMessage;
internal class GarbageCollectionQueue : IDebounceQueueHandler<GarbageCollectionQueueItem, GarbageCollectionQueueOptions>
{
    public Task HandleAsync(GarbageCollectionQueueItem items)
    {
        for (int i = 0; i < 3; i++)
            GC.Collect();
        return Task.CompletedTask;
    }
}
