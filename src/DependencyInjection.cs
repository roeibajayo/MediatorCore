using MediatorCore;
using MediatorCore.Publisher;
using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.DebounceQueue;
using MediatorCore.RequestTypes.FireAndForget;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using MediatorCore.RequestTypes.ThrottlingQueue;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{

    /// <summary>
    /// Add MediatorCore services.
    /// </summary>
    /// <typeparam name="TMarker"></typeparam>
    /// <param name="services"></param>
    /// <param name="options">Global MediatorCore configuration</param>
    public static IServiceCollection AddMediatorCore<TMarker>(this IServiceCollection services,
        Action<MediatorCoreOptions>? options = null)
    {
        if (!services.Any(x => x.ServiceType == typeof(IPublisher)))
        {
            MediatorCoreOptions.instance = new MediatorCoreOptions();
            options?.Invoke(MediatorCoreOptions.instance);

            services.Add(new ServiceDescriptor(typeof(IPublisher),
                typeof(MessageBusPublisher),
                MediatorCoreOptions.instance.HandlersLifetime));
        }

        services.AddAccumulatorQueueHandlers<TMarker>();
        services.AddDebounceQueueHandlers<TMarker>();
        services.AddFireAndForgetHandlers<TMarker>();
        services.AddNotificationsHandlers<TMarker>();
        services.AddQueueHandlers<TMarker>();
        services.AddStackHandlers<TMarker>();
        services.AddResponseHandlers<TMarker>();
        services.AddThrottlingQueueHandlers<TMarker>();

        return services;
    }
}