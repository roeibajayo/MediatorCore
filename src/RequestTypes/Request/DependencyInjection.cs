using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.RequestTypes.Request;

internal static class DependencyInjection
{
    internal static void AddRequestHandlers<TMarker>(this IServiceCollection services)
    {
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(typeof(IRequestHandler<>), typeof(TMarker));
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