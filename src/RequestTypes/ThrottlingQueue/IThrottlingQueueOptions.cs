namespace MediatorCore;

public interface IThrottlingQueueOptions
{
    ThrottlingWindow[] ThrottlingTimeSpans { get; }
    int? Capacity { get; }
    MaxCapacityBehaviors? MaxCapacityBehavior { get; }
}
