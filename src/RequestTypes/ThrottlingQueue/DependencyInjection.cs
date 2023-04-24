using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.ThrottlingQueue;

internal static class DependencyInjection
{
    internal static void AddThrottlingQueueHandlers<TMarker>(this IServiceCollection services)
    {
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(typeof(IThrottlingQueueHandler<,>), typeof(TMarker));
        foreach (var handler in handlers)
        {
            var args = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IThrottlingQueueHandler<,>))
                .GetGenericArguments();

            var messageType = args[0];
            var optionsType = args[1];

            var serviceType = typeof(ThrottlingQueueBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IThrottlingQueueBackgroundService<>)
                .MakeGenericType(messageType);
            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

            var handlerInterface = typeof(IBaseThrottlingQueue<>)
                .MakeGenericType(messageType);

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}