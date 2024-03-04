using MediatorCore;
using MediatorCore.Publisher;
using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.BubblingNotification;
using MediatorCore.RequestTypes.DebounceQueue;
using MediatorCore.RequestTypes.Notification;
using MediatorCore.RequestTypes.Queue;
using MediatorCore.RequestTypes.Request;
using MediatorCore.RequestTypes.Response;
using MediatorCore.RequestTypes.Stack;
using MediatorCore.RequestTypes.ThrottlingQueue;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Add MediatorCore services from the calling assembly.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options">Global MediatorCore configuration (Optional).</param>
    /// <returns>The original <paramref name="services"/>.</returns>
    public static IServiceCollection AddMediatorCore(this IServiceCollection services,
        Action<MediatorCoreOptions>? options = null)
    {
        return AddMediatorCore(services, new[] { Assembly.GetCallingAssembly() }, options);
    }

    /// <summary>
    /// Add MediatorCore services from assembly that contains the <typeparamref name="TMarker"/> type.
    /// </summary>
    /// <typeparam name="TMarker">Marker of the assembly to register services from</typeparam>
    /// <param name="services"></param>
    /// <param name="options">Global MediatorCore configuration (Optional).</param>
    /// <returns>The original <paramref name="services"/>.</returns>
    public static IServiceCollection AddMediatorCore<TMarker>(this IServiceCollection services,
        Action<MediatorCoreOptions>? options = null)
    {
        return AddMediatorCore(services, new[] { typeof(TMarker).Assembly }, options);
    }

    /// <summary>
    /// Add MediatorCore services from the exected assembly.
    /// </summary>
    /// <param name="assemblies">Array of assemblies to register handlers from.</param>
    /// <param name="services"></param>
    /// <param name="options">Global MediatorCore configuration (Optional).</param>
    /// <returns>The original <paramref name="services"/>.</returns>
    public static IServiceCollection AddMediatorCore(this IServiceCollection services,
        Assembly[] assemblies,
        Action<MediatorCoreOptions>? options = null)
    {
        if (assemblies is null)
            throw new ArgumentNullException(nameof(assemblies));

        TryRemoveMediatorCore(services);

        MediatorCoreOptions.instance = new MediatorCoreOptions();
        options?.Invoke(MediatorCoreOptions.instance);

        services.AddSingleton<IPublisher, MessageBusPublisher>();

        var assembliesToAdd = assemblies
            .Distinct()
            .Where(assembly => assemblies is not null)
            .ToArray();

        services.AddAccumulatorQueueHandlers(assembliesToAdd);
        services.AddBubblingNotificationHandlers(assembliesToAdd);
        services.AddDebounceQueueHandlers(assembliesToAdd);
        services.AddNotificationHandlers(assembliesToAdd);
        services.AddQueueHandlers(assembliesToAdd);
        services.AddRequestHandlers(assembliesToAdd);
        services.AddResponseHandlers(assembliesToAdd);
        services.AddStackHandlers(assembliesToAdd);
        services.AddThrottlingQueueHandlers(assembliesToAdd);

        return services;
    }

    private static void TryRemoveMediatorCore(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPublisher));
        if (descriptor is null)
            return;

        services.Remove(descriptor);

        services.RemoveAll(typeof(IAccumulatorQueueBackgroundService<>));
        services.RemoveAll(typeof(IDebounceQueueBackgroundService<>));
        services.RemoveAll(typeof(IQueueBackgroundService<>));
        services.RemoveAll(typeof(IStackBackgroundService<>));
        services.RemoveAll(typeof(IThrottlingQueueBackgroundService<>));

        services.RemoveAll(typeof(IAccumulatorQueueHandler<,>));
        services.RemoveAll(typeof(IBubblingNotificationHandler<,>));
        services.RemoveAll(typeof(IDebounceQueueHandler<,>));
        services.RemoveAll(typeof(INotificationHandler<>));
        services.RemoveAll(typeof(IQueueHandler<,>));
        services.RemoveAll(typeof(IRequestHandler<>));
        services.RemoveAll(typeof(IResponseHandler<,>));
        services.RemoveAll(typeof(IStackHandler<,>));
        services.RemoveAll(typeof(IThrottlingQueueHandler<,>));
    }
}