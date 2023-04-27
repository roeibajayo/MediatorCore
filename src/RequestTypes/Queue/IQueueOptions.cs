using MediatorCore.RequestTypes.ThrottlingQueue;

namespace MediatorCore.RequestTypes.Queue;

public interface IQueueOptions
{
    int? MaxMessagesStored { get; }
    MaxMessagesStoredBehaviors? MaxMessagesStoredBehavior { get; }
}
