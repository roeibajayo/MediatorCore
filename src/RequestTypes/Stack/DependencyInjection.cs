using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.Stack;

internal static class DependencyInjection
{
    internal static void AddStackHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IStackHandler<,>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IStackHandler<,>));

            foreach (var item in handlerInterfaces)
            {
                var args = item.GetGenericArguments();
                var messageType = args[0];
                var optionsType = args[1];

                var serviceType = typeof(StackBackgroundService<,>)
                    .MakeGenericType(messageType, optionsType);
                var serviceInterface = typeof(IStackBackgroundService<>)
                    .MakeGenericType(messageType);
                services.AddSingleton(serviceInterface, serviceType);
                services.AddSingleton(s => s.GetRequiredService(serviceInterface) as IHostedService);

                var handlerInterface = typeof(IBaseStackHandler<>)
                    .MakeGenericType(messageType);

                services.Add(new ServiceDescriptor(handlerInterface,
                    handler,
                    MediatorCoreOptions.instance.HandlersLifetime));
            }
        }
    }
}