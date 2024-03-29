using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.BubblingNotification;

internal static class DependencyInjection
{
    internal static void AddBubblingNotificationHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IBubblingNotificationHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        var orders = new Dictionary<Type, List<(Type, BubblingNotificationOptions)>>();
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

            foreach (var handlerInterface in handlerInterfaces)
            {
                var handlerArgs = handlerInterface
                    .GetGenericArguments();

                var messageType = handlerArgs[0];
                var optionsType = handlerArgs[1];
                var notificationOptions = Activator.CreateInstance(optionsType) as BubblingNotificationOptions;

                if (orders.TryGetValue(messageType, out var list))
                {
                    list.Add((handler, notificationOptions!));
                }
                else
                {
                    orders.Add(messageType, [(handler, notificationOptions!)]);
                }
            }
        }

        foreach (var foundHandlers in orders)
        {
            foreach (var handler in foundHandlers.Value.OrderBy(x => x.Item2.Sort).Select(x => x.Item1))
            {
                var handlerInterface = typeof(IBaseBubblingNotification<>).MakeGenericType(foundHandlers.Key);

                if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                    continue;

                services.Add(new ServiceDescriptor(handlerInterface,
                    handler,
                    options.HandlersLifetime));
            }
        }
    }

    internal static void AddBubblingNotificationHandler(this IServiceCollection services,
        MediatorCoreOptions options, Type handler)
    {
        var handlerType = typeof(IBubblingNotificationHandler<,>);
        var handlerInterfaces = handler.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

        foreach (var handlerInterface in handlerInterfaces)
        {
            var handlerArgs = handlerInterface
                .GetGenericArguments();

            var messageType = handlerArgs[0];
            var optionsType = handlerArgs[1];
            var notificationOptions = Activator.CreateInstance(optionsType) as BubblingNotificationOptions;
            var serviceInterface = typeof(IBaseBubblingNotification<>).MakeGenericType(messageType);
            var alreadyRegistred = services
                .Where(x => x.ServiceType.IsGenericType && x.ServiceType == serviceInterface)
                .ToArray();

            for (var i = 0; i < alreadyRegistred.Length; i++)
            {
                services.Remove(alreadyRegistred[i]);
            }

            var orders = alreadyRegistred
                .Select(x => x.ImplementationType)
                .Concat([handler])
                .ToArray();

            foreach (var found in orders.OrderBy(x => (Activator.CreateInstance(x!) as BubblingNotificationOptions)!.Sort))
            {
                services.Add(new ServiceDescriptor(handlerInterface,
                    found!,
                    options.HandlersLifetime));
            }
        }
    }
}