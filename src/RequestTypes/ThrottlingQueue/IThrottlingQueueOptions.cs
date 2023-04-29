namespace MediatorCore;

public interface IThrottlingQueueOptions
{
    ThrottlingWindow[] ThrottlingTimeSpans { get; }
    int? MaxMessagesStored { get; }
    MaxMessagesStoredBehaviors? MaxMessagesStoredBehavior { get; }
}
