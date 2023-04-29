using MediatorCore.Infrastructure;
using MediatorCore.RequestTypes.Notification;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.BubblingNotification;

internal static class DependencyInjection
{
    internal static void AddBubblingNotificationHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var notificaitonHandler = typeof(IBubblingNotificationHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(notificaitonHandler, assemblies: assemblies);
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
        }

        foreach (var foundHandlers in orders)
        {
            foreach (var handler in foundHandlers.Value.OrderBy(x => x.Item2.Sort).Select(x => x.Item1))
            {
                services.Add(new ServiceDescriptor(typeof(IBaseBubblingNotification<>).MakeGenericType(foundHandlers.Key),
                    handler,
                    MediatorCoreOptions.instance.HandlersLifetime));
            }
        }
    }
}