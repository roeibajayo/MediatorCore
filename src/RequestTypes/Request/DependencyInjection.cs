using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.Request;

internal static class DependencyInjection
{
    internal static void AddRequestHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlers = AssemblyExtentions.GetAllInherits(typeof(IRequestHandler<>), assemblies: assemblies);
        foreach (var handler in handlers)
        {
            var messageType = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
                .GetGenericArguments()
                .First();

            var handlerInterface = typeof(IRequestHandler<>).MakeGenericType(messageType);
            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}