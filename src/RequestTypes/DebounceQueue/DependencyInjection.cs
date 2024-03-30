using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.DebounceQueue;

internal static class DependencyInjection
{
    internal static void AddDebounceQueueHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IDebounceQueueHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            services.AddDebounceQueueHandler(options, handler, handlerType);
        }
    }
    internal static void AddDebounceQueueHandler(this IServiceCollection services,
        MediatorCoreOptions options, Type handler, Type? handlerType = null)
    {
        handlerType ??= typeof(IDebounceQueueHandler<,>);
        var handlerInterfaces = handler.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

        foreach (var item in handlerInterfaces)
        {
            var args = item.GetGenericArguments();
            var messageType = args[0];
            var optionsType = args[1];
            var handlerInterface = typeof(IBaseDebounceQueue<>)
                .MakeGenericType(messageType);

            if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                continue;

            var serviceType = typeof(DebounceQueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IDebounceQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => (IHostedService)s.GetRequiredService(serviceInterface));

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                options.HandlersLifetime));
        }
    }
}