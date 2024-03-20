using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.Request;

internal static class DependencyInjection
{
    internal static void AddRequestHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IRequestHandler<>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            services.AddRequestHandler(options, handler, handlerType);
        }
    }

    internal static void AddRequestHandler(this IServiceCollection services,
        MediatorCoreOptions options, Type handler, Type? handlerType = null)
    {
        handlerType ??= typeof(IRequestHandler<>);
        var handlerInterfaces = handler.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

        foreach (var handlerInterface in handlerInterfaces)
        {
            if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                continue;

            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                options.HandlersLifetime));
        }
    }
}