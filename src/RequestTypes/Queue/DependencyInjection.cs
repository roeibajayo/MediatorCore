using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.Queue;

internal static class DependencyInjection
{
    internal static void AddQueueHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IQueueHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

            foreach (var item in handlerInterfaces)
            {
                var args = item.GetGenericArguments();
                var messageType = args[0];
                var optionsType = args[1];
                var handlerInterface = typeof(IBaseQueueHandler<>)
                    .MakeGenericType(messageType);

                if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                    continue;

                var serviceType = typeof(QueueBackgroundService<,>)
                    .MakeGenericType(messageType, optionsType);
                var serviceInterface = typeof(IQueueBackgroundService<>)
                    .MakeGenericType(messageType);
                services.AddSingleton(serviceInterface, serviceType);
                services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

                services.Add(new ServiceDescriptor(handlerInterface,
                    handler,
                    options.HandlersLifetime));
            }
        }
    }
}