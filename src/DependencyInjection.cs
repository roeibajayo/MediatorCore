using MediatorCore.Publisher;
using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore;

public static class DependencyInjection
{
    public static void AddMediatorCore<TMarker>(this IServiceCollection services)
    {
        services.AddSingleton<IPublisher, MessageBusPublisher>();
        services.AddSingleton<TaskRunnerBackgroundService>();
        services.AddTransient<IHostedService>((s) => s.GetService<TaskRunnerBackgroundService>()!);

        services.AddAccumulatorQueueHandlers<TMarker>();
        services.AddFireAndForgetHandlers<TMarker>();
        services.AddNotificationsHandlers<TMarker>();
        services.AddQueueHandlers<TMarker>();
        services.AddResponseHandlers<TMarker>();
    }
}