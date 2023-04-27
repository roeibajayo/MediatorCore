using MediatorCore.RequestTypes.ThrottlingQueue;

namespace MediatorCore.RequestTypes.Stack;

public interface IStackOptions
{
    int? MaxMessagesStored { get; }
    MaxMessagesStoredBehaviors? MaxMessagesStoredBehavior { get; }
}
