using MediatorCore.Infrastructure;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

public interface IThrottlingQueueOptions
{
    ThrottlingTimeSpan[] ThrottlingTimeSpans { get; }
}
