using MediatorCore;
using MediatorCore.Infrastructure;
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
        return AddMediatorCore(services, [Assembly.GetCallingAssembly(), .. Assembly.GetCallingAssembly().GetAllReferencedAssemblies()], options);
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
        return AddMediatorCore(services, [typeof(TMarker).Assembly], options);
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

        var typesToRemove = new[]
        {
            typeof(IAccumulatorQueueBackgroundService<>),
            typeof(IAccumulatorQueueHandler<,>),
            typeof(IBaseAccumulatorQueue<>),

            typeof(IBubblingNotificationHandler<,>),
            typeof(IBaseBubblingNotification<>),

            typeof(IDebounceQueueBackgroundService<>),
            typeof(IDebounceQueueHandler<,>),

            typeof(INotificationHandler<>),

            typeof(IQueueBackgroundService<>),
            typeof(IQueueHandler<,>),

            typeof(IRequestHandler<>),
            typeof(IResponseHandler<,>),

            typeof(IStackBackgroundService<>),
            typeof(IStackHandler<,>),

            typeof(IThrottlingQueueBackgroundService<>),
            typeof(IThrottlingQueueHandler<,>)
        };

        var removed = new List<ServiceDescriptor>();
        foreach (var service in services)
        {
            if (service.ServiceType.IsGenericType && typesToRemove.Contains(service.ServiceType.GetGenericTypeDefinition()))
                removed.Add(service);
        }

        foreach (var service in removed)
            services.Remove(service);
    }
}