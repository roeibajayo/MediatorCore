using MediatorCore.Publisher;
using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMediatorCore<TMarker>(this IServiceCollection services)
    {
        services.AddSingleton<IPublisher, MessageBusPublisher>();
        services.AddSingleton<TaskRunnerBackgroundService>();
        services.AddTransient<IHostedService>((s) => s.GetService<TaskRunnerBackgroundService>()!);

        services.AddAccumulatorQueueHandlers<TMarker>();
        services.AddFireAndForgetHandlers<TMarker>();
        services.AddNotificationsHandlers<TMarker>();
        services.AddQueueHandlers<TMarker>();
        services.AddStackHandlers<TMarker>();
        services.AddResponseHandlers<TMarker>();

        return services;
    }
}