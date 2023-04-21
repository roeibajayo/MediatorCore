using MediatorCore.Publisher;
using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.DebounceQueue;
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
        if (!services.Any(x => x.ServiceType == typeof(IPublisher)))
        {
            services.AddSingleton<IPublisher, MessageBusPublisher>();
            services.AddSingleton<TaskRunnerBackgroundService>();
            services.AddTransient<IHostedService>((s) => s.GetService<TaskRunnerBackgroundService>()!);
            services.AddMediatorCore<IPublisher>();
        }

        services.AddAccumulatorQueueHandlers<TMarker>();
        services.AddDebounceQueueHandlers<TMarker>();
        services.AddFireAndForgetHandlers<TMarker>();
        services.AddNotificationsHandlers<TMarker>();
        services.AddQueueHandlers<TMarker>();
        services.AddStackHandlers<TMarker>();
        services.AddResponseHandlers<TMarker>();

        return services;
    }
}