using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

internal static class DependencyInjection
{
    internal static void AddThrottlingQueueHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IThrottlingQueueHandler<,>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IThrottlingQueueHandler<,>));

            foreach (var item in handlerInterfaces)
            {
                var args = item.GetGenericArguments();
                var messageType = args[0];
                var optionsType = args[1];

                var serviceType = typeof(ThrottlingQueueBackgroundService<,>)
                    .MakeGenericType(messageType, optionsType);
                var serviceInterface = typeof(IThrottlingQueueBackgroundService<>)
                    .MakeGenericType(messageType);
                services.AddSingleton(serviceInterface, serviceType);
                services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

                var handlerInterface = typeof(IBaseThrottlingQueueHandler<>)
                    .MakeGenericType(messageType);

                services.Add(new ServiceDescriptor(handlerInterface,
                    handler,
                    MediatorCoreOptions.instance.HandlersLifetime));
            }
        }
    }
}