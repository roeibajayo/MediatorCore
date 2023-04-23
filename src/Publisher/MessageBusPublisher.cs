using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.DebounceQueue;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

internal sealed class MessageBusPublisher : IPublisher
{
    private readonly IServiceProvider serviceProvider;

    public MessageBusPublisher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        if (message is IAccumulatorQueueMessage)
        {
            dynamic handler = this;
            handler.HandleAccumulatorQueueMessage<TMessage>(message);
            return;
        }

        if (message is IQueueMessage)
        {
            dynamic handler = this;
            handler.HandleQueueMessage<TMessage>(message);
            return;
        }

        if (message is IStackMessage)
        {
            dynamic handler = this;
            handler.HandleStackMessage<TMessage>(message);
            return;
        }

        if (message is IDebounceQueueMessage)
        {
            dynamic handler = this;
            handler.HandleDebounceQueueMessage<TMessage>(message);
            return;
        }

        if (message is IFireAndForgetMessage ||
            message is IBubblingNotificationMessage)
        {
            var taskRunnerMessage = new TaskRunnerMessage(message, cancellationToken);
            var service = serviceProvider.GetService<TaskRunnerBackgroundService>()!;
            service.Enqueue(taskRunnerMessage);
            return;
        }

        if (message is IParallelNotificationMessage)
        {
            ExecuteParallelHandlers(message);
            return;
        }

        throw new NotSupportedException();
    }

    private async void ExecuteParallelHandlers<TMessage>(TMessage message)
    {
        //todo: use dic instead for using a scope in each handler
        using var scope = serviceProvider.CreateScope();
        var handlersType = TaskRunnerBackgroundService._parallelHandlers[typeof(TMessage)];
        var handlers = scope.ServiceProvider
            .GetServices(handlersType)
            .Select(handler => handler as IBaseParallelNotificationHandler);
        var tasks = handlers
            .Select(handler => handler!.Handle(message));
        await Task
            .WhenAll(tasks);
    }

    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message) =>
        await GetResponseAsync(message, CancellationToken.None);
    public async Task<TResponse> GetResponseAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken) =>
        await HandleResponseMessageAsync(message, cancellationToken);

    private void HandleAccumulatorQueueMessage<TMessage>(TMessage message)
        where TMessage : IAccumulatorQueueMessage
    {
        var services = serviceProvider.GetServices<IAccumulatorQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }

    private void HandleDebounceQueueMessage<TMessage>(TMessage message)
        where TMessage : IDebounceQueueMessage
    {
        var services = serviceProvider.GetServices<IDebounceQueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }

    private void HandleQueueMessage<TMessage>(TMessage message)
        where TMessage : IQueueMessage
    {
        var services = serviceProvider.GetServices<QueueBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Enqueue(message);
    }

    private void HandleStackMessage<TMessage>(TMessage message)
        where TMessage : IStackMessage
    {
        var services = serviceProvider.GetServices<StackBackgroundService<TMessage>>();
        foreach (var service in services)
            service.Push(message);
    }

    private async Task<TResponse> HandleResponseMessageAsync<TResponse>(IResponseMessage<TResponse> message,
        CancellationToken cancellationToken)
    {
        RequestTypes.Response.DependencyInjection.responseHandlers.TryGetValue(message.GetType(),
            out var handler);

        return await ((BaseResponseHandlerWrapper<TResponse>)handler!)
            .HandleAsync(message, serviceProvider, cancellationToken);
    }
}
