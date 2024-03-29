namespace MediatorCore;
public record QueueOptions(int? Capacity = null, MaxCapacityBehaviors MaxCapacityBehavior = MaxCapacityBehaviors.Wait);
