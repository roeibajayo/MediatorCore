﻿namespace MediatorCore;

public interface IAccumulatorQueueOptions
{
    int MsInterval { get; }
    int? MaxMessagesOnDequeue { get; }
    int? MaxMessagesStored { get; }
    MaxMessagesStoredBehaviors? MaxMessagesStoredBehavior { get; }
}
