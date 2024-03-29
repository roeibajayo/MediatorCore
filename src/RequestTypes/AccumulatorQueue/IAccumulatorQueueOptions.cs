namespace MediatorCore;

public interface IAccumulatorQueueOptions
{
    int MsInterval { get; }
    int? AccumulationCapacity { get; }
    int? TotalCapacity { get; }
    MaxCapacityBehaviors? MaxTotalCapacityBehavior { get; }
}
