using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.Queue;

internal static class DependencyInjection
{
    internal static void AddQueueHandlers<TMarker>(this IServiceCollection services)
    {
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(typeof(IQueueHandler<>), typeof(TMarker));
        foreach (var handler in handlers)
        {
            var messageType = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueueHandler<>))
                .GetGenericArguments()
                .First();

            var serviceType = typeof(QueueBackgroundService<>).MakeGenericType(messageType);
            services.AddSingleton(serviceType);
            services.AddTransient(s => s.GetRequiredService(serviceType) as IHostedService);

            var handlerInterface = typeof(IQueueHandler<>).MakeGenericType(messageType);
            services.AddScoped(handlerInterface, handler);
        }
    }
}