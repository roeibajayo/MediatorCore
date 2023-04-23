using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorCore.RequestTypes.Stack;

internal static class DependencyInjection
{
    internal static void AddStackHandlers<TMarker>(this IServiceCollection services)
    {
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(typeof(IStackHandler<>), typeof(TMarker));
        foreach (var handler in handlers)
        {
            var messageType = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IStackHandler<>))
                .GetGenericArguments()
                .First();

            var serviceType = typeof(StackBackgroundService<>).MakeGenericType(messageType);
            services.AddSingleton(serviceType);
            services.AddSingleton(s => s.GetRequiredService(serviceType) as IHostedService);

            var handlerInterface = typeof(IStackHandler<>).MakeGenericType(messageType);
            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}