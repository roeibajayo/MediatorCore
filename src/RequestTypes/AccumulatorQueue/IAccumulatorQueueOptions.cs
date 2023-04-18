namespace MediatorCore.RequestTypes.AccumulatorQueue;

public interface IAccumulatorQueueOptions
{
    int MsInterval { get; }
    int? MaxItemsOnDequeue { get; }
    int? MaxItemsStored { get; }
    MaxItemsStoredBehaviors? MaxItemsBehavior { get; }
}
