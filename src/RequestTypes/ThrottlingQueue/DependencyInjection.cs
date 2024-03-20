using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

internal static class DependencyInjection
{
    internal static void AddThrottlingQueueHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IThrottlingQueueHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            services.AddThrottlingQueueHandler(options, handler, handlerType);
        }
    }

    internal static void AddThrottlingQueueHandler(this IServiceCollection services,
        MediatorCoreOptions options, Type handler, Type? handlerType = null)
    {
        handlerType ??= typeof(IThrottlingQueueHandler<,>);

        var handlerInterfaces = handler.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

        foreach (var item in handlerInterfaces)
        {
            var args = item.GetGenericArguments();
            var messageType = args[0];
            var optionsType = args[1];
            var handlerInterface = typeof(IBaseThrottlingQueueHandler<>)
                .MakeGenericType(messageType);

            if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                continue;

            var serviceType = typeof(ThrottlingQueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IThrottlingQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                options.HandlersLifetime));
        }
    }
}