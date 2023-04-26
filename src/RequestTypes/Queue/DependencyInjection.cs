using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.Queue;

internal static class DependencyInjection
{
    internal static void AddQueueHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IQueueHandler<>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var messageType = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueueHandler<>))
                .GetGenericArguments()
                .First();

            var serviceType = typeof(QueueBackgroundService<>).MakeGenericType(messageType);
            services.AddSingleton(serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceType) as IHostedService);

            var handlerInterface = typeof(IQueueHandler<>).MakeGenericType(messageType);
            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}