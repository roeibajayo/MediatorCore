using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.Queue;

internal static class DependencyInjection
{
    internal static void AddQueueHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IQueueHandler<,>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var args = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueueHandler<,>))
                .GetGenericArguments();

            var messageType = args[0];
            var optionsType = args[1];

            var serviceType = typeof(QueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

            var handlerInterface = typeof(IBaseQueueHandler<>)
                .MakeGenericType(messageType);

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}