namespace MediatorCore;

public interface IQueueOptions
{
    int? MaxMessagesStored { get; }
    MaxMessagesStoredBehaviors? MaxMessagesStoredBehavior { get; }
}
