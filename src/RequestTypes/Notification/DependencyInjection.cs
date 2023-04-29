using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.Notification;

internal static class DependencyInjection
{
    internal static void AddNotificationHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var notificaitonHandler = typeof(INotificationHandler<>);
        var handlers = AssemblyExtentions.GetAllInherits(notificaitonHandler, assemblies: assemblies);
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