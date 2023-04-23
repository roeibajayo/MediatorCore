using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.RequestTypes.Notification;

internal static class DependencyInjection
{
    internal static void AddNotificationsHandlers<TMarker>(this IServiceCollection services)
    {
        services.AddBubblingNotificationHandlers<TMarker>();
        services.AddParallelNotificationHandlers<TMarker>();
    }

    internal static IDictionary<Type, Type[]> _bubblingHandlers =
        new Dictionary<Type, Type[]>();

    private static void AddBubblingNotificationHandlers<TMarker>(this IServiceCollection services)
    {
        var notificaitonHandler = typeof(IBubblingNotificationHandler<,>);
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(notificaitonHandler, typeof(TMarker));
        var orders = new Dictionary<Type, List<(Type, IBubblingNotificationOptions)>>();
        foreach (var handler in handlers)
        {
            var handlerInterface = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == notificaitonHandler);
            var handlerArgs = handlerInterface
                .GetGenericArguments();

            var messageType = handlerArgs[0];
            var optionsType = handlerArgs[1];
            var options = Activator.CreateInstance(optionsType) as IBubblingNotificationOptions;

            if (orders.TryGetValue(messageType, out var list))
            {
                list.Add((handler, options!));
            }
            else
            {
                orders.Add(messageType,
                    new List<(Type, IBubblingNotificationOptions)> { (handler, options!) });
            }

            services.Add(new ServiceDescriptor(handler,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }

        _bubblingHandlers = orders.ToDictionary(x => x.Key,
            x => x.Value.OrderBy(y => y.Item2.Sort).Select(y => y.Item1).ToArray());
    }
    private static void AddParallelNotificationHandlers<TMarker>(this IServiceCollection services)
    {
        var notificaitonHandler = typeof(IParallelNotificationHandler<>);
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(notificaitonHandler, typeof(TMarker));
        foreach (var handler in handlers)
        {
            var handlerInterface = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == notificaitonHandler);
            var messageType = handlerInterface
                .GetGenericArguments()
                .First();

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}