namespace MediatorCore;

public abstract record AccumulatorQueueOptions(int MsInterval,
    int? AccumulationCapacity = null,
    int? TotalCapacity = null,
    MaxCapacityBehaviors MaxTotalCapacityBehavior = MaxCapacityBehaviors.Wait);