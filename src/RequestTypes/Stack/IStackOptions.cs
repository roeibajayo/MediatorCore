namespace MediatorCore;

public interface IStackOptions
{
    int? Capacity { get; }
    MaxCapacityBehaviors? MaxCapacityBehavior { get; }
}
