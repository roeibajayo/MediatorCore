using MediatorCore;
using MediatorCore.Publisher;
using MediatorCore.RequestTypes.AccumulatorQueue;
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
    private static readonly HashSet<string> registredAssemblies = new();

    /// <summary>
    /// Add MediatorCore services from the calling assembly.
    /// </summary>
    /// <typeparam name="TMarker">Marker of the assembly to register services from</typeparam>
    /// <param name="services"></param>
    /// <param name="options">Global MediatorCore configuration</param>
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
    /// <param name="options">Global MediatorCore configuration</param>
    public static IServiceCollection AddMediatorCore<TMarker>(this IServiceCollection services,
        Action<MediatorCoreOptions>? options = null)
    {
        return AddMediatorCore(services, new[] { typeof(TMarker).Assembly }, options);
    }

    /// <summary>
    /// Add MediatorCore services from the exected assembly.
    /// </summary>
    /// <typeparam name="TMarker">Marker of the assembly to register services from</typeparam>
    /// <param name="services"></param>
    /// <param name="options">Global MediatorCore configuration</param>
    public static IServiceCollection AddMediatorCore(this IServiceCollection services,
        Assembly[] assemblies,
        Action<MediatorCoreOptions>? options = null)
    {
        var assembliesToAdd = assemblies
            .Where(assembly => !registredAssemblies.Contains(assembly.FullName!))
            .ToArray();

        if (registredAssemblies.Count == 0)
        {
            MediatorCoreOptions.instance = new MediatorCoreOptions();
            options?.Invoke(MediatorCoreOptions.instance);

            services.Add(new ServiceDescriptor(typeof(IPublisher),
                typeof(MessageBusPublisher),
                MediatorCoreOptions.instance.HandlersLifetime));
        }

        services.AddAccumulatorQueueHandlers(assembliesToAdd);
        services.AddDebounceQueueHandlers(assembliesToAdd);
        services.AddNotificationsHandlers(assembliesToAdd);
        services.AddQueueHandlers(assembliesToAdd);
        services.AddRequestHandlers(assembliesToAdd);
        services.AddResponseHandlers(assembliesToAdd);
        services.AddStackHandlers(assembliesToAdd);
        services.AddThrottlingQueueHandlers(assembliesToAdd);

        foreach (var assembly in assembliesToAdd)
            registredAssemblies.Add(assembly.FullName!);

        return services;
    }
}