using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.AccumulatorQueue;

internal static class DependencyInjection
{
    internal static void AddAccumulatorQueueHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IAccumulatorQueueHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            services.AddAccumulatorQueueHandler(options, handler, handlerType);
        }
    }

    internal static void AddAccumulatorQueueHandler(this IServiceCollection services,
        MediatorCoreOptions options, Type handler, Type? handlerType = null)
    {
        handlerType ??= typeof(IAccumulatorQueueHandler<,>);
        var handlerInterfaces = handler.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

        foreach (var item in handlerInterfaces)
        {
            var args = item.GetGenericArguments();
            var messageType = args[0];
            var optionsType = args[1];

            var handlerInterface = typeof(IBaseAccumulatorQueue<>)
                .MakeGenericType(messageType);

            if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                continue;

            var serviceType = typeof(AccumulatorQueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IAccumulatorQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                options.HandlersLifetime));
        }
    }
}