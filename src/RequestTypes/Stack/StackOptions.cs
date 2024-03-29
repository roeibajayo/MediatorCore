namespace MediatorCore;
public abstract record StackOptions(int? Capacity = null, MaxCapacityBehaviors MaxCapacityBehavior = MaxCapacityBehaviors.Wait);
