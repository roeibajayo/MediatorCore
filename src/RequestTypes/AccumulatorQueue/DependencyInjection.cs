using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.AccumulatorQueue;

internal static class DependencyInjection
{
    internal static void AddAccumulatorQueueHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IAccumulatorQueueHandler<,>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var args = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAccumulatorQueueHandler<,>))
                .GetGenericArguments();

            var messageType = args[0];
            var optionsType = args[1];

            var serviceType = typeof(AccumulatorQueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IAccumulatorQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

            var handlerInterface = typeof(IBaseAccumulatorQueue<>)
                .MakeGenericType(messageType);

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}