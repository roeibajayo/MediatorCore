namespace MediatorCore;

public interface IQueueOptions
{
    int? Capacity { get; }
    MaxCapacityBehaviors? MaxCapacityBehavior { get; }
}
