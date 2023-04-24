using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.DebounceQueue;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using MediatorCore.RequestTypes.ThrottlingQueue;

namespace MediatorCore.Publisher;

internal partial class MessageBusPublisher : IPublisher
{
    private readonly IServiceProvider serviceProvider;

    public MessageBusPublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        dynamic handler = this;

        switch (message)
        {
            case IAccumulatorQueueMessage:
                handler.HandleAccumulatorQueueMessage<TMessage>(message);
                break;
            case IQueueMessage:
                handler.HandleQueueMessage<TMessage>(message);
                break;
            case IStackMessage:
                handler.HandleStackMessage<TMessage>(message);
                break;
            case IDebounceQueueMessage:
                handler.HandleDebounceQueueMessage<TMessage>(message);
                break;
            case IFireAndForgetMessage:
                handler.HandleFireAndForgetMessage<TMessage>(message, cancellationToken);
                break;
            case IBubblingNotificationMessage:
                handler.HandleBubblingNotificationMessage<TMessage>(message);
                break;
            case IParallelNotificationMessage:
                handler.HandleParallelNotificationMessage<TMessage>(message);
                break;
            case IThrottlingQueueMessage:
                handler.HandleThrottlingQueueMessage<TMessage>(message);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken = default) =>
        await HandleResponseMessageAsync(message, cancellationToken);

}
