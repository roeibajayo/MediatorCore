using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.DebounceQueue;

internal static class DependencyInjection
{
    internal static void AddDebounceQueueHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IDebounceQueueHandler<,>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var args = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDebounceQueueHandler<,>))
                .GetGenericArguments();

            var messageType = args[0];
            var optionsType = args[1];

            var serviceType = typeof(DebounceQueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IDebounceQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

            var handlerInterface = typeof(IBaseDebounceQueue<>)
                .MakeGenericType(messageType);
            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}